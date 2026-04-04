using Auth.Models;
using ELearningProject.Features.Auth.Admin.GetUsers;
using ELearningProject.Features.Shared;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace ELearningProject.Features.Auth.Admin.GetAdmins
{
    public class GetAdminsHandler : IRequestHandler<GetAdminsQuery, EndpointResponse<List<UserListItemDto>>>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public GetAdminsHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<EndpointResponse<List<UserListItemDto>>> Handle(
            GetAdminsQuery request,
            CancellationToken cancellationToken)
        {
            var roleNames = request.IncludeSuperAdmins
                ? new[] { "Admin", "SuperAdmin" }
                : new[] { "Admin" };

            var adminsById = new Dictionary<Guid, ApplicationUser>();

            foreach (var roleName in roleNames)
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(roleName);

                foreach (var user in usersInRole.Where(u => !u.IsDeleted))
                {
                    adminsById[user.Id] = user;
                }
            }

            IEnumerable<ApplicationUser> adminsQuery = adminsById.Values;

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim();

                adminsQuery = adminsQuery.Where(user =>
                    (!string.IsNullOrWhiteSpace(user.Email) &&
                     user.Email.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                    user.FullName.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            var admins = adminsQuery
                .OrderByDescending(user => user.CreatedAt)
                .ToList();

            var items = new List<UserListItemDto>(admins.Count);

            foreach (var admin in admins)
            {
                var roles = await _userManager.GetRolesAsync(admin);

                items.Add(new UserListItemDto
                {
                    UserId = admin.Id,
                    Email = admin.Email ?? string.Empty,
                    FullName = admin.FullName,
                    IsActivated = admin.IsActivated,
                    Roles = roles.ToList(),
                    CreatedAt = admin.CreatedAt
                });
            }

            return EndpointResponse<List<UserListItemDto>>.SuccessResponse(
                items,
                "Admins retrieved successfully");
        }
    }
}
