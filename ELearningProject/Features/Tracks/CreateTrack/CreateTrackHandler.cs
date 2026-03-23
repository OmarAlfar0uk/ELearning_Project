
using ELearningProject.Contarcts;
using ELearningProject.Features.Shared;
using ELearningProject.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ELearningProject.Features.Tracks.CreateTrack
{
    public class CreateTrackHandler : IRequestHandler<CreateTrackCommand, EndpointResponse<Guid>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateTrackHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<EndpointResponse<Guid>> Handle(CreateTrackCommand request, CancellationToken cancellationToken)
        {
            var batchRepository = _unitOfWork.GetRepository<Batch>();
            var trackRepository = _unitOfWork.GetRepository<Track>();

            // 1. Check Batch exists
            var batch = await batchRepository.GetByIdAsync(request.BatchId);
            if (batch == null)
            {
                return EndpointResponse<Guid>.NotFoundResponse("Batch not found.");
            }

            // 2. Check Name Uniqueness in Batch
            var existingTrack = await trackRepository.FindByCondition(t => 
                t.BatchId == request.BatchId && t.Name == request.Name)
                .AnyAsync(cancellationToken);

            if (existingTrack)
            {
                return EndpointResponse<Guid>.ErrorResponse("Track with this name already exists in the batch.", 409);
            }

            // 3. Create Track
            var track = new Track
            {
                Name = request.Name,
                BatchId = request.BatchId
            };

            await trackRepository.CreateAsync(track);
            await _unitOfWork.SaveChangesAsync();

            return EndpointResponse<Guid>.SuccessResponse(track.Id, "Track created successfully.", 201);
        }
    }
}
