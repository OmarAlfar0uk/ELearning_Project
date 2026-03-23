
using ELearningProject.Contarcts;
using ELearningProject.Features.Shared;
using ELearningProject.Features.Tracks.DTOs;
using ELearningProject.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ELearningProject.Features.Tracks.GetTrackDetails
{
    public class GetTrackDetailsHandler : IRequestHandler<GetTrackDetailsQuery, EndpointResponse<TrackDetailsDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetTrackDetailsHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<EndpointResponse<TrackDetailsDto>> Handle(GetTrackDetailsQuery request, CancellationToken cancellationToken)
        {
            var trackRepository = _unitOfWork.GetRepository<Track>();

            var track = await trackRepository.FindByCondition(t => t.Id == request.TrackId)
                .Include(t => t.Lectures)
                .FirstOrDefaultAsync(cancellationToken);

            if (track == null)
            {
                return EndpointResponse<TrackDetailsDto>.NotFoundResponse("Track not found.");
            }

            var dto = new TrackDetailsDto(
                track.Id,
                track.Name,
                track.BatchId,
                track.Lectures.Count
            );

            return EndpointResponse<TrackDetailsDto>.SuccessResponse(dto);
        }
    }
}
