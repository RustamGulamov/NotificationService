using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using NotificationService.Data.Extensions;

namespace NotificationService.Web.Extensions
{
    /// <summary>
    /// Методы расширения класса <see cref="HttpContext"/>.
    /// </summary>
    public static class HttpContextExtensions
    {
        private static readonly RouteData emptyRouteData = new RouteData();
        private static readonly ActionDescriptor emptyActionDescriptor = new ActionDescriptor();
        
        /// <summary>
        /// Асинхронно выполняет результат <see cref="ActionResult"/> ответа.
        /// Используется для обработки результата всех необработанных исключений.
        /// </summary>
        /// <param name="context">Контекст http-запроса.</param>
        /// <param name="result">Экземпляр ответа.</param>
        /// <returns>Асинхронная задача.</returns>
        public static Task ExecuteResultAsync<TResult>(this HttpContext context, TResult result) 
            where TResult : ActionResult
        {
            context.ThrowIfNull(nameof(context));
            result.ThrowIfNull(nameof(result));

            var executor = context.RequestServices.GetRequiredService<IActionResultExecutor<TResult>>();

            RouteData routeData = context.GetRouteData() ?? emptyRouteData;
            var actionContext = new ActionContext(context, routeData, emptyActionDescriptor);

            return executor.ExecuteAsync(actionContext, result);
        }
    }
}
