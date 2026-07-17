using ELearningProject.Features.Shared;
using MediatR;

namespace ELearningProject.Features.Exams
{
    /// <summary>
    /// Query to retrieve all attempts for an exam.
    /// </summary>
    public record GetExamAttemptsQuery(Guid ExamId)
        : IRequest<EndpointResponse<List<ExamAttemptSummaryDto>>>;
}
