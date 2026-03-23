
using ELearningProject.Features.Shared;
using MediatR;

namespace ELearningProject.Features.Tracks.CreateTrack
{
    public record CreateTrackCommand(
        Guid BatchId,
        string Name
    ) : IRequest<EndpointResponse<Guid>>;
}
