using FluentValidation;
using FluentValidation.Results;
using NotificationService.Web.Models;

namespace NotificationService.Web.Validators
{
    /// <summary>
    /// Валидатор для класса <see cref="Template"/>.
    /// </summary>
    public class TemplateValidator : AbstractValidator<Template>
    {
        /// <summary>
        /// Конструктор.
        /// </summary>
        public TemplateValidator()
        {
            RuleFor(t => t).NotNull();
            RuleFor(t => t.Name).NotEmpty();
            RuleFor(t => t.Title).NotEmpty();
            RuleFor(t => t.Body).NotEmpty();
            RuleFor(t => t.Name).NotEqual(t => t.Parent)
                .WithMessage("Template can't link to itself");
        }

        /// <inheritdoc />
        public override ValidationResult Validate(ValidationContext<Template> context)
        {
            return context?.InstanceToValidate == null ? GetFailure() : base.Validate(context);
        }
        
        /// <summary>
        /// Возвращает негативный результат проверки модели запроса".
        /// </summary>
        /// <returns>Негативный результат проверки.</returns>
        private ValidationResult GetFailure()
        {
            return new ValidationResult(new[]
            {
                new ValidationFailure(nameof(Template), "API request model should be valid!")
            });
        }
    }
}
