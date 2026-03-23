using Auth.Models;
using ELearningProject.Features.Shared;
using MediatR;

namespace ELearningProject.Features.Batche.CreateBatch
{
    public class CreateBatchCommand : IRequest<EndpointResponse<Guid>>
    {
        public string Name { get; set; } = default!;
        public DateTime StartDate { get; set; }
    }
}
