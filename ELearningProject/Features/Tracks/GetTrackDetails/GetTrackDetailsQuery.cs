
using ELearningProject.Features.Shared;
using ELearningProject.Features.Tracks.DTOs;
using MediatR;

namespace ELearningProject.Features.Tracks.GetTrackDetails
{
    public record GetTrackDetailsQuery(Guid TrackId) : IRequest<EndpointResponse<TrackDetailsDto>>;
}
