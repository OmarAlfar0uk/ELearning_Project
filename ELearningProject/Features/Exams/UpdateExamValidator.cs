using FluentValidation;

namespace ELearningProject.Features.Exams
{
    /// <summary>
    /// Validator for UpdateExamCommand.
    /// </summary>
    public class UpdateExamValidator : AbstractValidator<UpdateExamCommand>
    {
        public UpdateExamValidator()
        {
            RuleFor(x => x.ExamId)
                .NotEmpty()
                .WithMessage("ExamId is required.");

            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage("Title is required.");

            RuleFor(x => x.DurationMinutes)
                .GreaterThan(0)
                .WithMessage("DurationMinutes must be greater than zero.");

            RuleFor(x => x.DueDate)
                .Must(dueDate => dueDate > DateTime.UtcNow)
                .WithMessage("DueDate must be in the future.");
        }
    }
}
