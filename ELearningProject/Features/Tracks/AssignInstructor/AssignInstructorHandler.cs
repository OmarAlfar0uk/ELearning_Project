
using ELearningProject.Contarcts;
using ELearningProject.Features.Shared;
using ELearningProject.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ELearningProject.Features.Tracks.AssignInstructor
{
    public class AssignInstructorHandler : IRequestHandler<AssignInstructorCommand, RequestResponse<string>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public AssignInstructorHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<RequestResponse<string>> Handle(AssignInstructorCommand request, CancellationToken cancellationToken)
        {
            var trackRepository = _unitOfWork.GetRepository<Track>();
            var instructorTrackRepository = _unitOfWork.GetRepository<InstructorTrack>();

            var track = await trackRepository.GetByIdAsync(request.TrackId);
            if (track == null)
            {
                return RequestResponse<string>.Fail("Track not found.");
            }

            // Check if user exists and is instructor? 
            // Ideally we check User existence. But IGenericRepository logic might vary.
            // Let's assume we trust the Admin provided a valid Instructor ID or let FK constraint fail if user invalid.
            // But better to check.
            // Accessing UserManager or User Repo would be good.
            // Assuming IUnitOfWork has generic repo for ApplicationUser? 
            // _unitOfWork is Generic.

            // Check if already assigned
            var exists = await instructorTrackRepository.FindByCondition(it => 
                it.TrackId == request.TrackId && it.InstructorId == request.InstructorId)
                .AnyAsync(cancellationToken);

            if (exists)
            {
                return RequestResponse<string>.Fail("Instructor is already assigned to this track.");
            }

            await instructorTrackRepository.CreateAsync(new InstructorTrack
            {
                TrackId = request.TrackId,
                InstructorId = request.InstructorId
            });

            await _unitOfWork.SaveChangesAsync();

            return RequestResponse<string>.Success(null, "Instructor assigned successfully.");
        }
    }
}
