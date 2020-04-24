namespace NotificationService.Logic.Exceptions
{
    /// <summary>
    /// Исключение при создании сообщения ЭП.
    /// </summary>
    public class EmailMessageCreationException : ApplicationDomainException
    {
        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="message">Сообщение с ошибкой.</param>
        public EmailMessageCreationException(string message)
            : base($"Ошибка во время создания сообщения ЭП: {message}")
        {
        }
    }
}
