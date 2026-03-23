using MediatR;

namespace ELearningProject.Features.Auth.ChangePassword
{
    public record ChangePasswordCommand(
       string CurrentPassword,
       string NewPassword
   ) : IRequest<bool>;
}

