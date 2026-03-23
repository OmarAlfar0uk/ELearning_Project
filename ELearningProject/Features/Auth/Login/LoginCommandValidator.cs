using FluentValidation;

namespace ELearningProject.Features.Auth.Login
{
    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty();

            RuleFor(x => x.Password)
                .NotEmpty();
        }
    }
}
