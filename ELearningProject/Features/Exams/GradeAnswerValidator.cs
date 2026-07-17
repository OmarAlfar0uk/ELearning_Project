using FluentValidation;

namespace ELearningProject.Features.Exams
{
    /// <summary>
    /// Validator for GradeAnswerCommand.
    /// </summary>
    public class GradeAnswerValidator : AbstractValidator<GradeAnswerCommand>
    {
        public GradeAnswerValidator()
        {
            RuleFor(x => x.AnswerId)
                .NotEmpty()
                .WithMessage("AnswerId is required.");

            RuleFor(x => x.PointsAwarded)
                .GreaterThanOrEqualTo(0)
                .WithMessage("PointsAwarded cannot be negative.");

            RuleFor(x => x.Feedback)
                .MaximumLength(4000)
                .WithMessage("Feedback cannot exceed 4000 characters.");
        }
    }
}
