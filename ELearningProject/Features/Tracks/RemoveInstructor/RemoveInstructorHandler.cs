
using ELearningProject.Contarcts;
using ELearningProject.Features.Shared;
using ELearningProject.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ELearningProject.Features.Tracks.RemoveInstructor
{
    public class RemoveInstructorHandler : IRequestHandler<RemoveInstructorCommand, RequestResponse<string>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public RemoveInstructorHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<RequestResponse<string>> Handle(RemoveInstructorCommand request, CancellationToken cancellationToken)
        {
            var instructorTrackRepository = _unitOfWork.GetRepository<InstructorTrack>();

            var link = await instructorTrackRepository.FindByCondition(it => 
                it.TrackId == request.TrackId && it.InstructorId == request.InstructorId)
                .FirstOrDefaultAsync(cancellationToken);

            if (link == null)
            {
                return RequestResponse<string>.Fail("Instructor is not assigned to this track.");
            }

            instructorTrackRepository.Delete(link);
            await _unitOfWork.SaveChangesAsync();

            return RequestResponse<string>.Success(null, "Instructor removed successfully.");
        }
    }
}
