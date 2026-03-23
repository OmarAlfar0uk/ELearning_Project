
using ELearningProject.Contarcts;
using ELearningProject.Features.Assignments.DTOs;
using ELearningProject.Features.Shared;
using ELearningProject.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ELearningProject.Features.Assignments.GetAssignmentStats
{
    public class GetAssignmentStatsHandler : IRequestHandler<GetAssignmentStatsQuery, EndpointResponse<AssignmentStatsDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetAssignmentStatsHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<EndpointResponse<AssignmentStatsDto>> Handle(GetAssignmentStatsQuery request, CancellationToken cancellationToken)
        {
            var assignmentRepository = _unitOfWork.GetRepository<Assignment>();
            var submissionRepository = _unitOfWork.GetRepository<Submission>();
            var batchStudentRepository = _unitOfWork.GetRepository<BatchStudent>();

            // 1. Get Assignment with Track -> Batch info
            var assignment = await assignmentRepository.FindByCondition(a => a.Id == request.AssignmentId)
                .Include(a => a.Lecture)
                    .ThenInclude(l => l.Track)
                .FirstOrDefaultAsync(cancellationToken);

            if (assignment == null)
            {
                return EndpointResponse<AssignmentStatsDto>.NotFoundResponse("Assignment not found.");
            }

            // 2. Count Total Students in Batch (Enrolled)
            // Assuming Track has BatchId
            var batchId = assignment.Lecture.Track.BatchId;
            var totalStudents = await batchStudentRepository.FindByCondition(bs => bs.BatchId == batchId)
                .CountAsync(cancellationToken);

            // 3. Count Submitted
            var submittedCount = await submissionRepository.FindByCondition(s => s.AssignmentId == request.AssignmentId)
                .CountAsync(cancellationToken);

            // 4. Calculate Average Score
            // Only finalize? or all graded? usually all graded (Score != null)
            var averageScore = await submissionRepository.FindByCondition(s => s.AssignmentId == request.AssignmentId && s.Score != null)
                .AverageAsync(s => s.Score, cancellationToken) ?? 0;

            var stats = new AssignmentStatsDto(
                totalStudents,
                submittedCount,
                totalStudents - submittedCount, // Not Submitted
                averageScore
            );

            return EndpointResponse<AssignmentStatsDto>.SuccessResponse(stats);
        }
    }
}
