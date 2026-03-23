using ELearningProject.Contarcts;
using ELearningProject.Features.Shared;
using ELearningProject.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ELearningProject.Features.Batche.UpdateBatch
{
    public class UpdateBatchHandler : IRequestHandler<UpdateBatchCommand, RequestResponse<string>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateBatchHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<RequestResponse<string>> Handle(UpdateBatchCommand request, CancellationToken cancellationToken)
        {
            var batchRepository = _unitOfWork.GetRepository<Batch>();

            var batch = await batchRepository.GetByIdAsync(request.Id);

            if (batch == null)
            {
                return RequestResponse<string>.Fail("Batch not found.");
            }

            // Check if name is unique (excluding current batch)
             var duplicateBatch = await batchRepository.FindByCondition(b => b.Name == request.Name && b.Id != request.Id)
                                                     .FirstOrDefaultAsync(cancellationToken);

            if (duplicateBatch != null)
            {
                 return RequestResponse<string>.Fail("Another Batch with the same name already exists.");
            }

            batch.Name = request.Name;
            batch.StartDate = request.StartDate;

            batchRepository.Update(batch);
            await _unitOfWork.SaveChangesAsync();

            return RequestResponse<string>.Success(null, "Batch updated successfully.");
        }
    }
}
