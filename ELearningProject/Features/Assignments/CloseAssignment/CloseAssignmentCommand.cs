
using ELearningProject.Features.Shared;
using MediatR;

namespace ELearningProject.Features.Assignments.CloseAssignment
{
    public record CloseAssignmentCommand(Guid AssignmentId) : IRequest<RequestResponse<string>>;
}
