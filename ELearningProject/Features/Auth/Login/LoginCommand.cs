using ELearningProject.Features.Shared;
using MediatR;

namespace ELearningProject.Features.Auth.Login
{
    public class LoginCommand : IRequest<EndpointResponse<LoginResponse>>
    {
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
        public bool RememberMe { get; set; } = false;
    }
}
