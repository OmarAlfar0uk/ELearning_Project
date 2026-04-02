using MediatR;
using ELearningProject.Features.Shared;

namespace ELearningProject.Features.Admin.DeleteUser
{
    public record DeleteUserCommand(Guid UserId)
        : IRequest<RequestResponse<bool>>;
}
