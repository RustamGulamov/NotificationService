namespace NotificationService.Logic.Models
{
    /// <summary>
    /// Электронный адрес.
    /// </summary>
    public class UserEmailAddress
    {
        /// <summary>
        /// Электронный адрес почты.
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Имя пользователя, которому принадлежит адрес.
        /// </summary>
        public string DisplayName { get; set; }
    }
}