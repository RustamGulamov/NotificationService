using System.Collections.Generic;

namespace NotificationService.Web.Authorization
{
    /// <summary>
    /// Настройки авторизации.
    /// </summary>
    public class AuthorizationSettings
    {
        /// <summary>
        /// Список требуемых ролей.
        /// </summary>
        public List<string> RequiredRoles { get; set; }
    }
}
