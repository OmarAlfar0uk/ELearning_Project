
using ELearningProject.Contarcts;
using ELearningProject.Features.Shared;
using ELearningProject.Features.Tracks.DTOs;
using ELearningProject.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ELearningProject.Features.Tracks.GetBatchTracks
{
    public class GetBatchTracksHandler : IRequestHandler<GetBatchTracksQuery, EndpointResponse<List<TrackDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly Microsoft.AspNetCore.Http.IHttpContextAccessor _httpContextAccessor;

        public GetBatchTracksHandler(IUnitOfWork unitOfWork, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<EndpointResponse<List<TrackDto>>> Handle(GetBatchTracksQuery request, CancellationToken cancellationToken)
        {
            var trackRepository = _unitOfWork.GetRepository<Track>();
            var batchRepository = _unitOfWork.GetRepository<Batch>();

            // Optional: Check if Batch exists
            var batchExists = await batchRepository.GetByIdAsync(request.BatchId);
            if (batchExists == null)
            {
                return EndpointResponse<List<TrackDto>>.NotFoundResponse("Batch not found.");
            }

            var user = _httpContextAccessor.HttpContext?.User;
            var userId = ELearningProject.Extensions.ClaimsPrincipalExtensions.GetUserId(user!);
            var userRole = ELearningProject.Extensions.ClaimsPrincipalExtensions.GetUserRole(user!);

            if (userRole == "Student")
            {
                // Verify Student is in Batch
                var isStudentInBatch = await _unitOfWork.GetRepository<BatchStudent>()
                    .FindByCondition(bs => bs.BatchId == request.BatchId && bs.StudentId == userId)
                    .AnyAsync(cancellationToken);

                if (!isStudentInBatch)
                {
                    return EndpointResponse<List<TrackDto>>.ErrorResponse("You are not enrolled in this batch.", 403);
                }
            }
            
            var query = trackRepository.FindByCondition(t => t.BatchId == request.BatchId);



            var tracks = await query
                .Select(t => new TrackDto(t.Id, t.Name, t.BatchId))
                .ToListAsync(cancellationToken);

            return EndpointResponse<List<TrackDto>>.SuccessResponse(tracks);
        }
    }
}
