namespace Samples.EmailNotifications
{
    /// <summary>
    /// Файл шаблона.
    /// </summary>
    public class TemplateFile
    {
        /// <summary>
        /// Движки шаблонизаторов.
        /// </summary>
        public Engines Engine { get; set; }

        /// <summary>
        /// Путь к шаблону.
        /// </summary>
        public string TemplateFilePath { get; set; }

        /// <summary>
        /// Путь к родительскому шаблону.
        /// </summary>
        public string ParentTemplatePath { get; set; }

        /// <summary>
        /// Заголовок.
        /// </summary>
        public string Title { get; set; }
    }
}