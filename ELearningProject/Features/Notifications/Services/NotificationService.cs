
using ELearningProject.Contarcts;
using ELearningProject.Models;
using Microsoft.AspNetCore.SignalR;
using ELearningProject.Features.Notifications.Hubs;
using ELearningProject.Features.Notifications.DTOs;

namespace ELearningProject.Features.Notifications.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(
            IUnitOfWork unitOfWork,
            IHubContext<NotificationHub> hubContext)
        {
            _unitOfWork = unitOfWork;
            _hubContext = hubContext;
        }

        public async Task CreateNotificationAsync(Guid userId, string title, string message, NotificationType type, string? actionUrl = null)
        {
            var notificationRepository = _unitOfWork.GetRepository<Notification>();

            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                ActionUrl = actionUrl,
                IsRead = false
            };

            await notificationRepository.CreateAsync(notification);
            await _unitOfWork.SaveChangesAsync(); // ← persist now so ID is real

            var dto = new NotificationDto(
                notification.Id,
                title,
                message,
                type.ToString(),
                actionUrl,
                false,
                notification.CreatedAt
            );

            // Send real-time notification via SignalR
            await _hubContext.Clients.Group(userId.ToString()).SendAsync("ReceiveNotification", dto);
        }
    }
}
