
using ELearningProject.Contarcts;
using ELearningProject.Features.Dashboard.DTOs;
using ELearningProject.Features.Shared;
using ELearningProject.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ELearningProject.Features.Dashboard.Queries.GetInstructorDashboard
{
    public class GetInstructorDashboardHandler : IRequestHandler<GetInstructorDashboardQuery, EndpointResponse<InstructorDashboardDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetInstructorDashboardHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<EndpointResponse<InstructorDashboardDto>> Handle(GetInstructorDashboardQuery request, CancellationToken cancellationToken)
        {
            // 1. Get Instructor Tracks
            var instructorTrackRepo = _unitOfWork.GetRepository<InstructorTrack>();
            var tracks = await instructorTrackRepo.FindByCondition(it => it.InstructorId == request.InstructorId)
                .Select(it => it.TrackId)
                .ToListAsync(cancellationToken);

            var myTracksCount = tracks.Count;

            // 2. Total Students in these tracks
            // Logic: Track -> Batch -> Students
            // More accurately: Get Tracks -> Batches -> Sum(Students)
            var batchStudentRepo = _unitOfWork.GetRepository<BatchStudent>();
            
            // We need to find students in batches that contain these tracks.
            // But BatchStudent is linked to Batch. Track is linked to Batch.
            // So: Find Batches for these Tracks.
            // Assumption: Track has BatchId.
            var trackRepo = _unitOfWork.GetRepository<Track>();
            var batchIds = await trackRepo.FindByCondition(t => tracks.Contains(t.Id))
                .Select(t => t.BatchId)
                .Distinct()
                .ToListAsync(cancellationToken);

            var totalStudents = await batchStudentRepo.FindByCondition(bs => batchIds.Contains(bs.BatchId))
                .CountAsync(cancellationToken);

            // 3. Pending Submissions
            // Submissions for assignments in these tracks where Score is null
            var submissionRepo = _unitOfWork.GetRepository<Submission>();
            var pendingSubmissionsCount = await submissionRepo.FindByCondition(s => 
                tracks.Contains(s.Assignment.Lecture.TrackId) && s.Score == null)
                .CountAsync(cancellationToken);

            // 4. Recent Submissions
            var recentSubmissions = await submissionRepo.FindByCondition(s => 
                tracks.Contains(s.Assignment.Lecture.TrackId))
                .OrderByDescending(s => s.CreatedAt)
                .Take(5)
                .Include(s => s.Student)
                .Include(s => s.Assignment)
                .Select(s => new DashboardSubmissionDto(
                    s.Student.FullName,
                    s.Assignment.Title,
                    s.CreatedAt,
                    s.Id))
                .ToListAsync(cancellationToken);

            var dto = new InstructorDashboardDto(
                myTracksCount,
                totalStudents,
                pendingSubmissionsCount,
                recentSubmissions
            );

            return EndpointResponse<InstructorDashboardDto>.SuccessResponse(dto);
        }
    }
}
