
using ELearningProject.Features.Shared;
using MediatR;

namespace ELearningProject.Features.Batche.GetAllBatches
{
    public record GetAllBatchesQuery(
        int PageNumber = 1,
        int PageSize = 10,
        string? Keyword = null
    ) : IRequest<EndpointResponse<PaginatedResult<BatchDto>>>;
}
