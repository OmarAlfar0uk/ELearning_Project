using ELearningProject.Contarcts;
using ELearningProject.Features.Shared;
using ELearningProject.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ELearningProject.Features.Batche.CreateBatch
{
    public class CreateBatchHandler : IRequestHandler<CreateBatchCommand, EndpointResponse<Guid>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateBatchHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<EndpointResponse<Guid>> Handle(CreateBatchCommand request, CancellationToken cancellationToken)
        {
            var batchRepository = _unitOfWork.GetRepository<Batch>();

            // Check if batch with the same name exists
            var existingBatch = await batchRepository.FindByCondition(b => b.Name == request.Name)
                                                     .FirstOrDefaultAsync(cancellationToken);

            if (existingBatch != null)
            {
                return EndpointResponse<Guid>.ErrorResponse("Batch with the same name already exists.", 409);
            }

            var batch = new Batch
            {
                Name = request.Name,
                StartDate = request.StartDate
            };

            await batchRepository.CreateAsync(batch);
            await _unitOfWork.SaveChangesAsync();

            return EndpointResponse<Guid>.SuccessResponse(batch.Id, "Batch created successfully.", 201);
        }
    }
}
