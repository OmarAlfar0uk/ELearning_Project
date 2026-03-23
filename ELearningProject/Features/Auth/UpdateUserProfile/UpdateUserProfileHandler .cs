using Auth.Contarcts;
using Auth.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace ELearningProject.Features.Auth.UpdateUserProfile
{
    public class UpdateUserProfileHandler : IRequestHandler<UpdateUserProfileCommand, UpdateUserProfileResponse>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly IImageHelper _imageHelper;

        public UpdateUserProfileHandler(
            UserManager<ApplicationUser> userManager,
            ITokenService tokenService,
            IImageHelper imageHelper)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _imageHelper = imageHelper;
        }

        public async Task<UpdateUserProfileResponse> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
                throw new KeyNotFoundException("User not found.");

            if (!string.IsNullOrWhiteSpace(request.FirstName))
                user.FirstName = request.FirstName;

            if (!string.IsNullOrWhiteSpace(request.LastName))
                user.LastName = request.LastName;

            if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
                user.PhoneNumber = request.PhoneNumber;

            if (!string.IsNullOrWhiteSpace(request.ProfileImage))
                user.ProfileImageUrl = request.ProfileImage;

            user.UpdatedAt = DateTime.UtcNow;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                var errors = string.Join("; ", updateResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to update profile: {errors}");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var tokens = await _tokenService.GenerateTokensAsync(user, false);

            return new UpdateUserProfileResponse
            {
                Success = true,
                Message = "Profile updated successfully",
                UserId = user.Id,
                UserName = user.UserName ?? string.Empty,
                FirstName = user.FirstName,
                lastName = user.LastName,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                ProfileImageUrl = _imageHelper.GetImageUrl(user.ProfileImageUrl),
                Roles = roles,
                Token = tokens.AccessToken,
                RefreshToken = tokens.RefreshToken
            };
        }
    }
}
