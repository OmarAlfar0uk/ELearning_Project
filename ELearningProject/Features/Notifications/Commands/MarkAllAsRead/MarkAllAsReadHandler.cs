
using ELearningProject.Contarcts;
using ELearningProject.Features.Shared;
using ELearningProject.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ELearningProject.Features.Notifications.Commands.MarkAllAsRead
{
    public class MarkAllAsReadHandler : IRequestHandler<MarkAllAsReadCommand, RequestResponse<string>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public MarkAllAsReadHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<RequestResponse<string>> Handle(MarkAllAsReadCommand request, CancellationToken cancellationToken)
        {
            var notificationRepository = _unitOfWork.GetRepository<Notification>();

            // 1. Get all unread for user
            var unreadNotifications = await notificationRepository.FindByCondition(n => n.UserId == request.UserId && !n.IsRead)
                .ToListAsync(cancellationToken);

            if (!unreadNotifications.Any())
            {
                return RequestResponse<string>.Success(null, "No unread notifications.");
            }

            // 2. Update all
            foreach (var n in unreadNotifications)
            {
                n.IsRead = true;
                notificationRepository.Update(n);
            }

            await _unitOfWork.SaveChangesAsync();

            return RequestResponse<string>.Success(null, "All notifications marked as read.");
        }
    }
}
