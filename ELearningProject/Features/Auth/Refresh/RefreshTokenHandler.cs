using Auth.Contarcts;
using ELearningProject.Features.Shared;
using MediatR;

namespace ELearningProject.Features.Auth.Refresh
{
    public class RefreshTokenHandler
        : IRequestHandler<RefreshTokenCommand, EndpointResponse<RefreshTokenResponse>>
    {
        private readonly ITokenService _tokenService;

        public RefreshTokenHandler(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

        public async Task<EndpointResponse<RefreshTokenResponse>> Handle(
            RefreshTokenCommand request,
            CancellationToken cancellationToken)
        {
            var newAccessToken =
                await _tokenService.RefreshAccessTokenAsync(request.RefreshToken);

            if (string.IsNullOrEmpty(newAccessToken))
            {
                return EndpointResponse<RefreshTokenResponse>.UnauthorizedResponse(
                    "Invalid or expired refresh token");
            }

            return EndpointResponse<RefreshTokenResponse>.SuccessResponse(
                new RefreshTokenResponse
                {
                    AccessToken = newAccessToken
                },
                "Token refreshed successfully"
            );
        }
    }
}
