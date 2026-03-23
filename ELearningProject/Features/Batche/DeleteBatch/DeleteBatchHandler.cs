using ELearningProject.Contarcts;
using ELearningProject.Features.Shared;
using ELearningProject.Models;
using MediatR;

namespace ELearningProject.Features.Batche.DeleteBatch
{
    public class DeleteBatchHandler : IRequestHandler<DeleteBatchCommand, RequestResponse<string>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteBatchHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<RequestResponse<string>> Handle(DeleteBatchCommand request, CancellationToken cancellationToken)
        {
            var batchRepository = _unitOfWork.GetRepository<Batch>();
            var batch = await batchRepository.GetByIdAsync(request.Id);

            if (batch == null)
            {
                return RequestResponse<string>.Fail("Batch not found.");
            }

            batchRepository.Delete(batch);
            await _unitOfWork.SaveChangesAsync();

            return RequestResponse<string>.Success(null, "Batch deleted successfully.");
        }
    }
}
