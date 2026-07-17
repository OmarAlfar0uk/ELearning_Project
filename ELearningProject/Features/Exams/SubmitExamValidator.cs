using FluentValidation;

namespace ELearningProject.Features.Exams
{
    /// <summary>
    /// Validator for SubmitExamCommand.
    /// </summary>
    public class SubmitExamValidator : AbstractValidator<SubmitExamCommand>
    {
        public SubmitExamValidator()
        {
            RuleFor(x => x.AttemptId)
                .NotEmpty()
                .WithMessage("AttemptId is required.");

            RuleFor(x => x.Answers)
                .NotNull()
                .WithMessage("Answers are required.");

            RuleForEach(x => x.Answers)
                .ChildRules(answer =>
                {
                    answer.RuleFor(x => x.ExamQuestionId)
                          .NotEmpty()
                          .WithMessage("ExamQuestionId is required.");
                });

            RuleFor(x => x.Answers)
                .Must(answers => answers != null &&
                    answers.Select(answer => answer.ExamQuestionId).Distinct().Count() == answers.Count)
                .WithMessage("Each exam question may only be submitted once.");
        }
    }
}
