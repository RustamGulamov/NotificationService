using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using NotificationService.Data.Exceptions;
using NotificationService.Data.Extensions;
using NotificationService.Data.Models;
using NotificationService.Data.Repositories;
using NotificationService.Data.Validators;

namespace NotificationService.Data
{
    /// <summary>
    /// Менеджер шаблонов сообщений. Содержит бизнес логику по обработке шаблонов перед добавлением их в базу.
    /// </summary>
    public class TemplateManager : ITemplateManager
    {
        private const string EnginesShouldMatchMessage = "The engine types of parent and child templates should be match";

        private readonly IMessageTemplatesRepository repository;
        private readonly ILogger<TemplateManager> logger;

        /// <summary>
        /// Конструктор. Получает и сохраняет экземпляр репозитория.
        /// </summary>
        /// <param name="repository">Репозиторий с шаблонами сообщений <seealso cref="IMessageTemplatesRepository"/>.</param>
        /// <param name="logger">Для логирования событий.</param>
        public TemplateManager(IMessageTemplatesRepository repository, ILogger<TemplateManager> logger)
        {
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task AddTemplate(MessageTemplate template)
        {
            template.ThrowIfNull(nameof(template));
            template.Name = template.Name.ToLower();

            if (await repository.Get(template.Name) != null)
            {
                string errorMessage = $"Template with the given name {template.Name} already exists";
                logger.LogWarning(errorMessage);
                throw new ArgumentException(errorMessage);
            }
            
            await VerifyParent(template);
            await repository.Add(template);
        }

        /// <inheritdoc />
        public async Task<bool> DeleteTemplate(string templateName)
        {
            templateName.ThrowIfNullOrEmty(nameof(templateName));
            templateName = templateName.ToLower();

            await VerifyExistence(templateName);
            VerifyChildExistence(templateName);
            
            DeleteResult result = await repository.Delete(templateName);
            return result.IsAcknowledged && result.DeletedCount == 1;
        }

        /// <inheritdoc />
        public async Task<bool> DeleteAllTemplates()
        {
            DeleteResult result = await repository.DeleteAll();
            return result.IsAcknowledged;
        }

        /// <inheritdoc />
        public async Task<bool> UpdateTemplate(MessageTemplate template)
        {
            template.ThrowIfNull(nameof(template));
            template.Name = template.Name.ToLower();
            
            await VerifyExistence(template.Name);
            await VerifyParent(template);
            await VerifyChildEngines(template);
            
            UpdateResult result = await repository.Update(template);
            return result.IsAcknowledged;
        }

        /// <inheritdoc />
        public async Task<MessageTemplate> GetTemplate(string templateName)
        {
            templateName.ThrowIfNullOrEmty(nameof(templateName));
            return await VerifyExistence(templateName.ToLower());
        }

        /// <inheritdoc />
        public AllTemplates GetTemplatesPaged(PageInfo pageInfo)
        {
            IQueryable<MessageTemplate> templates = repository.GetAll();
            int total = templates.Count();

            if (total == 0 || pageInfo == null || pageInfo.PageSize == 0 && pageInfo.CurrentPage == 0)
            {
                logger.LogInformation("Returning default page");

                return new AllTemplates
                {
                    TotalTemplates = total,
                    PageSize = total > 0 ? total : 10,
                    Templates = templates.AsEnumerable(),
                    CurrentPage = 1,
                    PagesCount = 1
                };
            }

            AllTemplates result = new AllTemplates
            {
                TotalTemplates = total,
                PageSize = pageInfo.PageSize,
                CurrentPage = pageInfo.CurrentPage,
                Templates = templates.Skip(--pageInfo.CurrentPage * pageInfo.PageSize)
                    .Take(pageInfo.PageSize)
                    .AsEnumerable()
            };
            
            ValidationResult validationResult = new AllTemplatesValidator().Validate(result);
            return validationResult.IsValid ? result : throw new ArgumentException(validationResult.GetAllErrors());
        }

        /// <summary>
        /// Проверяет существование шаблона в репозитории.
        /// </summary>
        /// <param name="templateName">Название шаблона.</param>
        /// <returns>Возвращает найденный шаблон.</returns>
        private async Task<MessageTemplate> VerifyExistence(string templateName)
        {
            logger.LogInformation($"Verifying existence of template named {templateName}");

            MessageTemplate template = await repository.Get(templateName);

            if (template == null)
            {
                string errorMessage = $"Couldn't found template named {templateName}";
                logger.LogError(errorMessage);
                throw new TemplateNotFoundException(errorMessage);
            }

            return template;
        }
        
        /// <summary>
        /// Проверяет существование родительского шаблона и отсутствие родителей у родительского шаблона.
        /// А также проверяет идентичность типов шаблонов у дочернего и родительского элементов.
        /// </summary>
        /// <param name="template">Проверяемый шаблон.</param>
        private async Task VerifyParent(MessageTemplate template)
        {
            logger.LogInformation($"Verifying parent templates of {template.Name}");

            if (string.IsNullOrEmpty(template.Parent))
            {
                logger.LogInformation("Template doesn't contain parents");
                return;
            }

            template.Parent = template.Parent.ToLower();
            MessageTemplate parent = await repository.Get(template.Parent);

            if (parent == null)
            {
                string errorMessage = $"Couldn't found parent template named {template.Parent}";
                logger.LogError(errorMessage);
                throw new TemplateNotFoundException(errorMessage);
            }

            if (!string.IsNullOrEmpty(parent.Parent))
            {
                string errorMessage = $"Parent template {template.Parent} shouldn't have own parents";
                logger.LogError(errorMessage);
                throw new ArgumentException(errorMessage);
            }

            if (template.EngineType != parent.EngineType)
            {
                logger.LogError(EnginesShouldMatchMessage);
                throw new ArgumentException(EnginesShouldMatchMessage);
            }
        }

        /// <summary>
        /// Проверяет соответствие движков дочерних шаблонов с родительским.
        /// </summary>
        /// <param name="template">Проверяемая сущность.</param>
        private async Task VerifyChildEngines(MessageTemplate template)
        {
            logger.LogInformation($"Verifying child templates engine types of {template.Name}");

            if (!string.IsNullOrEmpty(template.Parent))
            {
                logger.LogInformation("Template doesn't contain child templates");
                return;
            }

            List<MessageTemplate> childTemplates = await repository.GetChildTemplates(template.Name);
            int childsWithIncorrectEngines = childTemplates.Count(child => 
                    child.EngineType != template.EngineType);

            if (childsWithIncorrectEngines > 0)
            {
                logger.LogError(EnginesShouldMatchMessage);
                throw new ArgumentException(EnginesShouldMatchMessage);
            }
        }

        /// <summary>
        /// Производит проверку дочерних шаблонов у родительского.
        /// </summary>
        /// <param name="templateName">Имя шаблона.</param>
        private void VerifyChildExistence(string templateName)
        {
            int childsCount = repository.GetAll().Count(element => templateName.Equals(element.Parent));
            if (childsCount > 0)
            {
                string errorMessage = "Can't delete parent template with linked child templates";
                logger.LogError(errorMessage);
                throw new ArgumentException(errorMessage);
            }
        }
    }
}
