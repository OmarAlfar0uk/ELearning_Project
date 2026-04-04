using ELearningProject.Features.Auth.Admin.GetUsers;
using ELearningProject.Features.Shared;
using MediatR;

namespace ELearningProject.Features.Auth.Admin.GetAdmins
{
    public class GetAdminsQuery : IRequest<EndpointResponse<List<UserListItemDto>>>
    {
        public string? Search { get; set; }
        public bool IncludeSuperAdmins { get; set; } = true;
    }
}
