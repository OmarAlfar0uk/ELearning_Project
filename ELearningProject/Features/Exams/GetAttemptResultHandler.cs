using ELearningProject.Contarcts;
using ELearningProject.Extensions;
using ELearningProject.Features.Shared;
using ELearningProject.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ELearningProject.Features.Exams
{
    /// <summary>
    /// Handler to retrieve an attempt result with student ownership enforcement.
    /// </summary>
    public class GetAttemptResultHandler : IRequestHandler<GetAttemptResultQuery, EndpointResponse<ExamAttemptResultDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GetAttemptResultHandler(
            IUnitOfWork unitOfWork,
            IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<EndpointResponse<ExamAttemptResultDto>> Handle(
            GetAttemptResultQuery request,
            CancellationToken cancellationToken)
        {
            var attemptRepository = _unitOfWork.GetRepository<ExamAttempt>();
            var attempt = await attemptRepository
                .FindByCondition(currentAttempt => currentAttempt.Id == request.AttemptId)
                .Include(currentAttempt => currentAttempt.Exam)
                .Include(currentAttempt => currentAttempt.Answers)
                    .ThenInclude(answer => answer.ExamQuestion)
                .FirstOrDefaultAsync(cancellationToken);

            if (attempt == null)
            {
                return EndpointResponse<ExamAttemptResultDto>.NotFoundResponse(
                    "Exam attempt not found.");
            }

            var user = _httpContextAccessor.HttpContext?.User;
            var userId = user?.GetUserId() ?? Guid.Empty;
            var role = user?.GetUserRole() ?? string.Empty;

            if (role == "Student" && attempt.StudentId != userId)
            {
                return EndpointResponse<ExamAttemptResultDto>.ErrorResponse(
                    "You do not have access to this exam attempt.",
                    403);
            }

            var gradingStatus = attempt.Status switch
            {
                AttemptStatus.Graded => "graded",
                AttemptStatus.Submitted => "pending manual grading",
                _ => "in progress"
            };

            var answers = attempt.Answers
                .OrderBy(answer => answer.ExamQuestion.Order)
                .Select(answer => new ExamAttemptAnswerDto(
                    answer.Id,
                    answer.ExamQuestionId,
                    answer.ExamQuestion.Text,
                    answer.SelectedOptionId,
                    answer.AnswerText,
                    answer.IsCorrect,
                    answer.PointsAwarded))
                .ToList();

            var dto = new ExamAttemptResultDto(
                attempt.Id,
                attempt.ExamId,
                attempt.Status.ToString(),
                gradingStatus,
                attempt.Score,
                answers);

            return EndpointResponse<ExamAttemptResultDto>.SuccessResponse(
                dto,
                "Exam attempt result retrieved successfully.");
        }
    }
}
