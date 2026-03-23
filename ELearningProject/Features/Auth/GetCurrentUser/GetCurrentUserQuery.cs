using MediatR;
using ELearningProject.Features.Auth.Login;

namespace ELearningProject.Features.Auth.GetCurrentUser
{
    public record GetCurrentUserQuery(Guid UserId) : IRequest<CurrentUserResponse>;

}
