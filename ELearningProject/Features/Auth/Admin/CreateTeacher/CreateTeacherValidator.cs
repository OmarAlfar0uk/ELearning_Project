using FluentValidation;

namespace ELearningProject.Features.Auth.Admin.CreateTeacher
{
    /// <summary>
    /// Validator for CreateTeacherCommand ensuring valid inputs.
    /// </summary>
    public class CreateTeacherValidator : AbstractValidator<CreateTeacherCommand>
    {
        public CreateTeacherValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.");

            RuleFor(x => x.TrackIds)
                .NotEmpty().WithMessage("TrackIds must have at least one entry.")
                .Must(x => x != null && x.Count > 0).WithMessage("TrackIds must have at least one entry.");
        }
    }
}
