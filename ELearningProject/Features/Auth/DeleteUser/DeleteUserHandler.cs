using Auth.Models;
using ELearningProject.Features.Admin.DeleteUser;
using ELearningProject.Features.Shared;

using MediatR;
using Microsoft.AspNetCore.Identity;

namespace ELearningProject.Features.Auth.DeleteUser
{
    public class DeleteUserHandler
        : IRequestHandler<DeleteUserCommand, RequestResponse<bool>>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public DeleteUserHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<RequestResponse<bool>> Handle(
            DeleteUserCommand request,
            CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return RequestResponse<bool>.Fail("User not found");

            // ❌ ممنوع حذف SuperAdmin
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("SuperAdmin"))
                return RequestResponse<bool>.Fail("Cannot delete SuperAdmin");

           
          
            user.IsDeleted = true;
            user.UpdatedAt = DateTime.UtcNow;

            await _userManager.UpdateAsync(user);

            return RequestResponse<bool>.Success(
                true,
                "User deleted successfully"
            );
        }
    }
}