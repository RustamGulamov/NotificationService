using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQClient.Messaging.Rabbit;

namespace RabbitClient.Dotnet
{
    /// <summary>
    /// Test RabbitMQ Client for .NET.
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// Точка входа.
        /// </summary>
        /// <param name="args">Аргументы.</param>
        private static void Main(string[] args)
        {
            var message = new
            {
                Token = "token",
                ServiceName = "ServiceName",
                Message = new
                {
                    TemplateName = "middleunless",
                    From = Guid.NewGuid().ToString(),
                    To = new List<string> {Guid.NewGuid().ToString()}
                }
            };

            var tokenSource = new CancellationTokenSource();
            var emailRabbitConfiguration = new RabbitConfiguration
            {
                Host = "localhost",
                UserName = "guest",
                Password = "guest",
                QueueName = "EmailQueue",
                ExchangeName = "EmailExchange"
            };

            var sender = new RabbitMessageSender<object>(emailRabbitConfiguration);

            Task.WaitAll(sender.SendMessageAsync(message, tokenSource.Token));
        }
    }
}
