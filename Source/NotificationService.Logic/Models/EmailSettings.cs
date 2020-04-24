namespace NotificationService.Logic.Models
{
    /// <summary>
    /// Настройки отправителя сообщений.
    /// </summary>
    public class EmailSettings
    {
        /// <summary>
        /// Количество повторов отправки письма с экспоненциальным временем ожидания.
        /// Все повторы, превышающие значение переменной
        /// будут ожидать квадрат значения данного свойства в секундах.
        /// </summary>
        public int RetryCountsWithExponentialWaitTime { get; set; }
    }
}
