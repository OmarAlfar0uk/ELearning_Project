using Auth.Contarcts;
using Auth.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace ELearningProject.Features.Auth.GetCurrentUser
{
    public class GetCurrentUserHandler : IRequestHandler<GetCurrentUserQuery, CurrentUserResponse>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IImageHelper _imageHelper;

        public GetCurrentUserHandler(
            UserManager<ApplicationUser> userManager,
            IImageHelper imageHelper)
        {
            _userManager = userManager;
            _imageHelper = imageHelper;
        }

        public async Task<CurrentUserResponse> Handle(
            GetCurrentUserQuery request,
            CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
                throw new KeyNotFoundException("User not found.");

            var roles = await _userManager.GetRolesAsync(user);

            return new CurrentUserResponse
            {
                UserId = user.Id,
                UserName = user.UserName ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = $"{user.FirstName} {user.LastName}",
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                ProfileImageUrl = _imageHelper.GetImageUrl(user.ProfileImageUrl),
                Roles = roles.ToList()
            };
        }
    }
}
