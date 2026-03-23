
using ELearningProject.Contarcts;
using ELearningProject.Features.Shared;
using ELearningProject.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ELearningProject.Features.Lectures.DeleteLecture
{
    public class DeleteLectureHandler : IRequestHandler<DeleteLectureCommand, RequestResponse<string>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteLectureHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<RequestResponse<string>> Handle(DeleteLectureCommand request, CancellationToken cancellationToken)
        {
            var lectureRepository = _unitOfWork.GetRepository<Lecture>();

            var lecture = await lectureRepository.FindByCondition(l => l.Id == request.LectureId)
                .Include(l => l.Assignments)
                .FirstOrDefaultAsync(cancellationToken);

            if (lecture == null)
            {
                return RequestResponse<string>.Fail("Lecture not found.");
            }

            if (lecture.Assignments.Any())
            {
                return RequestResponse<string>.Fail("Cannot delete lecture because it has assignments.");
            }

            lectureRepository.Delete(lecture);
            await _unitOfWork.SaveChangesAsync();

            return RequestResponse<string>.Success(null, "Lecture deleted successfully.");
        }
    }
}
