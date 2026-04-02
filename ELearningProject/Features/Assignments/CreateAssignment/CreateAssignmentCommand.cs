
using ELearningProject.Features.Shared;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace ELearningProject.Features.Assignments.CreateAssignment
{
    public record CreateAssignmentCommand(
        Guid LectureId,
        string Title,
        int MaxScore,
        DateTime? DueDate,
        IFormFile? File = null
    ) : IRequest<EndpointResponse<Guid>>;
}
