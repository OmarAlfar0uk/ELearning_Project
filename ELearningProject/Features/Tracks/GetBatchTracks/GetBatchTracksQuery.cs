
using ELearningProject.Features.Shared;
using ELearningProject.Features.Tracks.DTOs;
using MediatR;

namespace ELearningProject.Features.Tracks.GetBatchTracks
{
    public record GetBatchTracksQuery(Guid BatchId) : IRequest<EndpointResponse<List<TrackDto>>>;
}
