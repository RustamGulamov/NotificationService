using System;
using System.Collections.Generic;

namespace NotificationService.Logic.Models
{
    /// <summary>
    /// Представляет отформатированное электронное сообщение.
    /// Используется для отправки конечным потребителям.
    /// </summary>
    public class EmailMessage
    {
        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="subject">Тема.</param>
        /// <param name="message">Тело.</param>
        /// <param name="from">Отправитель.</param>
        /// <param name="to">Список получателей.</param>
        /// <param name="cc">Список копии.</param>
        public EmailMessage(string subject, string message, UserEmailAddress from, List<string> to, List<string> cc = null)
        {
            if (to == null)
            {
                throw new ArgumentNullException(nameof(to));
            }
            if (to.Count == 0)
            {
                throw new ArgumentException("To should contain recipients");
            }
            Subject = subject ?? throw new ArgumentNullException(nameof(subject));
            Message = message ?? throw new ArgumentNullException(nameof(message));
            From = from ?? throw new ArgumentNullException(nameof(from));
            To = to;
            Сс = cc;
        }


        /// <summary>
        /// Электронная почта отправителя.
        /// </summary>
        public UserEmailAddress From { get; }

        /// <summary>
        /// Список получателей письма.
        /// </summary>
        public List<string> To { get; }

        /// <summary>
        /// Список получателей копии письма.
        /// </summary>
        public List<string> Сс { get; }

        /// <summary>
        /// Тема (заголовок) письма.
        /// </summary>
        public string Subject { get; }

        /// <summary>
        /// Тело (содержимое) письма.
        /// </summary>
        public string Message { get; }
    }
}
