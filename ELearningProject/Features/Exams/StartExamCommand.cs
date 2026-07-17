using ELearningProject.Features.Shared;
using MediatR;

namespace ELearningProject.Features.Exams
{
    /// <summary>
    /// Command to start one exam attempt for the current student.
    /// </summary>
    public record StartExamCommand(Guid ExamId)
        : IRequest<EndpointResponse<ExamAttemptDto>>;
}
