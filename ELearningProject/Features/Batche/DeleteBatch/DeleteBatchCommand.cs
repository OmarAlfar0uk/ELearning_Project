using ELearningProject.Features.Shared;
using MediatR;

namespace ELearningProject.Features.Batche.DeleteBatch
{
    public record DeleteBatchCommand(Guid Id) : IRequest<RequestResponse<string>>;
}
