using System.Threading.Tasks;
using NotificationService.Data.Models;

namespace NotificationService.Logic.Services.Interfaces
{
    /// <summary>
    /// Интерфейс класса который находит шаблон по имени, и генерирует текст на основе параметров.
    /// </summary>
    public interface IEmailBodyGenerator
    {
        /// <summary>
        /// Генерировать текст email по названия шаблона.
        /// </summary>
        /// <typeparam name="T">Тип модели.</typeparam>
        /// <param name="template">Шаблон сообщения.</param>
        /// <param name="model">Модель шаблона.</param>
        /// <returns>Сгенерированный текст.</returns>
        Task<string> GenerateEmailBody<T>(MessageTemplate template, T model);
    }
}