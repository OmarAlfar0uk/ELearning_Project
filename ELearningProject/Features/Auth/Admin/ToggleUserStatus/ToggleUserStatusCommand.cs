using ELearningProject.Features.Shared;
using MediatR;

namespace ELearningProject.Features.Auth.Admin.ToggleUserStatus
{
    public class ToggleUserStatusCommand
        : IRequest<EndpointResponse<string>>
    {
        public Guid UserId { get; set; }
        public bool Enable { get; set; } 
    }
}
