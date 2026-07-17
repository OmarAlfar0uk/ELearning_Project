using ELearningProject.Features.Shared;
using MediatR;

namespace ELearningProject.Features.Exams
{
    /// <summary>
    /// Command to create an exam for a track.
    /// </summary>
    public record CreateExamCommand(
        string Title,
        Guid TrackId,
        int DurationMinutes,
        DateTime DueDate
    ) : IRequest<EndpointResponse<ExamDto>>;
}
