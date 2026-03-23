using MediatR;

namespace ELearningProject.Features.Auth.ForgetPassword.OTP
{
    public record SendOtpCommand(string Email) : IRequest<bool>;

}
