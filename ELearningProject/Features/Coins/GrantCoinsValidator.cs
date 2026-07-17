using FluentValidation;

namespace ELearningProject.Features.Coins
{
    /// <summary>
    /// Validates <see cref="GrantCoinsCommand"/> before it reaches the handler.
    /// </summary>
    public class GrantCoinsValidator : AbstractValidator<GrantCoinsCommand>
    {
        public GrantCoinsValidator()
        {
            RuleFor(x => x.StudentId)
                .NotEmpty()
                .WithMessage("StudentId is required.");

            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage("Amount must be greater than zero.");

            RuleFor(x => x.Reason)
                .NotEmpty()
                .WithMessage("Reason is required.")
                .MaximumLength(500)
                .WithMessage("Reason must not exceed 500 characters.");
        }
    }
}
