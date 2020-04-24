using System.Threading.Tasks;
using NotificationService.Data.Models;

namespace NotificationService.Logic.MessageTemplateEngines
{
    /// <summary>
    /// Интерфейс шаблонизатора.
    /// </summary>
    public interface ITemplateEngine
    {
        /// <summary>
        /// Добавить родительский шаблон в шаблонизатор.
        /// </summary>
        /// <param name="parentTemplate">Родительский шаблон.</param>
        void AddParentTemplate(MessageTemplate parentTemplate);

        /// <summary>
        /// Рендер по шаблону. 
        /// </summary>
        /// <typeparam name="T">Тип модели.</typeparam>
        /// <param name="template">Шаблон.</param>
        /// <param name="model">Модель шаблона.</param>
        /// <returns>Сгенерированный текст.</returns>
        Task<string> RenderAsync<T>(MessageTemplate template, T model);
    }
}