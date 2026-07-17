using ELearningProject.Features.Shared;
using MediatR;

namespace ELearningProject.Features.StudentEvaluation
{
    /// <summary>
    /// MediatR query to retrieve aggregate performance and evaluation metrics for a student.
    /// </summary>
    /// <param name="StudentId">Unique identifier of the target student.</param>
    /// <param name="TrackId">Optional track identifier to filter track-scoped metrics (completion, exams, assignments).</param>
    public record GetStudentEvaluationQuery(
        Guid StudentId,
        Guid? TrackId = null
    ) : IRequest<EndpointResponse<StudentEvaluationDto>>;
}
