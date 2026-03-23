using ELearningProject.Features.Shared;
using MediatR;

namespace ELearningProject.Features.Auth.ChangeUserRole
{
    public record ChangeUserRoleCommand(
        string TargetUserEmail,
        string NewRole
    ) : IRequest<RequestResponse<bool>>;
}
