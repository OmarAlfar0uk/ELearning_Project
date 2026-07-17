using ELearningProject.Contarcts;
using ELearningProject.Features.Shared;
using ELearningProject.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ELearningProject.Features.Exams
{
    /// <summary>
    /// Handler to retrieve exam attempt summaries for teaching staff.
    /// </summary>
    public class GetExamAttemptsHandler : IRequestHandler<GetExamAttemptsQuery, EndpointResponse<List<ExamAttemptSummaryDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetExamAttemptsHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<EndpointResponse<List<ExamAttemptSummaryDto>>> Handle(
            GetExamAttemptsQuery request,
            CancellationToken cancellationToken)
        {
            var examRepository = _unitOfWork.GetRepository<Exam>();
            var exam = await examRepository.GetByIdAsync(request.ExamId);

            if (exam == null)
            {
                return EndpointResponse<List<ExamAttemptSummaryDto>>.NotFoundResponse(
                    "Exam not found.");
            }

            var attemptRepository = _unitOfWork.GetRepository<ExamAttempt>();
            var attempts = await attemptRepository
                .FindByCondition(attempt => attempt.ExamId == request.ExamId)
                .Include(attempt => attempt.Student)
                .OrderBy(attempt => attempt.StartedAt)
                .Select(attempt => new ExamAttemptSummaryDto(
                    attempt.Id,
                    attempt.ExamId,
                    attempt.StudentId,
                    $"{attempt.Student.FirstName} {attempt.Student.LastName}",
                    attempt.Status.ToString(),
                    attempt.Score,
                    attempt.StartedAt,
                    attempt.SubmittedAt))
                .ToListAsync(cancellationToken);

            return EndpointResponse<List<ExamAttemptSummaryDto>>.SuccessResponse(
                attempts,
                "Exam attempts retrieved successfully.");
        }
    }
}
