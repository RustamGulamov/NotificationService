using System.Threading.Tasks;
using NotificationService.Logic.Models;
using NotificationService.Logic.NotificationMessages;

namespace NotificationService.Logic.Factories.Interfaces
{
    /// <summary>
    /// Фабрика по созданию сообщений ЭП <see cref="EmailMessage"/>.
    /// </summary>
    public interface IEmailMessageFactory
    {
        /// <summary>
        /// Создает сообщение ЭП из нотификации об отправке сообщения.
        /// </summary>
        /// <param name="notificationMessage">Нотификация об отправке сообщения.</param>
        /// <returns><see cref="EmailMessage"/>.</returns>
        Task<EmailMessage> CreateFromNotificationMessage(NotificationMessage<EmailNotificationMessage> notificationMessage);
    }
}
