using FluentValidation;

namespace ELearningProject.Features.Auth.Logout
{
    public class LogoutValidator : AbstractValidator<LogoutCommand>
    {
        public LogoutValidator()
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty();
        }
    }
}
