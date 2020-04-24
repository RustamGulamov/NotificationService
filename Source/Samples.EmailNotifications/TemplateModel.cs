namespace Samples.EmailNotifications
{
    /// <summary>
    /// Описывает поля шаблона. Используется для сериализации тела запроса.
    /// </summary>
    public class TemplateModel
    {
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

        /// <summary>
        /// Имя шаблона.
        /// </summary>
        public string Name { get; set; }
    }
}