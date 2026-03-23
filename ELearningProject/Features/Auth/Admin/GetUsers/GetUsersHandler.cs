using Auth.Models;
using ELearningProject.Features.Shared;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ELearningProject.Features.Auth.Admin.GetUsers
{
    public class GetUsersHandler : IRequestHandler<GetUsersQuery, EndpointResponse<PaginatedResult<UserListItemDto>>>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public GetUsersHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<EndpointResponse<PaginatedResult<UserListItemDto>>> Handle(
            GetUsersQuery request,
            CancellationToken cancellationToken)
        {
            var query = _userManager.Users.AsQueryable();

            // 🔍 Search
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                query = query.Where(u =>
            u.Email!.Contains(request.Search) ||
            (u.FirstName + " " + u.LastName).Contains(request.Search));

            }

            var totalCount = await query.CountAsync(cancellationToken);

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var items = new List<UserListItemDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                items.Add(new UserListItemDto
                {
                    UserId = user.Id,
                    Email = user.Email!,
                    FullName = user.FullName,
                    IsActivated = user.IsActivated,
                    Roles = roles.ToList(),
                    CreatedAt = user.CreatedAt
                });
            }

            var result = new PaginatedResult<UserListItemDto>(
                items,
                totalCount,
                request.Page,
                request.PageSize
            );

            return EndpointResponse<PaginatedResult<UserListItemDto>>.SuccessResponse(
                result,
                "Users retrieved successfully"
            );
        }
    }
}
