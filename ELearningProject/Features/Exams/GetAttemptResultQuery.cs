using ELearningProject.Features.Shared;
using MediatR;

namespace ELearningProject.Features.Exams
{
    /// <summary>
    /// Query to retrieve the result of an exam attempt.
    /// </summary>
    public record GetAttemptResultQuery(Guid AttemptId)
        : IRequest<EndpointResponse<ExamAttemptResultDto>>;
}
