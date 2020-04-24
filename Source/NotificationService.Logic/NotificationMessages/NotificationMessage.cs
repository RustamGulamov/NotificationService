namespace NotificationService.Logic.NotificationMessages
{
    /// <summary>
    /// Сообщение об отправки нотификации.
    /// </summary>
    /// <typeparam name="TMessage">Тип сообщения.</typeparam>
    public class NotificationMessage<TMessage> where TMessage : new() 
    {
        /// <summary>
        /// Токен сервиса, запросившего отправку уведомления.
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Имя сервиса запросившего отправку.
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// Сообщение.
        /// </summary>
        public TMessage Message { get; set; } = new TMessage();
    }
}