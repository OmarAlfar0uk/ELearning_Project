
using ELearningProject.Contarcts;
using ELearningProject.Features.Shared;
using ELearningProject.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ELearningProject.Features.Batche.RemoveStudentFromBatch
{
    public class RemoveStudentFromBatchHandler : IRequestHandler<RemoveStudentFromBatchCommand, RequestResponse<string>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public RemoveStudentFromBatchHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<RequestResponse<string>> Handle(RemoveStudentFromBatchCommand request, CancellationToken cancellationToken)
        {
            var batchStudentRepository = _unitOfWork.GetRepository<BatchStudent>();

            // Find the relationship
            // Assuming IGenericRepository.FindByCondition returns IQueryable
            var batchStudent = await batchStudentRepository.FindByCondition(
                bs => bs.BatchId == request.BatchId && bs.StudentId == request.StudentId)
                .FirstOrDefaultAsync(cancellationToken);

            if (batchStudent == null)
            {
                return RequestResponse<string>.Fail("Student is not in this batch or batch does not exist.");
            }

            batchStudentRepository.Delete(batchStudent);
            await _unitOfWork.SaveChangesAsync();

            return RequestResponse<string>.Success(null, "Student removed from batch successfully.");
        }
    }
}
