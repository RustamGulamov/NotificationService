using FluentValidation;
using NotificationService.Data.Models;

namespace NotificationService.Data.Validators
{
    /// <summary>
    /// Валидатор для класса <see cref="AllTemplates"/>.
    /// </summary>
    public class AllTemplatesValidator : AbstractValidator<AllTemplates>
    {
        private const string GreaterThanZeroErrorFormat = "Value of the {0} should be greater than 0";
        private const string GraterThanExpectedErrorFormat = "Value of the {0} shouldn't be greater than total {1}";

        /// <summary>
        /// Конструктор.
        /// </summary>
        public AllTemplatesValidator()
        {
            RuleFor(at => at.PageSize)
                .GreaterThan(0)
                .WithMessage(string.Format(GreaterThanZeroErrorFormat, nameof(AllTemplates.PageSize)));
            RuleFor(at => at.CurrentPage)
                .GreaterThan(0)
                .WithMessage(string.Format(GreaterThanZeroErrorFormat, nameof(AllTemplates.CurrentPage)))
                .LessThanOrEqualTo(at => at.PagesCount)
                .WithMessage(at => string.Format(GraterThanExpectedErrorFormat, nameof(AllTemplates.CurrentPage), at.PagesCount));
        }
    }
}
