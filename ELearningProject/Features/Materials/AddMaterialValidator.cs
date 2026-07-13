using FluentValidation;

namespace ELearningProject.Features.Materials
{
    /// <summary>
    /// Validator for AddMaterialCommand ensuring required inputs and exactly one resource URL.
    /// </summary>
    public class AddMaterialValidator : AbstractValidator<AddMaterialCommand>
    {
        public AddMaterialValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.");

            RuleFor(x => x.LectureId)
                .NotEmpty().WithMessage("LectureId is required.");

            RuleFor(x => x)
                .Must(x => (!string.IsNullOrWhiteSpace(x.FileUrl) && string.IsNullOrWhiteSpace(x.ExternalLink)) ||
                           (string.IsNullOrWhiteSpace(x.FileUrl) && !string.IsNullOrWhiteSpace(x.ExternalLink)))
                .WithMessage("Exactly one of FileUrl or ExternalLink must be provided.");
        }
    }
}
