using System;
using System.Threading.Tasks;
using FluentEmail.Core;
using FluentEmail.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotificationService.Data.Extensions;
using NotificationService.Logic.Models;
using NotificationService.Logic.NotificationChannels.Interfaces;
using Polly;

namespace NotificationService.Logic.NotificationChannels
{
    /// <summary>
    /// Сервис по отправке электронных сообщений.
    /// </summary>
    public class EmailNotifier : IEmailNotifier
    {
        private readonly ILogger<EmailNotifier> logger;
        private readonly IFluentEmail fluentEmail;
        private readonly int retryCountsWithExponentialWaitTime;

        private readonly Random random = new Random();

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="logger">Для логирования ошибок.</param>
        /// <param name="fluentEmail">Отправитель электронной почты.</param>
        /// <param name="emailSettings">Настройки отправителя сообщений.</param>
        public EmailNotifier(ILogger<EmailNotifier> logger, IFluentEmail fluentEmail, IOptionsSnapshot<EmailSettings> emailSettings)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.fluentEmail = fluentEmail ?? throw new ArgumentNullException(nameof(fluentEmail));
            
            retryCountsWithExponentialWaitTime = emailSettings?.Value?.RetryCountsWithExponentialWaitTime ?? throw new ArgumentNullException(nameof(emailSettings));
        }

        /// <inheritdoc/>
        public async Task<SendResponse> SendEmailAsync(EmailMessage message)
        {
            message.ThrowIfNull(nameof(message));

            IFluentEmail email = fluentEmail
                .SetFrom(message.From.Address, message.From.DisplayName)
                .Subject(message.Subject)
                .Body(message.Message);
            
            message.To?.ForEach(to => email.To(to));
            message.Сс?.ForEach(cc => email.CC(cc));

            email.Data.IsHtml = true;

            logger.LogInformation($"Sending email with title \"{message.Subject}\" asynchronously");

            return await Policy
                .Handle<Exception>()
                .WaitAndRetryForeverAsync(GetSleepTimeForRetry, LogRetryException)
                .ExecuteAsync(() => email.SendAsync());
        }

        private TimeSpan GetSleepTimeForRetry(int retryCounter)
        {
            int counter = retryCounter < retryCountsWithExponentialWaitTime
                ? retryCounter
                : retryCountsWithExponentialWaitTime;

            return TimeSpan.FromSeconds(Math.Pow(2, counter)) + TimeSpan.FromMilliseconds(random.Next(0, 100));
        }

        private void LogRetryException(Exception exception, int retryCount, TimeSpan timeSpan)
        {
            if (retryCount < retryCountsWithExponentialWaitTime)
            {
                logger.LogWarning(exception, $"Unhandled exception on sending email (retry {retryCount})");
            }
            else
            {
                logger.LogError(exception, $"Unhandled exception on sending email (retry {retryCount})");
            }
        }
    }
}