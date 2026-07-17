using FluentValidation;

namespace ELearningProject.Features.Exams
{
    /// <summary>
    /// Validator for CreateExamCommand.
    /// </summary>
    public class CreateExamValidator : AbstractValidator<CreateExamCommand>
    {
        public CreateExamValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage("Title is required.");

            RuleFor(x => x.TrackId)
                .NotEmpty()
                .WithMessage("TrackId is required.");

            RuleFor(x => x.DurationMinutes)
                .GreaterThan(0)
                .WithMessage("DurationMinutes must be greater than zero.");

            RuleFor(x => x.DueDate)
                .Must(dueDate => dueDate > DateTime.UtcNow)
                .WithMessage("DueDate must be in the future.");
        }
    }
}
