
using ELearningProject.Contarcts;
using ELearningProject.Features.Shared;
using ELearningProject.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ELearningProject.Features.Notifications.Queries.GetUnreadCount
{
    public class GetUnreadCountHandler : IRequestHandler<GetUnreadCountQuery, EndpointResponse<int>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetUnreadCountHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<EndpointResponse<int>> Handle(GetUnreadCountQuery request, CancellationToken cancellationToken)
        {
            var count = await _unitOfWork.GetRepository<Notification>()
                .FindByCondition(n => n.UserId == request.UserId && !n.IsRead)
                .CountAsync(cancellationToken);

            return EndpointResponse<int>.SuccessResponse(count);
        }
    }
}
