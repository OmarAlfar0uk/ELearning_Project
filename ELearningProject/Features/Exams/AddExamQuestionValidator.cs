using ELearningProject.Models;
using FluentValidation;

namespace ELearningProject.Features.Exams
{
    /// <summary>
    /// Validator for AddExamQuestionCommand and its answer options.
    /// </summary>
    public class AddExamQuestionValidator : AbstractValidator<AddExamQuestionCommand>
    {
        public AddExamQuestionValidator()
        {
            RuleFor(x => x.ExamId)
                .NotEmpty()
                .WithMessage("ExamId is required.");

            RuleFor(x => x.Text)
                .NotEmpty()
                .WithMessage("Text is required.");

            RuleFor(x => x.Points)
                .GreaterThan(0)
                .WithMessage("Points must be greater than zero.");

            RuleForEach(x => x.Options)
                .ChildRules(option =>
                {
                    option.RuleFor(x => x.Text)
                          .NotEmpty()
                          .WithMessage("Option text is required.");
                });

            RuleFor(x => x)
                .Must(HasValidOptions)
                .WithMessage(x => x.Type == QuestionType.MultipleChoice || x.Type == QuestionType.TrueFalse
                    ? "MultipleChoice and TrueFalse questions must have at least two options with exactly one correct option."
                    : "Essay and FillInBlank questions must not have options.");
        }

        private static bool HasValidOptions(AddExamQuestionCommand command)
        {
            var options = command.Options ?? new List<CreateOptionRequest>();

            if (command.Type == QuestionType.MultipleChoice || command.Type == QuestionType.TrueFalse)
            {
                return options.Count >= 2 && options.Count(option => option.IsCorrect) == 1;
            }

            return options.Count == 0;
        }
    }
}
