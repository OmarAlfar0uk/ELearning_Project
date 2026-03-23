using ELearningProject.Contarcts;
using ELearningProject.Features.Shared;
using ELearningProject.Models;
using ELearningProject.Features.Progress.Services;
using ELearningProject.Features.Notifications.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ELearningProject.Features.Submissions.GradeSubmission
{
    public class GradeSubmissionHandler : IRequestHandler<GradeSubmissionCommand, RequestResponse<string>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IProgressService _progressService;
        private readonly INotificationService _notificationService;
        private readonly Microsoft.AspNetCore.Http.IHttpContextAccessor _httpContextAccessor;

        public GradeSubmissionHandler(
            IUnitOfWork unitOfWork, 
            IProgressService progressService, 
            INotificationService notificationService,
            Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _progressService = progressService;
            _notificationService = notificationService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<RequestResponse<string>> Handle(GradeSubmissionCommand request, CancellationToken cancellationToken)
        {
            var submissionRepository = _unitOfWork.GetRepository<Submission>();
            var progressRepository = _unitOfWork.GetRepository<Models.Progress>();

            var submission = await submissionRepository.FindByCondition(s => s.Id == request.SubmissionId)
                .Include(s => s.Assignment)
                    .ThenInclude(a => a.Lecture)
                        .ThenInclude(l => l.Track)
                .FirstOrDefaultAsync(cancellationToken);

            if (submission == null)
            {
                return RequestResponse<string>.Fail("Submission not found.");
            }

            var user = _httpContextAccessor.HttpContext?.User; // Requires injection
            var userId = ELearningProject.Extensions.ClaimsPrincipalExtensions.GetUserId(user!);
            var userRole = ELearningProject.Extensions.ClaimsPrincipalExtensions.GetUserRole(user!);

            if (userRole == "Instructor") 
            {
                var assignmentTrackId = submission.Assignment.Lecture.TrackId;
                var isAssigned = await _unitOfWork.GetRepository<InstructorTrack>()
                    .FindByCondition(it => it.TrackId == assignmentTrackId && it.InstructorId == userId)
                    .AnyAsync(cancellationToken);

                if (!isAssigned)
                {
                    return RequestResponse<string>.Fail("You are not assigned to this track.");
                }
            }
            else if (userRole != "Admin" && userRole != "SuperAdmin")
            {
                return RequestResponse<string>.Fail("You don't have permission to grade this submission.");
            }

            if (submission.IsFinalized)
            {
                return RequestResponse<string>.Fail("Submission is already finalized and cannot be graded again.");
            }

            if (request.Score < 0 || request.Score > submission.Assignment.MaxScore)
            {
                return RequestResponse<string>.Fail($"Score must be between 0 and {submission.Assignment.MaxScore}.");
            }

            submission.Score = request.Score;
            submission.Feedback = request.Feedback;
            submission.IsFinalized = true;

            submissionRepository.Update(submission);

            var trackId = submission.Assignment.Lecture.TrackId;
            await _progressService.UpdateProgressAsync(submission.StudentId, trackId, cancellationToken);

            await _unitOfWork.SaveChangesAsync();

            await _notificationService.CreateNotificationAsync(
                submission.StudentId,
                "Assignment Graded",
                $"Your submission for {submission.Assignment.Title} has been graded. Score: {request.Score}",
                ELearningProject.Models.NotificationType.Grade,
                $"/assignments/{submission.Assignment.Id}/details"
            );

            return RequestResponse<string>.Success(null, "Submission graded successfully.");
        }
    }
}
