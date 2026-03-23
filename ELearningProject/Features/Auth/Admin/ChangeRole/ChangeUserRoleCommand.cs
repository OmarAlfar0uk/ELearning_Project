using ELearningProject.Features.Shared;
using MediatR;

namespace ELearningProject.Features.Auth.Admin.ChangeRole
{
    public class ChangeUserRoleCommand
       : IRequest<EndpointResponse<string>>
    {
        public Guid UserId { get; set; }
        public string NewRole { get; set; } = default!;
    }
}
