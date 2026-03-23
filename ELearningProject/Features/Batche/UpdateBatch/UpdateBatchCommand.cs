using ELearningProject.Features.Shared;
using MediatR;
using System;

namespace ELearningProject.Features.Batche.UpdateBatch
{
    public class UpdateBatchCommand : IRequest<RequestResponse<string>>
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public DateTime StartDate { get; set; }
    }
}
