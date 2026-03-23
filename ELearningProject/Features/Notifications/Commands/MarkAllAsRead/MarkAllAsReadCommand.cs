
using ELearningProject.Features.Shared;
using MediatR;

namespace ELearningProject.Features.Notifications.Commands.MarkAllAsRead
{
    public record MarkAllAsReadCommand(Guid UserId) : IRequest<RequestResponse<string>>;
}
