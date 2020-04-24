using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NotificationService.Data;
using NotificationService.Data.Models;
using NotificationService.Logic.Exceptions;
using NotificationService.Logic.Factories.Interfaces;
using NotificationService.Logic.Models;
using NotificationService.Logic.NotificationMessages;
using NotificationService.Logic.Services.Interfaces;

namespace NotificationService.Logic.Factories
{
    /// <inheritdoc />
    public class EmailMessageFactory : IEmailMessageFactory
    {
        private readonly ILogger<EmailMessageFactory> logger;
        private readonly ITemplateManager templateManager;
        private readonly IEmailBodyGenerator emailBodyGenerator;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="logger"><see cref="ILogger{TCategoryName}"/>.</param>
        /// <param name="templateManager"><see cref="ITemplateManager"/>.</param>
        /// <param name="emailBodyGenerator"><see cref="IEmailBodyGenerator"/>.</param>
        public EmailMessageFactory(
            ILogger<EmailMessageFactory> logger,
            ITemplateManager templateManager,
            IEmailBodyGenerator emailBodyGenerator
        )
        {
            this.logger = logger;
            this.templateManager = templateManager;
            this.emailBodyGenerator = emailBodyGenerator;
        }

        /// <inheritdoc />
        public async Task<EmailMessage> CreateFromNotificationMessage(NotificationMessage<EmailNotificationMessage> notificationMessage)
        {
            try
            {
                logger.LogInformation("Start email message creation");

                MessageTemplate template = await templateManager.GetTemplate(notificationMessage.Message.TemplateName);

                logger.LogInformation(
                    "Information about received template from repository:" +
                    $"Name:{template.Name}, " +
                    $"Engine type :{template.EngineType}, " +
                    $"Parent template: {template.Parent}");

                string emailBody = await emailBodyGenerator.GenerateEmailBody(template, notificationMessage.Message.Params);

                return CreateEmailMessage(template.Title, emailBody, notificationMessage);
            }
            catch (Exception e)
            {
                throw new EmailMessageCreationException(e.Message);
            }
        }

        private EmailMessage CreateEmailMessage(string title, string emailBody, NotificationMessage<EmailNotificationMessage> notificationMessage)
        {
            EmailNotificationMessage message = notificationMessage.Message;
            string subject = string.IsNullOrWhiteSpace(message.Subject)
                ? title
                : message.Subject;
            
            return new EmailMessage(subject, emailBody, message.From, message.To, message.Cс);
        }
    }
}
