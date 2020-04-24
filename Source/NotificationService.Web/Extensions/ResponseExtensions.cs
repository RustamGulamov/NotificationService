using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace NotificationService.Web.Extensions
{
    /// <summary>
    /// Методы расширения для ответов API.
    /// </summary>
    public static class ResponseExtensions
    {
        /// <summary>
        /// Метод расширения форматирует ответ загоняя его значение в анонимный тип с полем:
        /// data в случае успеха и error в случае ошибки.
        /// </summary>
        /// <param name="response">Ответ.</param>
        /// <param name="statusCode">Код http-ответа.</param>
        /// <returns>Возвращает форматированный ответ.</returns>
        public static ObjectResult ToFormattedResponse<T>(this T response, int statusCode = 200)
        {
            ObjectResult objectResult = response as ObjectResult;
            if (objectResult != null)
            {
                bool success = objectResult.StatusCode.HasValue ? objectResult.StatusCode.IsSuccess() : IsSuccess(statusCode);
                string field = success ? "data" : "error";
                    
                return new ObjectResult(new Dictionary<string, object>
                {
                    {
                        field, objectResult.Value
                    }
                })
                {
                    StatusCode = objectResult.StatusCode ?? statusCode
                };
            }
            else
            {
                return new ObjectResult(new { data = response });
            }
        }

        /// <summary>
        /// Проверяет вхождение http-статуса в диапазон успешных.
        /// </summary>
        /// <param name="statusCode">Http-статус.</param>
        /// <returns>Входит или не входит.</returns>
        private static bool IsSuccess(this int? statusCode)
        {
            return statusCode >= 200 && statusCode <= 299;
        }
    }
}
