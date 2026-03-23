using ELearningProject.Features.Shared;
using MediatR;

namespace ELearningProject.Features.Batche.GetTopBatchesByScore
{
    public record GetTopBatchesByScoreQuery(int Limit = 5)
        : IRequest<EndpointResponse<List<TopBatchByScoreDto>>>;
}
