
using ELearningProject.Features.Shared;
using MediatR;

namespace ELearningProject.Features.Batche.GetStudentsInBatch
{
    public record GetBatchStudentsQuery(Guid BatchId) : IRequest<EndpointResponse<List<BatchStudentItemDto>>>;
}
