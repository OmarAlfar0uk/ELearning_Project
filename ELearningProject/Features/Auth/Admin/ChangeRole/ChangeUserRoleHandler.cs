using Auth.Models;
using ELearningProject.Features.Shared;
using ELearningProject.Contracts;
using ELearningProject.Features.Auth.Admin.ChangeRole;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace ELearningProject.Features.Admin.ChangeRole
{
    public class ChangeUserRoleHandler
        : IRequestHandler<ChangeUserRoleCommand, EndpointResponse<string>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IAuditLogger _auditLogger;
        public ChangeUserRoleHandler(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager ,
            IAuditLogger auditLogger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _auditLogger = auditLogger;
        }

        public async Task<EndpointResponse<string>> Handle(
            ChangeUserRoleCommand request,
            CancellationToken cancellationToken)
        {
            // 1️⃣ Check user
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
            {
                return EndpointResponse<string>.NotFoundResponse("User not found");
            }

            // 2️⃣ Check role exists
            var roleExists = await _roleManager.RoleExistsAsync(request.NewRole);
            if (!roleExists)
            {
                return EndpointResponse<string>.ErrorResponse(
                    "Role does not exist",
                    400
                );
            }

            // 3️⃣ Get current roles
            var currentRoles = await _userManager.GetRolesAsync(user);

            // 🔐 Optional safety: prevent removing last SuperAdmin
            if (currentRoles.Contains("SuperAdmin") && request.NewRole != "SuperAdmin")
            {
                return EndpointResponse<string>.ErrorResponse(
                    "Cannot change role of SuperAdmin",
                    403
                );
            }

            // 4️⃣ Remove old roles
            if (currentRoles.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
            }

            // 5️⃣ Assign new role
            await _userManager.AddToRoleAsync(user, request.NewRole);
            await _auditLogger.LogAsync(
            action: "ChangeUserRole",
            targetId: user.Id.ToString(),
            description: $"Role changed to {request.NewRole}"
               );

            return EndpointResponse<string>.SuccessResponse(
                $"User role changed to {request.NewRole}",
                "Role updated successfully"
            );
        }
    }
}
