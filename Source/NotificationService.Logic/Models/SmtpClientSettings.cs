namespace NotificationService.Logic.Models
{
    /// <summary>
    /// Настройки SMTP клиента.
    /// </summary>
    public class SmtpClientSettings
    {
        /// <summary>
        /// Имя почтового хоста.
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// Порт.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Включить ли SSL.
        /// </summary>
        public bool EnableSsl { get; set; }

        /// <summary>
        /// Почта по умолчанию. From.
        /// </summary>
        public string DefaultEmail { get; set; } = string.Empty;

        /// <summary>
        /// Имя пользователя.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Пароль.
        /// </summary>
        public string Password { get; set; }
    }
}