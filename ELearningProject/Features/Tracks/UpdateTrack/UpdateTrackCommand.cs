
using ELearningProject.Features.Shared;
using MediatR;

namespace ELearningProject.Features.Tracks.UpdateTrack
{
    public record UpdateTrackCommand(Guid TrackId, string Name) : IRequest<RequestResponse<string>>;
}
