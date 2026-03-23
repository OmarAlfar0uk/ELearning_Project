
using ELearningProject.Contarcts;
using ELearningProject.Features.Shared;
using ELearningProject.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ELearningProject.Extensions;

namespace ELearningProject.Features.Assignments.CloseAssignment
{
    public class CloseAssignmentHandler : IRequestHandler<CloseAssignmentCommand, RequestResponse<string>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ELearningProject.Features.Notifications.Services.INotificationService _notificationService;
        private readonly Microsoft.AspNetCore.Http.IHttpContextAccessor _httpContextAccessor;

        public CloseAssignmentHandler(
            IUnitOfWork unitOfWork, 
            ELearningProject.Features.Notifications.Services.INotificationService notificationService,
            Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<RequestResponse<string>> Handle(CloseAssignmentCommand request, CancellationToken cancellationToken)
        {
            var assignmentRepository = _unitOfWork.GetRepository<Assignment>();

            var assignment = await assignmentRepository.GetByIdAsync(request.AssignmentId);
            if (assignment == null)
            {
                return RequestResponse<string>.Fail("Assignment not found.");
            }

            // RBAC Check for Instructor
            var user = _httpContextAccessor.HttpContext?.User;
            var userId = ELearningProject.Extensions.ClaimsPrincipalExtensions.GetUserId(user!);
            var userRole = ELearningProject.Extensions.ClaimsPrincipalExtensions.GetUserRole(user!);

            if (userRole != "Admin" && userRole != "SuperAdmin")
            {
                return RequestResponse<string>.Fail("You don't have permission to close this assignment.");
            }

            if (assignment.IsClosed)
            {
                return RequestResponse<string>.Fail("Assignment is already closed.");
            }

            assignment.IsClosed = true;
            assignmentRepository.Update(assignment);
            await _unitOfWork.SaveChangesAsync();

           
            var students = await _unitOfWork.GetRepository<Models.BatchStudent>()
                .FindByCondition(bs => bs.Batch.Tracks.Any(t => t.Id == assignment.TrackId))
                .Select(bs => bs.StudentId)
                .ToListAsync(cancellationToken);

            foreach (var studentId in students)
            {
                await _notificationService.CreateNotificationAsync(
                    studentId,
                    "Assignment Closed",
                    $"The assignment '{assignment.Title}' has been closed.",
                    ELearningProject.Models.NotificationType.AssignmentClosed,
                    $"/assignments/{assignment.Id}/details"
                );
            }

            return RequestResponse<string>.Success(null, "Assignment closed successfully.");
        }
    }
}
