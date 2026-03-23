
using ELearningProject.Features.Shared;
using MediatR;
using System.Text.Json.Serialization;

namespace ELearningProject.Features.Notifications.Commands.MarkAsRead
{
    public class MarkAsReadCommand : IRequest<RequestResponse<string>>
    {
        public Guid NotificationId { get; set; }
        
        [JsonIgnore]
        public Guid UserId { get; set; }
    }
}
