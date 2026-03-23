using MediatR;

namespace ELearningProject.Features.Auth.ForgetPassword.OTP
{
    public record VerifyOtpCommand(string Email, string OtpCode) : IRequest<bool>;

}
