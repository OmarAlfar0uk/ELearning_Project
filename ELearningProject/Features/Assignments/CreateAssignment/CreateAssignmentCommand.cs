
using ELearningProject.Features.Shared;
using MediatR;

namespace ELearningProject.Features.Assignments.CreateAssignment
{
    public record CreateAssignmentCommand(
        Guid LectureId,
        string Title,
        int MaxScore,
        DateTime? DueDate
    ) : IRequest<EndpointResponse<Guid>>;
}
