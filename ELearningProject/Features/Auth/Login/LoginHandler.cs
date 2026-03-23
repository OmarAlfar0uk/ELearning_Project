using Auth.Contarcts;
using Auth.Models;
using ELearningProject.Features.Shared;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace ELearningProject.Features.Auth.Login
{
    public class LoginHandler : IRequestHandler<LoginCommand, EndpointResponse<LoginResponse>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;

        public LoginHandler(
            UserManager<ApplicationUser> userManager,
            ITokenService tokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
        }

        public async Task<EndpointResponse<LoginResponse>> Handle(
            LoginCommand request,
            CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);


            if (user == null)
            {
                return EndpointResponse<LoginResponse>.UnauthorizedResponse(
                    "Invalid Email or password");
            }

            var isPasswordValid =
                await _userManager.CheckPasswordAsync(user, request.Password);

            if (!isPasswordValid)
            {
                return EndpointResponse<LoginResponse>.UnauthorizedResponse(
                    "Invalid Email or password");
            }

            if (!user.IsActivated)
            {
                return EndpointResponse<LoginResponse>.ErrorResponse(
                    "Account is not activated yet",
                    403
                );
            }

            var roles = await _userManager.GetRolesAsync(user);

            var tokens = await _tokenService.GenerateTokensAsync(
                user,
                request.RememberMe
            );

            return EndpointResponse<LoginResponse>.SuccessResponse(
                new LoginResponse
                {
                    AccessToken = tokens.AccessToken,
                    RefreshToken = tokens.RefreshToken,
                    Roles = roles.ToList()
                },
                "Login successful"
            );
        }
    }
}
