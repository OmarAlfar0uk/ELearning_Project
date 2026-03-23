
using ELearningProject.Contarcts;
using ELearningProject.Features.Shared;
using ELearningProject.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ELearningProject.Features.Notifications.Commands.MarkAsRead
{
    public class MarkAsReadHandler : IRequestHandler<MarkAsReadCommand, RequestResponse<string>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public MarkAsReadHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<RequestResponse<string>> Handle(MarkAsReadCommand request, CancellationToken cancellationToken)
        {
            var notificationRepository = _unitOfWork.GetRepository<Notification>();

            // 1. Get Notification & Verify Ownership
            var notification = await notificationRepository.FindByCondition(n => n.Id == request.NotificationId)
                .FirstOrDefaultAsync(cancellationToken);

            if (notification == null)
            {
                return RequestResponse<string>.Fail("Notification not found.");
            }

            if (notification.UserId != request.UserId)
            {
                return RequestResponse<string>.Fail("Unauthorized access to this notification."); // Or 404 to hide existence
            }

            // 2. Mark as Read
            if (!notification.IsRead)
            {
                notification.IsRead = true;
                notificationRepository.Update(notification);
                await _unitOfWork.SaveChangesAsync();
            }

            return RequestResponse<string>.Success(null, "Notification marked as read.");
        }
    }
}
