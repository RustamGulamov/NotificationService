using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQClient.Messaging.Rabbit;

namespace Samples.EmailNotifications
{
    /// <summary>
    /// Rabbit MQ клиент для нотификации.
    /// </summary>
    public class NotificationsRabbitMqClient
    {
        private readonly RabbitMessageSender<object> sender;
        private readonly CancellationTokenSource tokenSource = new CancellationTokenSource();

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="baseAdress">Адрес сервера.</param>
        public NotificationsRabbitMqClient(string baseAdress)
        {
            var configuration = new RabbitConfiguration
            {
                Host = baseAdress,
                UserName = "guest",
                Password = "guest",
                QueueName = "EmailQueue",
                ExchangeName = "EmailExchange",
            };

            sender = new RabbitMessageSender<object>(configuration);
        }

        /// <summary>
        /// Создает сообщения и отправляет.
        /// </summary>
        /// <param name="templateNames">Имя шаблонов.</param>
        public void CreateAndSendMessage(List<string> templateNames)
        {
            foreach (var templateName in templateNames)
            {
                var message = CreateMessage(templateName);
                SendMessage(message);
            }
        }

        private object CreateMessage(string templateName)
        {
            return new
            {
                Token = "tokenName",
                ServiceName = "serviceName",
                Message = new
                {
                    TemplateName = templateName,
                    Params = new
                    {
                        ProjectName = "Release_board",
                        RepositoryName = "Notification service",
                        BranchName = "master",
                        Version = "1.0.1",
                        RunBy = "Gulamov.Rustam",
                        BuildState = "succeed",BuildTime = "01:29:54",
                        BuildType = "Релиз",
                    },
                    From = new
                    {
                        Address= "someone@mail.ru",
                        DisplayName = "Someone man"
                    },
                    To = new List<string>
                    {
                        "anotherMan@mail.ru"
                    },
                },
            };
        }

        private Task SendMessage(object message)
        {
            return sender.SendMessageAsync(message, tokenSource.Token);
        }
    }
}