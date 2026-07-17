using FluentValidation;

namespace ELearningProject.Features.Exams
{
    /// <summary>
    /// Validator for StartExamCommand.
    /// </summary>
    public class StartExamValidator : AbstractValidator<StartExamCommand>
    {
        public StartExamValidator()
        {
            RuleFor(x => x.ExamId)
                .NotEmpty()
                .WithMessage("ExamId is required.");
        }
    }
}
