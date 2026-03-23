
using ELearningProject.Contarcts;
using ELearningProject.Features.Lectures.DTOs;
using ELearningProject.Features.Shared;
using ELearningProject.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ELearningProject.Features.Lectures.GetLectureDetails
{
    public class GetLectureDetailsHandler : IRequestHandler<GetLectureDetailsQuery, EndpointResponse<LectureDetailsDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetLectureDetailsHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<EndpointResponse<LectureDetailsDto>> Handle(GetLectureDetailsQuery request, CancellationToken cancellationToken)
        {
            var lectureRepository = _unitOfWork.GetRepository<Lecture>();

            var lecture = await lectureRepository.FindByCondition(l => l.Id == request.LectureId)
                .Include(l => l.Assignments)
                .FirstOrDefaultAsync(cancellationToken);

            if (lecture == null)
            {
                return EndpointResponse<LectureDetailsDto>.NotFoundResponse("Lecture not found.");
            }

            var dto = new LectureDetailsDto(
                lecture.Id,
                lecture.Title,
                lecture.ContentText,
                lecture.DriveLink,
                lecture.FileUrl,
                lecture.TrackId,
                lecture.Assignments.Count
            );

            return EndpointResponse<LectureDetailsDto>.SuccessResponse(dto);
        }
    }
}
