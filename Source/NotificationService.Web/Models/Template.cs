using NotificationService.Data.Models;

namespace NotificationService.Web.Models
{
    /// <summary>
    /// Описывает поля шаблона. Используется для сериализации тела запроса.
    /// </summary>
    public class Template
    {
        /// <summary>
        /// Название шаблона.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Заголовок шаблона.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Имя родительского шаблона.
        /// </summary>
        public string Parent { get; set; }

        /// <summary>
        /// Содержимое шаблона.
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Тип движка шаблонизатора.
        /// </summary>
        public Engines Engine { get; set; }
    }
}
