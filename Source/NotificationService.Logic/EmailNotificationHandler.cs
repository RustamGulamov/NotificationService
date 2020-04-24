using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NotificationService.Data.Extensions;
using NotificationService.Logic.Factories.Interfaces;
using NotificationService.Logic.NotificationChannels.Interfaces;
using NotificationService.Logic.NotificationMessages;
using RabbitMQClient.Messaging.Interfaces;

namespace NotificationService.Logic
{
    /// <summary>
    /// Обработчик уведомлений по электронной почте.
    /// </summary>
    public sealed class EmailNotificationHandler : IMessageHandler<NotificationMessage<EmailNotificationMessage>>
    {
        private readonly IEmailNotifier emailNotifier;
        private readonly ILogger<EmailNotificationHandler> logger;
        private readonly IEmailMessageFactory emailMessageFactory;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="emailNotifier">Оповещатель email.</param>
        /// <param name="logger">Для логирования событий.</param>
        /// <param name="emailMessageFactory"><see cref="IEmailMessageFactory"/>.</param>
        public EmailNotificationHandler(
            IEmailNotifier emailNotifier,
            ILogger<EmailNotificationHandler> logger,
            IEmailMessageFactory emailMessageFactory
        )
        {
            this.emailNotifier = emailNotifier;
            this.logger = logger;
            this.emailMessageFactory = emailMessageFactory;
        }

        /// <inheritdoc />
        public async Task HandleAsync(NotificationMessage<EmailNotificationMessage> notificationMessage)
        {
            logger.LogInformation("Start handle message for sending email.");

            notificationMessage.ThrowIfNull(nameof(notificationMessage));

            var email = await emailMessageFactory.CreateFromNotificationMessage(notificationMessage);
            await emailNotifier.SendEmailAsync(email);
        }
    }
}