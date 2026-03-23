
using ELearningProject.Contarcts;
using ELearningProject.Features.Lectures.DTOs;
using ELearningProject.Features.Shared;
using ELearningProject.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ELearningProject.Features.Lectures.GetTrackLectures
{
    public class GetTrackLecturesHandler : IRequestHandler<GetTrackLecturesQuery, EndpointResponse<List<LectureDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetTrackLecturesHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<EndpointResponse<List<LectureDto>>> Handle(GetTrackLecturesQuery request, CancellationToken cancellationToken)
        {
            var lectureRepository = _unitOfWork.GetRepository<Lecture>();
            var trackRepository = _unitOfWork.GetRepository<Track>();

            // Optional: Check Track exists
            var trackExists = await trackRepository.GetByIdAsync(request.TrackId);
            if (trackExists == null)
            {
                return EndpointResponse<List<LectureDto>>.NotFoundResponse("Track not found.");
            }

            var lectures = await lectureRepository.FindByCondition(l => l.TrackId == request.TrackId)
                .Select(l => new LectureDto(l.Id, l.Title, l.ContentText, l.DriveLink, l.FileUrl, l.TrackId))
                .ToListAsync(cancellationToken);

            return EndpointResponse<List<LectureDto>>.SuccessResponse(lectures);
        }
    }
}
