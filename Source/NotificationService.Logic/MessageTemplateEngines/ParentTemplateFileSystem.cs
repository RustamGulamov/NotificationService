using DotLiquid;
using DotLiquid.FileSystems;
using NotificationService.Data.Models;

namespace NotificationService.Logic.MessageTemplateEngines
{
    /// <summary>
    /// Файловая система хранилище шаблонов.
    /// </summary>
    public class ParentTemplateFileSystem : IFileSystem
    {
        private readonly MessageTemplate parentTemplate;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="parentTemplate">Родительский шаблон.</param>
        public ParentTemplateFileSystem(MessageTemplate parentTemplate)
        {
            this.parentTemplate = parentTemplate;
        }

        /// <summary>
        /// Читать шаблон из хранилище.
        /// </summary>
        /// <param name="context">Context keeps the variable stack and resolves variables, as well as keywords. Не используется.</param>
        /// <param name="templateName">Имя шаблона. Не используется.</param>
        /// <returns>Шаблон.</returns>
        public string ReadTemplateFile(Context context, string templateName)
        {
            return parentTemplate?.Body;
        }
    }
}