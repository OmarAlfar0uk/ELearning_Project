using MediatR;

namespace ELearningProject.Features.Auth.ForgetPassword.ResetPassword
{
    public record ResetPasswordCommand(string Email, string NewPassword) : IRequest<bool>;

}
