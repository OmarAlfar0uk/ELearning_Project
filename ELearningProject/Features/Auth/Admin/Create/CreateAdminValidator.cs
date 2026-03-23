using FluentValidation;

namespace ELearningProject.Features.Auth.Admin.Create
{
    public class CreateAdminValidator : AbstractValidator<CreateAdminCommand>
    {
        public CreateAdminValidator()
        {
            RuleFor(x => x.Email)
                .EmailAddress()
                .NotEmpty();

            RuleFor(x => x.Password)
                .MinimumLength(6);

            RuleFor(x => x.FirstName)
                .NotEmpty();

            RuleFor(x => x.LastName)
                .NotEmpty();
        }
    }
}
