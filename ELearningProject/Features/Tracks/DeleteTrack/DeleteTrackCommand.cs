
using ELearningProject.Features.Shared;
using MediatR;

namespace ELearningProject.Features.Tracks.DeleteTrack
{
    public record DeleteTrackCommand(Guid TrackId) : IRequest<RequestResponse<string>>;
}
