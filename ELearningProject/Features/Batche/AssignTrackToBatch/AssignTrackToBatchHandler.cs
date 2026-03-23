
using ELearningProject.Contarcts;
using ELearningProject.Features.Shared;
using ELearningProject.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ELearningProject.Features.Batche.AssignTrackToBatch
{
    public class AssignTrackToBatchHandler : IRequestHandler<AssignTrackToBatchCommand, RequestResponse<string>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public AssignTrackToBatchHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<RequestResponse<string>> Handle(AssignTrackToBatchCommand request, CancellationToken cancellationToken)
        {
            var batchRepository = _unitOfWork.GetRepository<Batch>();
            var trackRepository = _unitOfWork.GetRepository<Track>();

            // 1. Check Batch existence
            var batch = await batchRepository.GetByIdAsync(request.BatchId);
            if (batch == null)
            {
                return RequestResponse<string>.Fail("Batch not found.");
            }

            // 2. Check Track existence
            var track = await trackRepository.GetByIdAsync(request.TrackId);
            if (track == null)
            {
                return RequestResponse<string>.Fail("Track not found.");
            }

            // 3. Check if Track is already assigned to this batch
            if (track.BatchId == request.BatchId)
            {
                // Already assigned, return success or info?
                // The requirement says "Prevent duplication". Since BatchId is single FK, 
                // it matches if BatchId is the same.
                 return RequestResponse<string>.Fail("Track is already assigned to this batch.");
            }
            
            // 4. Update relationship
            // Assuming Track belongs to ONE batch (based on model: public Guid BatchId { get; set; })
            // If it belongs to one batch, re-assigning it might be what's intended or maybe stricter logic is needed.
            // Constraint: "Add relationship". If it's a re-assignment (move), simply update properties.
            track.BatchId = request.BatchId;

            trackRepository.Update(track);
            await _unitOfWork.SaveChangesAsync();

            return RequestResponse<string>.Success(null, "Track assigned to batch successfully.");
        }
    }
}
