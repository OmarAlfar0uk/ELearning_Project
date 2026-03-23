
namespace ELearningProject.Features.Notifications.Services
{
    public interface INotificationService
    {
        Task CreateNotificationAsync(Guid userId, string title, string message, ELearningProject.Models.NotificationType type, string? actionUrl = null);
    }
}
