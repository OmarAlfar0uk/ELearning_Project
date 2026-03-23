using ELearningProject.Features.Shared;
using MediatR;

namespace ELearningProject.Features.Auth.Refresh
{
    public class RefreshTokenCommand : IRequest<EndpointResponse<RefreshTokenResponse>>
    {
        public string RefreshToken { get; set; } = default!;
    }
}
