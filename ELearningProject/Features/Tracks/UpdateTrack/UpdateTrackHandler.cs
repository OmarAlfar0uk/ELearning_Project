
using ELearningProject.Contarcts;
using ELearningProject.Features.Shared;
using ELearningProject.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ELearningProject.Features.Tracks.UpdateTrack
{
    public class UpdateTrackHandler : IRequestHandler<UpdateTrackCommand, RequestResponse<string>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateTrackHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<RequestResponse<string>> Handle(UpdateTrackCommand request, CancellationToken cancellationToken)
        {
            var trackRepository = _unitOfWork.GetRepository<Track>();

            var track = await trackRepository.GetByIdAsync(request.TrackId);
            if (track == null)
            {
                return RequestResponse<string>.Fail("Track not found.");
            }

            // Check Name Uniqueness in SAME Batch (if name changed)
            if (track.Name != request.Name)
            {
                var existingTrack = await trackRepository.FindByCondition(t => 
                    t.BatchId == track.BatchId && t.Name == request.Name)
                    .AnyAsync(cancellationToken);

                if (existingTrack)
                {
                    return RequestResponse<string>.Fail("Track with this name already exists in the batch.");
                }
            }

            track.Name = request.Name;

            trackRepository.Update(track);
            await _unitOfWork.SaveChangesAsync();

            return RequestResponse<string>.Success(null, "Track updated successfully.");
        }
    }
}
