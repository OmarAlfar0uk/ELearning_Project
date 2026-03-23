
using Auth.Models;
using ELearningProject.Contarcts;
using ELearningProject.Features.Shared;
using ELearningProject.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ELearningProject.Features.Submissions.SubmitAssignment
{
    public class SubmitAssignmentHandler : IRequestHandler<SubmitAssignmentCommand, EndpointResponse<string>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ELearningProject.Features.Notifications.Services.INotificationService _notificationService;
        private readonly Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> _userManager;
        private readonly ELearningProject.Contracts.IFileService _fileService;

        public SubmitAssignmentHandler(
            IUnitOfWork unitOfWork, 
            ELearningProject.Features.Notifications.Services.INotificationService notificationService,
            Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> userManager,
            ELearningProject.Contracts.IFileService fileService)
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
            _userManager = userManager;
            _fileService = fileService;
        }

        public async Task<EndpointResponse<string>> Handle(SubmitAssignmentCommand request, CancellationToken cancellationToken)
        {
            var assignmentRepository = _unitOfWork.GetRepository<Assignment>();
            var submissionRepository = _unitOfWork.GetRepository<Submission>();
            var batchStudentRepository = _unitOfWork.GetRepository<BatchStudent>();

            var assignment = await assignmentRepository.FindByCondition(a => a.Id == request.AssignmentId)
                .Include(a => a.Lecture)
                    .ThenInclude(l => l.Track)
                .FirstOrDefaultAsync(cancellationToken);

            if (assignment == null)
            {
                return EndpointResponse<string>.NotFoundResponse("Assignment not found.");
            }

            // 2. Check Due Date and IsClosed
            if (assignment.IsClosed)
            {
                return EndpointResponse<string>.ErrorResponse("Assignment is closed.", 400);
            }

            if (assignment.DueDate.HasValue && assignment.DueDate.Value < DateTime.UtcNow)
            {
                 return EndpointResponse<string>.ErrorResponse("Assignment due date has passed.", 400);
            }

           
            var trackBatchId = assignment.Lecture.Track.BatchId;
           
            var isStudentInBatch = await batchStudentRepository.FindByCondition(bs => 
                bs.BatchId == trackBatchId && bs.StudentId == request.StudentId)
                .AnyAsync(cancellationToken);

            if (!isStudentInBatch)
            {
                return EndpointResponse<string>.ErrorResponse("You are not enrolled in the batch for this assignment.", 403);
            }

            var existingSubmission = await submissionRepository.FindByCondition(s => 
                s.AssignmentId == request.AssignmentId && s.StudentId == request.StudentId)
                .AnyAsync(cancellationToken);

            if (existingSubmission)
            {
                return EndpointResponse<string>.ErrorResponse("You have already submitted this assignment.", 409);
            }

            if (request.File == null || request.File.Length == 0)
            {
                 return EndpointResponse<string>.ErrorResponse("File is required.", 400);
            }

            var fileUrl = await _fileService.SaveFileAsync(request.File, "AssignmentSubmissions", request.StudentId);
            if (string.IsNullOrEmpty(fileUrl))
            {
                return EndpointResponse<string>.ErrorResponse("File upload failed.", 500);
            }

            var submission = new Submission
            {
                AssignmentId = request.AssignmentId,
                StudentId = request.StudentId,
                FileUrl = fileUrl,
                Score = null,
                Feedback = null,
                IsFinalized = false
            };

            await submissionRepository.CreateAsync(submission);
            await _unitOfWork.SaveChangesAsync();

            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            var superAdmins = await _userManager.GetUsersInRoleAsync("SuperAdmin");
            var adminIds = admins.Select(a => a.Id).Concat(superAdmins.Select(sa => sa.Id)).ToList();

            if (adminIds.Any())
            {
                var student = await _unitOfWork.GetRepository<ApplicationUser>()
                    .GetByIdAsync(request.StudentId);
                var studentName = student?.FullName ?? "A student";

                foreach (var adminId in adminIds)
                {
                    await _notificationService.CreateNotificationAsync(
                        adminId,
                        "New Submission",
                        $"{studentName} has submitted '{assignment.Title}'.",
                        ELearningProject.Models.NotificationType.Info,
                        $"/assignments/{assignment.Id}/submissions"
                    );
                }
            }

            return EndpointResponse<string>.SuccessResponse(submission.Id.ToString(), "Assignment submitted successfully.", 201);
        }
    }
}
