using ELearningProject.Features.Shared;
using MediatR;

namespace ELearningProject.Features.Auth.Logout
{
    public class LogoutCommand : IRequest<EndpointResponse<string>>
    {
        public string RefreshToken { get; set; } = default!;
    }
}
