
using ELearningProject.Features.Shared;
using MediatR;

namespace ELearningProject.Features.Batche.RemoveStudentFromBatch
{
    public record RemoveStudentFromBatchCommand(Guid BatchId, Guid StudentId) : IRequest<RequestResponse<string>>;
}
