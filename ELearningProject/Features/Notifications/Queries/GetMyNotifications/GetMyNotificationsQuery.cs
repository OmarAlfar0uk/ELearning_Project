
using ELearningProject.Features.Notifications.DTOs;
using ELearningProject.Features.Shared;
using MediatR;
using System.Text.Json.Serialization;

namespace ELearningProject.Features.Notifications.Queries.GetMyNotifications
{
    public class GetMyNotificationsQuery : IRequest<EndpointResponse<PaginatedResult<NotificationDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        
        [JsonIgnore]
        public Guid UserId { get; internal set; }
    }
}
