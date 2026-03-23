
using ELearningProject.Contarcts;
using ELearningProject.Features.Shared;
using ELearningProject.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ELearningProject.Features.Tracks.DeleteTrack
{
    public class DeleteTrackHandler : IRequestHandler<DeleteTrackCommand, RequestResponse<string>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteTrackHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<RequestResponse<string>> Handle(DeleteTrackCommand request, CancellationToken cancellationToken)
        {
            var trackRepository = _unitOfWork.GetRepository<Track>();

            var track = await trackRepository.FindByCondition(t => t.Id == request.TrackId)
                .Include(t => t.Lectures)
                .FirstOrDefaultAsync(cancellationToken);

            if (track == null)
            {
                return RequestResponse<string>.Fail("Track not found.");
            }

            if (track.Lectures.Any())
            {
                return RequestResponse<string>.Fail("Cannot delete track because it has lectures.");
            }

            trackRepository.Delete(track);
            await _unitOfWork.SaveChangesAsync();

            return RequestResponse<string>.Success(null, "Track deleted successfully.");
        }
    }
}
