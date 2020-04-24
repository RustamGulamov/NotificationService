using System.Threading.Tasks;
using NotificationService.Data.Models;

namespace NotificationService.Data
{
    /// <summary>
    /// Менеджер шаблонов сообщений. Содержит бизнес логику по обработке шаблонов перед добавлением их в базу.
    /// </summary>
    public interface ITemplateManager
    {
        /// <summary>
        /// Добавляет шаблон сообщения.
        /// </summary>
        /// <param name="template">Шаблон сообщения.</param>
        /// <returns>Задача.</returns>
        Task AddTemplate(MessageTemplate template);

        /// <summary>
        /// Возвращает шаблон по названию.
        /// </summary>
        /// <param name="templateName">Название шаблона.</param>
        /// <returns>Найденный шаблон или null.</returns>
        Task<MessageTemplate> GetTemplate(string templateName);

        /// <summary>
        /// Коллекция всех шаблонов.
        /// </summary>
        /// <param name="pageInfo">Информация о странице.</param>
        /// <returns>Возвращает шаблоны в постраничном виде.</returns>
        AllTemplates GetTemplatesPaged(PageInfo pageInfo);

        /// <summary>
        /// Обновляет шаблон сообщения.
        /// </summary>
        /// <param name="template">Шаблон сообщения.</param>
        /// <returns>True - шаблон успешно обновлен, иначе false.</returns>
        Task<bool> UpdateTemplate(MessageTemplate template);

        /// <summary>
        /// Удаляет шаблон сообщения.
        /// </summary>
        /// <param name="templateName">Название шаблона.</param>
        /// <returns>True - шаблон успешно удален, иначе false.</returns>
        Task<bool> DeleteTemplate(string templateName);

        /// <summary>
        /// Удаляет все шаблоны.
        /// </summary>
        /// <returns>True - шаблон успешно удален, иначе false.</returns>
        Task<bool> DeleteAllTemplates();
    }
}