using Auth.Contarcts;
using Auth.Models;
using ELearningProject.Contracts;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace ELearningProject.Features.Auth.UpdateUserProfile
{
    public class UpdateUserProfileOrchestrator : IUpdateUserProfileOrchestrator
    {
        private readonly IMediator _mediator;
        private readonly IImageHelper _imageHelper;
        private readonly UserManager<ApplicationUser> _userManager;

        public UpdateUserProfileOrchestrator(
            IMediator mediator,
            IImageHelper imageHelper,
            UserManager<ApplicationUser> userManager)
        {
            _mediator = mediator;
            _imageHelper = imageHelper;
            _userManager = userManager;
        }

        public async Task<UpdateUserProfileResponse> UpdateUserProfileAsync(Guid userId, UpdateUserProfileRequest request)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                throw new KeyNotFoundException("User not found.");

            var currentImageUrl = user.ProfileImageUrl;
            var imageUrl = currentImageUrl;
            var uploadedNewImage = false;

            if (request.ProfileImage is not null && request.ProfileImage.Length > 0)
            {
                imageUrl = await _imageHelper.SaveImageAsync(request.ProfileImage, "Users");
                uploadedNewImage = true;
            }

            try
            {
                var command = new UpdateUserProfileCommand(
                    userId,
                    request.FirstName,
                    request.LastName,
                    request.PhoneNumber,
                    imageUrl
                );

                var response = await _mediator.Send(command);

                if (uploadedNewImage &&
                    !string.IsNullOrWhiteSpace(currentImageUrl) &&
                    !string.Equals(currentImageUrl, imageUrl, StringComparison.OrdinalIgnoreCase))
                {
                    _imageHelper.DeleteImage(currentImageUrl);
                }

                return response;
            }
            catch
            {
                if (uploadedNewImage &&
                    !string.IsNullOrWhiteSpace(imageUrl) &&
                    !string.Equals(currentImageUrl, imageUrl, StringComparison.OrdinalIgnoreCase))
                {
                    _imageHelper.DeleteImage(imageUrl);
                }

                throw;
            }
        }
    }
}
