using System;
using System.Threading.Tasks;
using DotLiquid;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NotificationService.Data.Extensions;
using NotificationService.Data.Models;
using NotificationService.Logic.Extensions;

namespace NotificationService.Logic.MessageTemplateEngines
{
    /// <summary>
    /// Jinja шаблонизатор.
    /// </summary>
    public class JinjaTemplateEngine : ITemplateEngine
    {
        private readonly ILogger<JinjaTemplateEngine> logger;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="logger">Для логирования событий.</param>
        public JinjaTemplateEngine(ILogger<JinjaTemplateEngine> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public void AddParentTemplate(MessageTemplate parentTemplate)
        {
            if (parentTemplate != null)
            {
                Template.FileSystem = new ParentTemplateFileSystem(parentTemplate);
                logger.LogInformation($"Parent template {parentTemplate.Name} added to Jinja Engine.");
            }
        }

        /// <inheritdoc />
        public Task<string> RenderAsync<T>(MessageTemplate template, T model)
        {
            template.ThrowIfNull(nameof(template));

            if (template.EngineType != Engines.Jinja)
            {
                throw new ArgumentException($"Type template engine is not {Engines.Jinja:G}");
            }
            
            var jsonObject = model as JObject;

            var hashVariables = jsonObject != null
                ? Hash.FromDictionary(jsonObject.ToDictionary())
                : Hash.FromAnonymousObject(model);
            
            return
                Task.Run(() =>
                    Template
                        .Parse(template.Body)
                        .Render(hashVariables));
        }
    }
}