using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using NotificationService.Data.Extensions;
using NotificationService.Web.Extensions;

namespace NotificationService.Web.Authorization
{
    /// <summary>
    /// Предназначен для фильтрации пользователей с JWT и указанными ролями.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class JWTAuthorizationFilterAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private const string JwtPrefix = "Bearer";

        private readonly ILogger<JWTAuthorizationFilterAttribute> logger;
        private readonly IValidator<JwtSecurityToken> tokenValidator;

        /// <summary>
        /// Конструктор, инициализирует логгер и свойства.
        /// </summary>
        /// <param name="logger">Экземпляр модуля для логирования.</param>
        /// <param name="tokenValidator">Валидатор для класса <see cref="JwtSecurityToken"/>.</param>
        public JWTAuthorizationFilterAttribute(
            ILogger<JWTAuthorizationFilterAttribute> logger, 
            IValidator<JwtSecurityToken> tokenValidator)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.tokenValidator = tokenValidator ?? throw new ArgumentNullException(nameof(tokenValidator));
        }

        /// <inheritdoc/>
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            string authorizationHeader = context.HttpContext.Request.Headers[HttpRequestHeader.Authorization.ToString()];
            JwtSecurityToken token = InjectToken(authorizationHeader);

            ValidationResult validationResult = await tokenValidator.ValidateAsync(token);
            if (!validationResult.IsValid)
            {
                context.Result = new ObjectResult(validationResult.GetAllErrors())
                    .ToFormattedResponse((int)HttpStatusCode.Unauthorized);
                return;
            }
            
            context.HttpContext.User.AddIdentity(new ClaimsIdentity(token.Claims));
        }
        
        /// <summary>
        /// Возвращает JWT токен готовый к употреблению или null.
        /// </summary>
        /// <param name="authorizationHeader">Header авторизации из входящего запроса.</param>
        /// <returns>Токен или null.</returns>
        private JwtSecurityToken InjectToken(string authorizationHeader)
        {
            if (string.IsNullOrEmpty(authorizationHeader) ||
                !authorizationHeader.StartsWith(JwtPrefix, StringComparison.OrdinalIgnoreCase))
            {
                logger.LogWarning("Authorization header doesn't contains JSON web token");
                return null;
            }

            try
            {
                string encodedToken = authorizationHeader.Replace(JwtPrefix, string.Empty).Trim();
                return new JwtSecurityTokenHandler().ReadJwtToken(encodedToken);
            }
            catch (ArgumentException e)
            {
                logger.LogError(e.Message);
                return null;
            }
        }
    }
}
