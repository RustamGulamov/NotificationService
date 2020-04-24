using System.Linq;
using FluentValidation.Results;

namespace NotificationService.Data.Extensions
{
    /// <summary>
    /// Методы расширения класса <see cref="ValidationResult"/>.
    /// </summary>
    public static class ValidationResultExtensions
    {
        /// <summary>
        /// Возвращает все ошибки одной строкой, каждая ошибка в новой строке.
        /// </summary>
        /// <param name="validationResult">Результат валидации класса.</param>
        /// <returns>Все ошибки разделенные новой строкой.</returns>
        public static string GetAllErrors(this ValidationResult validationResult)
        {
            return string.Join(", ", validationResult.Errors
                .Select(e => e.ErrorMessage));
        }
    }
}
