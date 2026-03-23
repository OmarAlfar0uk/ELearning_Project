
using ELearningProject.Models;

namespace ELearningProject.Features.Notifications.DTOs
{
    public record NotificationDto(
        Guid Id,
        string Title,
        string Message,
        string Type,
        string? ActionUrl,
        bool IsRead,
        DateTime CreatedAt
    )
    {
        public NotificationDto(Guid id, string title, string message, NotificationType type, string? actionUrl, bool isRead, DateTime createdAt)
            : this(id, title, message, type.ToString(), actionUrl, isRead, createdAt)
        {
        }
    }
}
