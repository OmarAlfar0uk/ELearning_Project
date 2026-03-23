using MediatR;
using ELearningProject.Features.Shared;

namespace ELearningProject.Features.Admin.DeleteUser
{
    public record DeleteUserCommand(string Email)
        : IRequest<RequestResponse<bool>>;
}
