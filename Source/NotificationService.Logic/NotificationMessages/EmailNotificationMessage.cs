using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using NotificationService.Logic.Models;

namespace NotificationService.Logic.NotificationMessages
{
    /// <summary>
    /// Сообщение нотификации по электронной почте.
    /// </summary>
    public class EmailNotificationMessage
    {
        /// <summary>
        /// Тема письма.
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Имя шаблона.
        /// </summary>
        public string TemplateName { get; set; }

        /// <summary>
        /// Параметры для шаблона.
        /// </summary>
        public JObject Params { get; set; } = new JObject();

        /// <summary>
        /// Идентификатор пользователя от чьего имени будет производится отправка, если null то из под специальной учетки.
        /// </summary>
        public UserEmailAddress From { get; set; }

        /// <summary>
        /// Идентификаторы получателей.
        /// </summary>
        public List<string> To { get; set; } = new List<string>();

        /// <summary>
        /// Копии получателей.
        /// </summary>
        public List<string> Cс { get; set; } = new List<string>();
    }
}