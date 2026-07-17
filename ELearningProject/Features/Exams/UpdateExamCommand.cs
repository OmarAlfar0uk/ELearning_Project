using ELearningProject.Features.Shared;
using MediatR;

namespace ELearningProject.Features.Exams
{
    /// <summary>
    /// Command to update an exam before it is closed.
    /// </summary>
    public record UpdateExamCommand(
        Guid ExamId,
        string Title,
        int DurationMinutes,
        DateTime DueDate
    ) : IRequest<EndpointResponse<ExamDto>>;
}
