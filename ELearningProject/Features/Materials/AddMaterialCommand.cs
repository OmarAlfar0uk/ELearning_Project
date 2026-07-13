using ELearningProject.Features.Shared;
using ELearningProject.Models;
using MediatR;

namespace ELearningProject.Features.Materials
{
    /// <summary>
    /// Command to add a new material to a lecture.
    /// </summary>
    public record AddMaterialCommand(
        string Name,
        MaterialType Type,
        Guid LectureId,
        string? FileUrl,
        string? ExternalLink
    ) : IRequest<EndpointResponse<MaterialDto>>;
}
