using Auth.Contarcts;
using Auth.Models;
using ELearningProject.Features.Shared;
using ELearningProject.Data;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Serilog;

namespace ELearningProject.Features.Auth.Activate
{
    public class ActivateHandler
      : IRequestHandler<ActivateCommand, EndpointResponse<ActivateResponse>>
    {
        private readonly UniversitySystemAuthContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMemoryCache _cache;
        private readonly ITokenService _tokenService;

        public ActivateHandler(
            UniversitySystemAuthContext context,
            UserManager<ApplicationUser> userManager,
            ITokenService tokenService,
            IMemoryCache cache)
        {
            _context = context;
            _userManager = userManager;
            _tokenService = tokenService;
            _cache = cache;
        }

        public async Task<EndpointResponse<ActivateResponse>> Handle(
            ActivateCommand request,
            CancellationToken cancellationToken)
        {
            // 1️⃣ Validate activation code
            var activationCode = await _context.ActivationCodes
                .FirstOrDefaultAsync(x =>
                    x.Code == request.Code &&
                    !x.IsUsed &&
                    x.ExpiryDate > DateTime.UtcNow,
                    cancellationToken);

            if (activationCode == null)
            {
                return EndpointResponse<ActivateResponse>
                    .NotFoundResponse("Invalid or expired activation code");
            }

            // 2️⃣ Get user
            var user = await _userManager.FindByIdAsync(activationCode.UserId.ToString());
            if (user == null)
            {
                return EndpointResponse<ActivateResponse>
                    .NotFoundResponse("User not found");
            }

            if (user.IsActivated)
            {
                return EndpointResponse<ActivateResponse>
                    .ErrorResponse("Account already activated", 409);
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                // 3️⃣ Set password
                var passwordResult =
                    await _userManager.AddPasswordAsync(user, request.Password);

                if (!passwordResult.Succeeded)
                {
                    await transaction.RollbackAsync(cancellationToken);

                    return EndpointResponse<ActivateResponse>.ErrorResponse(
                        "Failed to set password",
                        400,
                        passwordResult.Errors.Select(e => e.Description).ToList()
                    );
                }

                // 4️⃣ Activate user
                user.IsActivated = true;

                // 5️⃣ Assign role (only if not exists)
                if (!await _userManager.IsInRoleAsync(user, activationCode.Role))
                {
                    await _userManager.AddToRoleAsync(user, activationCode.Role);
                }

                // 6️⃣ Invalidate role cache
                _cache.Remove($"roles_{user.Id}");

                // 7️⃣ Mark code as used
                activationCode.IsUsed = true;

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                // 8️⃣ Generate tokens
                var (accessToken, refreshToken) =
                    await _tokenService.GenerateTokensAsync(user, rememberMe: false);

                return EndpointResponse<ActivateResponse>.SuccessResponse(
                    new ActivateResponse
                    {
                        AccessToken = accessToken,
                        RefreshToken = refreshToken,
                        Role = activationCode.Role
                    },
                    "Account activated successfully"
                );
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);

                Log.Error(ex, "Activation failed for Code={Code}", request.Code);

                return EndpointResponse<ActivateResponse>.ErrorResponse(
                    "Unexpected error occurred during activation",
                    500
                );
            }
        }
    }
}
