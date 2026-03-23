
using ELearningProject.Features.Shared;
using MediatR;

namespace ELearningProject.Features.Batche.AssignTrackToBatch
{
    public record AssignTrackToBatchCommand(Guid BatchId, Guid TrackId) : IRequest<RequestResponse<string>>;
}
