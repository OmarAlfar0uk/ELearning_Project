
using ELearningProject.Contarcts;
using ELearningProject.Features.Shared;
using ELearningProject.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ELearningProject.Features.Lectures.UpdateLecture
{
    public class UpdateLectureHandler : IRequestHandler<UpdateLectureCommand, RequestResponse<string>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateLectureHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<RequestResponse<string>> Handle(UpdateLectureCommand request, CancellationToken cancellationToken)
        {
            var lectureRepository = _unitOfWork.GetRepository<Lecture>();

            var lecture = await lectureRepository.GetByIdAsync(request.LectureId);
            if (lecture == null)
            {
                return RequestResponse<string>.Fail("Lecture not found.");
            }

            // Check Title Uniqueness in SAME Track (if title changed)
            if (lecture.Title != request.Title)
            {
                var existingLecture = await lectureRepository.FindByCondition(l => 
                    l.TrackId == lecture.TrackId && l.Title == request.Title)
                    .AnyAsync(cancellationToken);

                if (existingLecture)
                {
                    return RequestResponse<string>.Fail("Lecture with this title already exists in the track.");
                }
            }

            lecture.Title = request.Title;
            lecture.ContentText = request.ContentText;
            lecture.DriveLink = request.DriveLink;

            if (request.FileUrl != null)
            {
                lecture.FileUrl = request.FileUrl;
            }

            lectureRepository.Update(lecture);
            await _unitOfWork.SaveChangesAsync();

            return RequestResponse<string>.Success(null, "Lecture updated successfully.");
        }
    }
}
