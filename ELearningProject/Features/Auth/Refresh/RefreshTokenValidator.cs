using FluentValidation;

namespace ELearningProject.Features.Auth.Refresh
{
    public class RefreshTokenValidator : AbstractValidator<RefreshTokenCommand>
    {
        public RefreshTokenValidator()
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty();
        }
    }
}
