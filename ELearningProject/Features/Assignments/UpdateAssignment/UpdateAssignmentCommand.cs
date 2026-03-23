
using ELearningProject.Features.Shared;
using MediatR;

namespace ELearningProject.Features.Assignments.UpdateAssignment
{
    public record UpdateAssignmentCommand(
        Guid AssignmentId,
        string Title,
        int MaxScore,
        DateTime? DueDate
    ) : IRequest<RequestResponse<string>>;
}
