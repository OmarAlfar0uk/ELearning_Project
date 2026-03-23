using ELearningProject.Features.Shared;
using MediatR;
using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Http;

namespace ELearningProject.Features.Submissions.SubmitAssignment;

public record SubmitAssignmentCommand(
    Guid AssignmentId,
    IFormFile File
) : IRequest<EndpointResponse<string>>
{
    [JsonIgnore]
    public Guid StudentId { get; set; }
}
