using ELearningProject.Features.Shared;
using MediatR;

namespace ELearningProject.Features.Exams
{
    /// <summary>
    /// Command to manually grade an essay or fill-in-the-blank answer.
    /// </summary>
    public record GradeAnswerCommand(
        Guid AnswerId,
        int PointsAwarded,
        string? Feedback
    ) : IRequest<EndpointResponse<string>>;
}
