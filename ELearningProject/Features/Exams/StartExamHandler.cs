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
    /// Handler to start a student's exam attempt.
    /// </summary>
    public class StartExamHandler : IRequestHandler<StartExamCommand, EndpointResponse<ExamAttemptDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public StartExamHandler(
            IUnitOfWork unitOfWork,
            IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<EndpointResponse<ExamAttemptDto>> Handle(
            StartExamCommand request,
            CancellationToken cancellationToken)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var studentId = user?.GetUserId() ?? Guid.Empty;

            if (studentId == Guid.Empty)
            {
                return EndpointResponse<ExamAttemptDto>.UnauthorizedResponse();
            }

            var examRepository = _unitOfWork.GetRepository<Exam>();
            var exam = await examRepository.GetByIdAsync(request.ExamId);

            if (exam == null)
            {
                return EndpointResponse<ExamAttemptDto>.NotFoundResponse("Exam not found.");
            }

            if (exam.IsClosed)
            {
                return EndpointResponse<ExamAttemptDto>.ErrorResponse(
                    "This exam is closed.",
                    400);
            }

            if (DateTime.UtcNow > exam.DueDate)
            {
                return EndpointResponse<ExamAttemptDto>.ErrorResponse(
                    "The exam due date has passed.",
                    400);
            }

            var attemptRepository = _unitOfWork.GetRepository<ExamAttempt>();
            var alreadyStarted = await attemptRepository
                .FindByCondition(attempt =>
                    attempt.ExamId == request.ExamId &&
                    attempt.StudentId == studentId)
                .AnyAsync(cancellationToken);

            if (alreadyStarted)
            {
                return EndpointResponse<ExamAttemptDto>.ErrorResponse(
                    "You have already started an attempt for this exam.",
                    409);
            }

            var startedAt = DateTime.UtcNow;
            var attempt = new ExamAttempt
            {
                ExamId = request.ExamId,
                StudentId = studentId,
                StartedAt = startedAt,
                Status = AttemptStatus.InProgress
            };

            await attemptRepository.CreateAsync(attempt);

            try
            {
                await _unitOfWork.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return EndpointResponse<ExamAttemptDto>.ErrorResponse(
                    "You have already started an attempt for this exam.",
                    409);
            }

            var dto = new ExamAttemptDto(
                attempt.Id,
                attempt.ExamId,
                attempt.StudentId,
                attempt.StartedAt,
                attempt.SubmittedAt,
                attempt.Status.ToString(),
                attempt.Score);

            return EndpointResponse<ExamAttemptDto>.SuccessResponse(
                dto,
                "Exam attempt started successfully.",
                201);
        }
    }
}
