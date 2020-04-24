using System;
using System.Net;
using System.Net.Mail;
using NotificationService.Logic.Models;

namespace NotificationService.Logic.Factories
{
    /// <summary>
    /// Фабрика для создания <see cref="SmtpClient"/>.
    /// </summary>
    public static class SmtpClientFactory
    {
        /// <summary>
        /// Создать Smtp клиент.
        /// </summary>
        /// <param name="settings">Настройки клиента SMTP.</param>
        /// <returns><see cref="SmtpClient"/>.</returns>
        public static SmtpClient Create(SmtpClientSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            return new SmtpClient
            {
                Host = settings.Host,
                Port = settings.Port,
                Credentials = new NetworkCredential(settings.UserName, settings.Password),
                EnableSsl = settings.EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network
            };
        }
    }
}