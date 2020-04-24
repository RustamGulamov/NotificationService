using System;

namespace NotificationService.Logic.Exceptions
{
    /// <summary>
    /// Исключение в бизнес-логике приложения.
    /// </summary>
    public abstract class ApplicationDomainException : ApplicationException
    {
        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="msg">Сообщение.</param>
        protected ApplicationDomainException(string msg) : base(msg)
        {
        }
    }
}
