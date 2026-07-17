using Auth.Models;
using ELearningProject.Contarcts;
using ELearningProject.Extensions;
using ELearningProject.Features.Coins.Services;
using ELearningProject.Features.Shared;
using ELearningProject.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ELearningProject.Features.StudentEvaluation
{
    /// <summary>
    /// Handler for <see cref="GetStudentEvaluationQuery"/>.
    /// Aggregates student metrics across Progress, ExamAttempt, Submission, ApplicationUser, and BatchStudent
    /// entities using pure database-level LINQ aggregate expressions (CountAsync, AverageAsync).
    /// Enforces fine-grained role authorization:
    /// <list type="bullet">
    ///   <item>Student role → may only view their own evaluation metrics (403 if StudentId ≠ caller).</item>
    ///   <item>Instructor role → may only view metrics for students in their assigned tracks' batches
    ///         (verified via <see cref="IStudentAccessService"/>).</item>
    ///   <item>Admin / SuperAdmin → unconditional access.</item>
    /// </list>
    /// </summary>
    public class GetStudentEvaluationHandler : IRequestHandler<GetStudentEvaluationQuery, EndpointResponse<StudentEvaluationDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IStudentAccessService _studentAccessService;

        public GetStudentEvaluationHandler(
            IUnitOfWork unitOfWork,
            IHttpContextAccessor httpContextAccessor,
            UserManager<ApplicationUser> userManager,
            IStudentAccessService studentAccessService)
        {
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _studentAccessService = studentAccessService;
        }

        public async Task<EndpointResponse<StudentEvaluationDto>> Handle(
            GetStudentEvaluationQuery request,
            CancellationToken cancellationToken)
        {
            // ── 1. Resolve caller identity ────────────────────────────────────────
            var callerPrincipal = _httpContextAccessor.HttpContext?.User;
            var callerId = callerPrincipal?.GetUserId() ?? Guid.Empty;
            var callerRole = callerPrincipal?.GetUserRole() ?? string.Empty;

            if (callerId == Guid.Empty)
            {
                return EndpointResponse<StudentEvaluationDto>.UnauthorizedResponse();
            }

            // ── 2. Per-handler authorization ──────────────────────────────────────
            if (callerRole == "Student" && callerId != request.StudentId)
            {
                return EndpointResponse<StudentEvaluationDto>.ErrorResponse(
                    "Students may only view their own evaluation.",
                    403);
            }

            if (callerRole != "Student")
            {
                var hasAccess = await _studentAccessService.CanAccessStudentAsync(
                    callerRole,
                    callerId,
                    request.StudentId,
                    cancellationToken);

                if (!hasAccess)
                {
                    return EndpointResponse<StudentEvaluationDto>.ErrorResponse(
                        "Instructors may only view evaluation for students within their assigned tracks.",
                        403);
                }
            }

            // ── 3. Verify target student exists and is in Student role ────────────
            var student = await _userManager.FindByIdAsync(request.StudentId.ToString());
            if (student == null || student.IsDeleted)
            {
                return EndpointResponse<StudentEvaluationDto>.NotFoundResponse("Student not found.");
            }

            var isStudentRole = await _userManager.IsInRoleAsync(student, "Student");
            if (!isStudentRole)
            {
                return EndpointResponse<StudentEvaluationDto>.NotFoundResponse("Student not found.");
            }

            // ── 4. Aggregate: Overall Completion Percentage (Database-level) ──────
            var progressRepo = _unitOfWork.GetRepository<ELearningProject.Models.Progress>();
            var progressQuery = progressRepo.FindByCondition(p => p.StudentId == request.StudentId);

            if (request.TrackId.HasValue)
            {
                progressQuery = progressQuery.Where(p => p.TrackId == request.TrackId.Value);
            }

            var overallCompletionPercentage = await progressQuery.AnyAsync(cancellationToken)
                ? await progressQuery.AverageAsync(p => (double)p.CompletionPercentage, cancellationToken)
                : 0.0;

            // ── 5. Aggregate: Exam metrics (Database-level) ───────────────────────
            var attemptRepo = _unitOfWork.GetRepository<ExamAttempt>();
            var attemptQuery = attemptRepo.FindByCondition(a =>
                a.StudentId == request.StudentId &&
                a.Status != AttemptStatus.InProgress);

            if (request.TrackId.HasValue)
            {
                attemptQuery = attemptQuery.Where(a => a.Exam.TrackId == request.TrackId.Value);
            }

            var examsTaken = await attemptQuery.CountAsync(cancellationToken);

            var gradedAttemptsQuery = attemptQuery.Where(a =>
                a.Score.HasValue &&
                a.Exam.Questions.Sum(q => q.Points) > 0);

            var examsPassed = await gradedAttemptsQuery
                .Where(a => ((double)a.Score!.Value / a.Exam.Questions.Sum(q => q.Points)) >= 0.60)
                .CountAsync(cancellationToken);

            var averageExamScorePercentage = await gradedAttemptsQuery.AnyAsync(cancellationToken)
                ? await gradedAttemptsQuery.AverageAsync(a =>
                    ((double)a.Score!.Value / a.Exam.Questions.Sum(q => q.Points)) * 100.0, cancellationToken)
                : 0.0;

            // ── 6. Aggregate: Assignment metrics (Database-level) ─────────────────
            var submissionRepo = _unitOfWork.GetRepository<Submission>();
            var submissionQuery = submissionRepo.FindByCondition(s => s.StudentId == request.StudentId);

            if (request.TrackId.HasValue)
            {
                submissionQuery = submissionQuery.Where(s => s.Assignment.Lecture.TrackId == request.TrackId.Value);
            }

            var assignmentsSubmitted = await submissionQuery.CountAsync(cancellationToken);

            var assignmentsOnTime = await submissionQuery
                .Where(s => !s.Assignment.DueDate.HasValue || s.CreatedAt <= s.Assignment.DueDate.Value)
                .CountAsync(cancellationToken);

            var scoredSubmissionsQuery = submissionQuery.Where(s => s.Score.HasValue);

            var averageSubmissionScore = await scoredSubmissionsQuery.AnyAsync(cancellationToken)
                ? await scoredSubmissionsQuery.AverageAsync(s => s.Score!.Value, cancellationToken)
                : 0.0;

            // ── 7. Global metrics & Batch metrics ─────────────────────────────────
            var coinBalance = student.CoinBalance;

            var batchStudentRepo = _unitOfWork.GetRepository<BatchStudent>();
            var latestBatchStudent = await batchStudentRepo
                .FindByCondition(bs => bs.StudentId == request.StudentId)
                .OrderByDescending(bs => bs.CreatedAt)
                .Select(bs => new { bs.Rank, bs.AverageScore })
                .FirstOrDefaultAsync(cancellationToken);

            int? rank = latestBatchStudent?.Rank;
            double? batchAverageScore = latestBatchStudent?.AverageScore;

            // ── 8. Build DTO & return ─────────────────────────────────────────────
            var dto = new StudentEvaluationDto(
                student.Id,
                student.FullName,
                Math.Round(overallCompletionPercentage, 2),
                examsTaken,
                examsPassed,
                Math.Round(averageExamScorePercentage, 2),
                assignmentsSubmitted,
                assignmentsOnTime,
                Math.Round(averageSubmissionScore, 2),
                coinBalance,
                rank,
                batchAverageScore.HasValue ? Math.Round(batchAverageScore.Value, 2) : null
            );

            return EndpointResponse<StudentEvaluationDto>.SuccessResponse(
                dto,
                "Student evaluation retrieved successfully.");
        }
    }
}
