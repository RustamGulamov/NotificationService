using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using NotificationService.Web.Extensions;

namespace NotificationService.Web.Filters
{
    /// <summary>
    /// Фильтр для форматирования всех ответов Web API.
    /// </summary>
    public class FormatResponseFilter : IActionFilter
    {
        private readonly ILogger<FormatResponseFilter> logger;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="logger">Для логирования событий.</param>
        public FormatResponseFilter(ILogger<FormatResponseFilter> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public void OnActionExecuting(ActionExecutingContext context)
        {
            logger.LogInformation($"Executing: {context?.ActionDescriptor?.DisplayName} " +
                $"with arguments: {string.Join(" ,", context?.ActionArguments?.Values)}");
        }

        /// <inheritdoc />
        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Result != null)
            {
                context.Result = context.Result.ToFormattedResponse(context.HttpContext.Response.StatusCode);
            }
        }
    }
}
