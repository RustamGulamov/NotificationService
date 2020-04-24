using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NotificationService.Data.Extensions;
using NotificationService.Data.Models;
using RazorLight;

namespace NotificationService.Logic.MessageTemplateEngines
{
    /// <summary>
    /// Razor шаблонизатор.
    /// </summary>
    public class RazorTemplateEngine : ITemplateEngine
    {
        private const string RenderBody = "@RenderBody()";

        private readonly RazorLightEngine engine;
        private readonly ILogger<RazorTemplateEngine> logger;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="logger">Для логирования событий.</param>
        public RazorTemplateEngine(ILogger<RazorTemplateEngine> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            engine = new RazorLightEngineBuilder()
                .UseMemoryCachingProvider()
                .Build();
        }

        /// <inheritdoc />
        public void AddParentTemplate(MessageTemplate parentTemplate)
        {
            if (parentTemplate != null && !string.IsNullOrEmpty(parentTemplate.Name))
            {
                engine.Options.DynamicTemplates[parentTemplate.Name] = parentTemplate.Body;
                logger.LogInformation($"Parent template {parentTemplate.Name} added to Razor Engine .");
            }
        }

        /// <inheritdoc />
        public Task<string> RenderAsync<T>(MessageTemplate template, T model)
        {
            template.ThrowIfNull(nameof(template));

            if (template.EngineType != Engines.Razor)
            {
                throw new ArgumentException($"Type template engine is not {Engines.Razor:G}");
            }

            ReplaceRenderBodyIfIsParentTemplateNull(template);

            return engine?.CompileRenderAsync(template.Name, template.Body, model);
        }

        private void ReplaceRenderBodyIfIsParentTemplateNull(MessageTemplate template)
        {
            if (string.IsNullOrEmpty(template.Parent))
            {
                template.Body = template.Body.Replace(RenderBody, string.Empty);
            }
        }
    }
}