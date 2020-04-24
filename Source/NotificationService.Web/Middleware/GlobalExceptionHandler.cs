using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NotificationService.Data.Exceptions;
using NotificationService.Web.Extensions;

namespace NotificationService.Web.Middleware
{
    /// <summary>
    /// Глобальный обработчик всех необработанных исключений.
    /// </summary>
    public class GlobalExceptionHandler
    {
        private readonly RequestDelegate next;
        private readonly ILogger<GlobalExceptionHandler> logger;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="next">Делегат обработки запроса <see cref="RequestDelegate"/>.</param>
        /// <param name="logger">Для логирования событий.</param>
        public GlobalExceptionHandler(RequestDelegate next, ILogger<GlobalExceptionHandler> logger)
        {
            this.next = next ?? throw new ArgumentNullException(nameof(next));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Запускает выполнение делегата обработки запроса.
        /// </summary>
        /// <param name="context">Контекст http-запроса.</param>
        /// <returns>Асинхронная задача.</returns>
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Unhandled exception was thrown");
                await HandleExceptionAsync(context, e);
            }
        }

        /// <summary>
        /// Асинхронный обработчик исключений.
        /// </summary>
        /// <param name="context">Контекст http.</param>
        /// <param name="exception">Возникшее исключение.</param>
        /// <returns>Обработанное исключение форматируется в соответствии с требованиями.</returns>
        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            HttpStatusCode code = GetStatusCode(exception);
            var result = new ObjectResult(exception.Message).ToFormattedResponse((int)code);
            context.ExecuteResultAsync(result);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Возвращает http-статус код исходя из исключения.
        /// </summary>
        /// <param name="exception">Исключение <see cref="Exception"/>.</param>
        /// <returns>Http-статус код <see cref="HttpStatusCode"/>.</returns>
        private static HttpStatusCode GetStatusCode(Exception exception)
        {
            switch (exception)
            {
                case ArgumentNullException e:
                {
                    return HttpStatusCode.BadRequest;
                }
                case TemplateNotFoundException e:
                {
                    return HttpStatusCode.NotFound;
                }
                default:
                {
                    return HttpStatusCode.InternalServerError;
                }
            }
        }
    }
}
