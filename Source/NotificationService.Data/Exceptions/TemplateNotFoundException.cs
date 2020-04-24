using System;
using NotificationService.Data.Models;

namespace NotificationService.Data.Exceptions
{
    /// <summary>
    /// Исключение для не найденных шаблонов <see cref="MessageTemplate"/>.
    /// </summary>
    public class TemplateNotFoundException : Exception
    {
        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="message">Сообщение об ошибке.</param>
        public TemplateNotFoundException(string message) 
            : base(message)
        {
        }
    }
}
