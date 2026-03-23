using Auth.Models;
using ELearningProject.Features.Shared;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace ELearningProject.Features.Auth.ChangeUserRole
{
    public class ChangeUserRoleHandler
       : IRequestHandler<ChangeUserRoleCommand, RequestResponse<bool>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public ChangeUserRoleHandler(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<RequestResponse<bool>> Handle(
            ChangeUserRoleCommand request,
            CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.TargetUserEmail);
            if (user == null)
                return RequestResponse<bool>.Fail("User not found");

            if (!await _roleManager.RoleExistsAsync(request.NewRole))
                return RequestResponse<bool>.Fail("Role does not exist");

            var currentRoles = await _userManager.GetRolesAsync(user);

            // 🔒 ممنوع تعديل SuperAdmin
            if (currentRoles.Contains("SuperAdmin"))
                return RequestResponse<bool>.Fail("Cannot change SuperAdmin role");

            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, request.NewRole);

            return RequestResponse<bool>.Success(
                true,
                $"User role changed to {request.NewRole}"
            );
        }
    }
}
