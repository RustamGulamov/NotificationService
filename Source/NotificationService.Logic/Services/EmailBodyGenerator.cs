using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using NotificationService.Data;
using NotificationService.Data.Extensions;
using NotificationService.Data.Models;
using NotificationService.Logic.MessageTemplateEngines;
using NotificationService.Logic.Services.Interfaces;

namespace NotificationService.Logic.Services
{
    /// <summary>
    /// Класс который находит шаблон по имени, и генерирует текст на основе параметров.
    /// </summary>
    public class EmailBodyGenerator : IEmailBodyGenerator
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ITemplateManager templateManager;
        private readonly ILogger<EmailBodyGenerator> logger;

        /// <summary>
        /// Конструктор.    
        /// </summary>
        /// <param name="serviceProvider">Провайдер сервисов.</param>
        /// <param name="templateManager">Менеджер шаблонов сообщений.</param>
        /// <param name="logger">Для логирования событий.</param>
        public EmailBodyGenerator(IServiceProvider serviceProvider, ITemplateManager templateManager, ILogger<EmailBodyGenerator> logger)
        {
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            this.templateManager = templateManager ?? throw new ArgumentNullException(nameof(templateManager));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<string> GenerateEmailBody<T>(MessageTemplate template, T model)
        {
            template.ThrowIfNull(nameof(template));

            logger.LogInformation($"Start generate email body by template:{template.Name}, model: {model?.ToJson()}, parent template:{template.Parent}");

            var parentTemplate = await GetParentTemplate(template);

            VerifyTemplateAndParentTemplate(template, parentTemplate);

            ITemplateEngine templateEngine = GetTemplateEngine(template.EngineType);
            templateEngine.AddParentTemplate(parentTemplate);

            logger.LogInformation($"Starting async render template with model. Type template engine: {templateEngine.GetType().Name}");

            return await templateEngine.RenderAsync(template, model);
        }

        private ITemplateEngine GetTemplateEngine(Engines engine)
        {
            Type templateEngineType;

            switch (engine)
            {
                case Engines.Jinja: templateEngineType = typeof(JinjaTemplateEngine); break;
                case Engines.Razor: templateEngineType = typeof(RazorTemplateEngine); break;

                default: throw new NotImplementedException();
            }

            return GetServiceByType(templateEngineType);
        }

        private ITemplateEngine GetServiceByType(Type serviceType)
        {
            return serviceProvider.GetService(serviceType) as ITemplateEngine;
        }

        private async Task<MessageTemplate> GetParentTemplate(MessageTemplate template)
        {
            return !string.IsNullOrWhiteSpace(template.Parent) ? await templateManager.GetTemplate(template.Parent) : null;
        }

        private void VerifyTemplateAndParentTemplate(MessageTemplate template, MessageTemplate parentTemplate)
        {
            if (!string.IsNullOrEmpty(template.Parent) && parentTemplate == null)
            {
                throw new InvalidOperationException($"Failed to get parent template from repository for template {template.Name}.");
            }

            if (parentTemplate != null && parentTemplate.EngineType != template.EngineType)
            {
                throw new InvalidOperationException($"Template type {template.Name} does not match type of the parent template {template.Parent}.");
            }
        }
    }
}