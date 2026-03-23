using Auth.Contarcts;
using Auth.Models;
using ELearningProject.Features.Shared;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ELearningProject.Features.Auth.Logout
{
    public class LogoutHandler : IRequestHandler<LogoutCommand, EndpointResponse<string>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;

        public LogoutHandler(
            UserManager<ApplicationUser> userManager,
            ITokenService tokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
        }

        public async Task<EndpointResponse<string>> Handle(
            LogoutCommand request,
            CancellationToken cancellationToken)
        {
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken, cancellationToken);

            if (user == null)
            {
                return EndpointResponse<string>.UnauthorizedResponse(
                    "Invalid refresh token");
            }

            await _tokenService.RevokeRefreshTokenAsync(user);

            return EndpointResponse<string>.SuccessResponse(
                "Logged out successfully",
                "Logout successful"
            );
        }
    }
}
