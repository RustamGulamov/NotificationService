using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using NotificationService.Data.Models;

namespace NotificationService.Data.Repositories
{
    /// <summary>
    /// Репозиторий шаблонов сообщения.
    /// Предназначен для создания/изменения/удаления/получения шаблонов в/из БД.
    /// </summary>
    public interface IMessageTemplatesRepository
    {
        /// <summary>
        /// Добавляет шаблон сообщения.
        /// </summary>
        /// <param name="template">Шаблон сообщения.</param>
        /// <returns>Задача.</returns>
        Task Add(MessageTemplate template);

        /// <summary>
        /// Удаляет шаблон сообщения.
        /// </summary>
        /// <param name="templateName">Название шаблона.</param>
        /// <returns>Задача.</returns>
        Task<DeleteResult> Delete(string templateName);

        /// <summary>
        /// Удаляет все шаблоны.
        /// </summary>
        /// <returns>Задача.</returns>
        Task<DeleteResult> DeleteAll();

        /// <summary>
        /// Обновляет шаблон сообщения.
        /// </summary>
        /// <param name="template">Шаблон сообщения.</param>
        /// <returns>Задача.</returns>
        Task<UpdateResult> Update(MessageTemplate template);

        /// <summary>
        /// Возвращает шаблон по названию.
        /// </summary>
        /// <param name="templateName">Название шаблона.</param>
        /// <returns>Найденный шаблон или null.</returns>
        Task<MessageTemplate> Get(string templateName);

        /// <summary>
        /// Возвращает дочерние шаблоны по названию родительского.
        /// </summary>
        /// <param name="templateName">Название родительского шаблона.</param>
        /// <returns>Найденный шаблон или null.</returns>
        Task<List<MessageTemplate>> GetChildTemplates(string templateName);

        /// <summary>
        /// Коллекция всех шаблонов.
        /// </summary>
        /// <returns>Возвращает все шаблоны.</returns>
        IQueryable<MessageTemplate> GetAll();
    }
}
