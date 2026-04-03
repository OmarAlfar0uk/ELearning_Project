
using ELearningProject.Features.Shared;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace ELearningProject.Features.Assignments.UpdateAssignment
{
    public record UpdateAssignmentCommand(
        Guid AssignmentId,
        string Title,
        int MaxScore,
        DateTime? DueDate,
        IFormFile? File = null
    ) : IRequest<RequestResponse<string>>;
}
