using ELearningProject.Features.Shared;
using MediatR;

namespace ELearningProject.Features.Batche.GetBatchStudentStats
{
    public record GetBatchStudentStatsQuery(Guid BatchId, Guid StudentId)
        : IRequest<EndpointResponse<BatchStudentStatsDto>>;
}
