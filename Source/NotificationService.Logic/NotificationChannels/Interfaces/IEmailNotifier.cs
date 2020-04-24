using System.Threading.Tasks;
using FluentEmail.Core.Models;
using NotificationService.Logic.Models;

namespace NotificationService.Logic.NotificationChannels.Interfaces
{
    /// <summary>
    /// Интерфейс оповещателя email.
    /// </summary>
    public interface IEmailNotifier
    {
        /// <summary>
        /// Отправляет Email.
        /// </summary>
        /// <param name="message">Сообщение электронной почты.</param>
        /// <returns><see cref="Task"/>Объект асинхронной отправки email.</returns>
        Task<SendResponse> SendEmailAsync(EmailMessage message);
    }
}