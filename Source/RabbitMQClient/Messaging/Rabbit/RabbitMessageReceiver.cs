using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQClient.Messaging.Interfaces;

namespace RabbitMQClient.Messaging.Rabbit
{
    /// <summary>
    /// Получатель сообщения из RabbitMQ.
    /// </summary>
    /// <typeparam name="TMessage">Тип обработанного сообщения.(Тип сообщения для десериализации).</typeparam>
    /// <typeparam name="TMessageHandler">Тип обработчика сообщения.</typeparam>
    public class RabbitMessageReceiver<TMessage, TMessageHandler> : MessageReceiver<TMessage, BasicDeliverEventArgs, TMessageHandler> 
        where TMessageHandler : IMessageHandler<TMessage>
    {
        private readonly RabbitConfiguration configuration;
        private readonly ILogger<RabbitMessageReceiver<TMessage, TMessageHandler>> logger;
        private readonly List<Task> handleMessageTasks = new List<Task>();
        private IConnection connection;
        private IModel channel;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="configuration">Конфигурация RabbitMQ.</param>
        /// <param name="serviceScopeFactory">Фабрика для создания экземпляров <see cref="IServiceScope"/>.</param>
        /// <param name="logger">Логирования.</param>
        public RabbitMessageReceiver(RabbitConfiguration configuration, IServiceScopeFactory serviceScopeFactory, ILogger<RabbitMessageReceiver<TMessage, TMessageHandler>> logger)
            : base(serviceScopeFactory, logger)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Запустить асинхронный процесс получения сообщений из очереди.
        /// </summary>
        /// <param name="cancellationToken">Токен аннулирования задачи. Не используется.</param>
        /// <returns><see cref="Task"/> объект асинхронного процесса получения сообщений.</returns>
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                return StartAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while starting the host to receive a message from queue Rabbit MQ.");

                return Task.FromException(ex);
            }
        }

        /// <summary>
        /// Остановить асинхронный процесс получения сообщений из очереди.
        /// </summary>
        /// <param name="cancellationToken">Токен аннулирования задачи. Не используется.</param>
        /// <returns><see cref="Task"/> объект асинхронного процесса получения сообщений.</returns>
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            channel?.Dispose();
            connection?.Dispose();

            logger.LogInformation("Host to receive message from queue Rabbit MQ successfully stoped.");

            return Task.CompletedTask;
        }

        /// <summary>
        /// Десериализовать извлеченное сообщение из очереди.
        /// </summary>
        /// <param name="rawMessage">Сообщение из очереди. Содержит всю информацию о доставленном сообщении от брокера AMQP.</param>
        /// <returns>Десериализованный объект обработанного сообщения.</returns>
        protected override TMessage DeserializeMessage(BasicDeliverEventArgs rawMessage)
        {
            var json = Encoding.UTF8.GetString(rawMessage.Body);
            return JsonConvert.DeserializeObject<TMessage>(json);
        }

        private Task StartAsync()
        {
            logger.LogInformation("Try start host to receive message from queue Rabbit MQ.");

            CreateConnection();

            channel = connection.CreateModel();

            SetupChannel();

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += OnReceived;

            channel.BasicConsume(configuration.QueueName, false, consumer);

            logger.LogInformation("Host to receive message from queue Rabbit MQ successfully started.");

            return Task.CompletedTask;
        }

        private async Task OnReceived(object o, BasicDeliverEventArgs args)
        {
            logger.LogInformation("Received a message from queue Rabbit MQ.");

            handleMessageTasks.RemoveAll(x => x.IsCompleted);

            handleMessageTasks.Add(Task.Run(async () =>
            {
                if (await HandleMessageAsync(args))
                {
                    channel.BasicAck(args.DeliveryTag, false);
                }
                else
                {
                    channel.BasicNack(args.DeliveryTag, false, false);
                }
            }));

            if (handleMessageTasks.Count == configuration.MaxParallelization)
            {
                await Task.WhenAll(handleMessageTasks);
            }

            await Task.Yield();
        }


        private void CreateConnection()
        {
            logger.LogInformation("RabbitMQ configurations from appsettings. " +
                                  $"Host: {configuration.Host}, " +
                                  $"UserName: {configuration.UserName}, " +
                                  $"ExchangeName:{configuration.ExchangeName}, " +
                                  $"QueueName:{configuration.QueueName} " +
                                  $"Routingkey: {configuration.Routingkey}");

            var factory = RabbitConnectionFactory.CreateFactory(configuration);

            logger.LogInformation("Try to create connection to Rabbit MQ.");

            connection = factory.CreateConnection();

            logger.LogInformation("Created connection to Rabbit MQ.");
        }

        private void SetupChannel()
        {
            channel.ExchangeDeclare(configuration.ExchangeName, ExchangeType.Direct);
            channel.QueueDeclare(configuration.QueueName, true, false, false);
            channel.QueueBind(configuration.QueueName, configuration.ExchangeName, configuration.Routingkey);
        }
    }
}