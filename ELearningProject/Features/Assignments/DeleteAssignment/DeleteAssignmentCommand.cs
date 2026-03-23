
using ELearningProject.Features.Shared;
using MediatR;

namespace ELearningProject.Features.Assignments.DeleteAssignment
{
    public record DeleteAssignmentCommand(Guid AssignmentId) : IRequest<RequestResponse<string>>;
}
