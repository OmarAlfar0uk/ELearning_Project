
using ELearningProject.Contarcts;
using ELearningProject.Features.Notifications.DTOs;
using ELearningProject.Features.Shared;
using ELearningProject.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ELearningProject.Features.Notifications.Queries.GetMyNotifications
{
    public class GetMyNotificationsHandler : IRequestHandler<GetMyNotificationsQuery, EndpointResponse<PaginatedResult<NotificationDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetMyNotificationsHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<EndpointResponse<PaginatedResult<NotificationDto>>> Handle(GetMyNotificationsQuery request, CancellationToken cancellationToken)
        {
            var notificationRepository = _unitOfWork.GetRepository<Notification>();

            // 1. Build Query
            var query = notificationRepository.FindByCondition(n => n.UserId == request.UserId);

            // 2. Count Total
            var totalCount = await query.CountAsync(cancellationToken);

            // 3. Ordering: Unread first, then CreatedAt Descending
            // Note: EF Core translation for complex sorting is usually fine.
            query = query.OrderBy(n => n.IsRead) // False (0) first, True (1) last
                         .ThenByDescending(n => n.CreatedAt); // Newest first

            // 4. Pagination
            var items = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(n => new NotificationDto(
                    n.Id,
                    n.Title,
                    n.Message,
                    n.Type,
                    n.ActionUrl,
                    n.IsRead,
                    n.CreatedAt)) // CreatedAt is in BaseEntity? Need to check.
                .ToListAsync(cancellationToken);

            var result = new PaginatedResult<NotificationDto>(items, totalCount, request.PageNumber, request.PageSize);

            return EndpointResponse<PaginatedResult<NotificationDto>>.SuccessResponse(result);
        }
    }
}
