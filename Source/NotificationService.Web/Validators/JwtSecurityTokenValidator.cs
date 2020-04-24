using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Options;
using NotificationService.Web.Authorization;

namespace NotificationService.Web.Validators
{
    /// <summary>
    /// Валидатор для класса <see cref="JwtSecurityToken"/>.
    /// </summary>
    public class JwtSecurityTokenValidator : AbstractValidator<JwtSecurityToken>
    {
        private readonly AuthorizationSettings settings;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="settings">Настройки авторизации.</param>
        public JwtSecurityTokenValidator(IOptionsSnapshot<AuthorizationSettings> settings)
        {
            this.settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));

            if (!this.settings.RequiredRoles.Any() || !this.settings.RequiredRoles.Any(role => !string.IsNullOrEmpty(role)))
            {
                throw new ArgumentException($"{nameof(AuthorizationSettings)} doesn't contains {nameof(AuthorizationSettings.RequiredRoles)} definition");
            }
            
            RuleFor(t => t.Claims)
                .Must(claims => ContainClaim(claims, 
                    c => string.Equals(c.Type, "id", StringComparison.OrdinalIgnoreCase)))
                .WithMessage("Token doesn't contains Id claims");
            RuleFor(t => t.Claims)
                .Must(claims => ContainClaim(claims, claim => claim.Type == ClaimTypes.Role))
                .WithMessage("Token doesn't contains Role claims");
            RuleFor(t => t.Claims)
                .Must(claims =>
                    claims.Where(claim => claim.Type == ClaimTypes.Role)
                        .Select(claim => claim.Value)
                        .Any(role => this.settings.RequiredRoles.Contains(role)))
                .WithMessage($"Token should contain one of required roles: {string.Join(", ", this.settings.RequiredRoles)}");
            RuleFor(t => t.ValidTo)
                .NotNull()
                .NotEmpty()
                .Must(v => v > DateTime.Now)
                .WithMessage("Token doesn't valid");
        }

        /// <inheritdoc />
        public override ValidationResult Validate(ValidationContext<JwtSecurityToken> context)
        {
            return context?.InstanceToValidate == null ? GetFailure() : base.Validate(context);
        }

        /// <inheritdoc />
        public override async Task<ValidationResult> ValidateAsync(
            ValidationContext<JwtSecurityToken> context,
            CancellationToken cancellation = default(CancellationToken))
        {
            return context?.InstanceToValidate == null ? GetFailure() : await base.ValidateAsync(context);
        }

        /// <inheritdoc />
        protected override void EnsureInstanceNotNull(object instanceToValidate)
        {
        }

        /// <summary>
        /// Проверяет существование <see cref="Claim"/> в списке <see cref="IEnumerable{Claim}"/>.
        /// </summary>
        /// <param name="claims">Список <see cref="IEnumerable{Claim}"/>.</param>
        /// <param name="predicate">Условие поиска.</param>
        /// <returns>True - существует, иначе false.</returns>
        private bool ContainClaim(IEnumerable<Claim> claims, Predicate<Claim> predicate)
        {
            return claims.Any(claim => predicate(claim));
        }

        /// <summary>
        /// Возвращает негативный результат проверки модели запроса".
        /// </summary>
        /// <returns>Негативный результат проверки.</returns>
        private ValidationResult GetFailure()
        {
            return new ValidationResult(new[]
            {
                new ValidationFailure(nameof(JwtSecurityToken), "Authorization header doesn't contains JSON web token")
            });
        }
    }
}
