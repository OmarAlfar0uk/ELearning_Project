
using ELearningProject.Features.Shared;
using MediatR;

namespace ELearningProject.Features.Notifications.Queries.GetUnreadCount
{
    public record GetUnreadCountQuery(Guid UserId) : IRequest<EndpointResponse<int>>;
}
