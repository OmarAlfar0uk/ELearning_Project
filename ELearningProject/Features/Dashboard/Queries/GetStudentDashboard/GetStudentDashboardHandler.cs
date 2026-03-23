
using ELearningProject.Contarcts;
using ELearningProject.Features.Dashboard.DTOs;
using ELearningProject.Features.Shared;
using ELearningProject.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ELearningProject.Features.Dashboard.Queries.GetStudentDashboard
{
    public class GetStudentDashboardHandler : IRequestHandler<GetStudentDashboardQuery, EndpointResponse<StudentDashboardDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetStudentDashboardHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<EndpointResponse<StudentDashboardDto>> Handle(GetStudentDashboardQuery request, CancellationToken cancellationToken)
        {
           
            var batchStudentRepo = _unitOfWork.GetRepository<BatchStudent>();
            var studentBatchId = await batchStudentRepo.FindByCondition(bs => bs.StudentId == request.StudentId)
                .Select(bs => bs.BatchId)
                .FirstOrDefaultAsync(cancellationToken);

            int enrolledTracksCount = 0;
            if (studentBatchId != Guid.Empty)
            {
                var trackRepo = _unitOfWork.GetRepository<Track>();
                enrolledTracksCount = await trackRepo.FindByCondition(t => t.BatchId == studentBatchId)
                    .CountAsync(cancellationToken);
            }

          
            var submissionRepo = _unitOfWork.GetRepository<Submission>();
            var mySubmissions = await submissionRepo.FindByCondition(s => s.StudentId == request.StudentId)
                .ToListAsync(cancellationToken);

            var completedAssignments = mySubmissions.Count; // Or count where Score != null
            var averageGrade = mySubmissions.Where(s => s.Score.HasValue).Average(s => s.Score) ?? 0;

            
            var assignmentRepo = _unitOfWork.GetRepository<Assignment>();
            var upcomingDeadlines = new List<UpcomingDeadlineDto>();

            if (studentBatchId != Guid.Empty)
            {
                upcomingDeadlines = await assignmentRepo.FindByCondition(a => 
                    a.Lecture.Track.BatchId == studentBatchId && 
                    a.DueDate.HasValue && 
                    a.DueDate.Value > DateTime.UtcNow)
                    .OrderBy(a => a.DueDate)
                    .Take(5)
                    .Select(a => new UpcomingDeadlineDto(a.Title, a.DueDate!.Value, a.Lecture.Track.Name))
                    .ToListAsync(cancellationToken);
            }

            // 4. Recent Grades
                         var recentGradesQuery = await submissionRepo.FindByCondition(s => s.StudentId == request.StudentId && s.Score.HasValue)
                .Include(s => s.Assignment)
                .OrderByDescending(s => s.CreatedAt)
                .Take(5)
                .Select(s => new RecentGradeDto(s.Assignment.Title, s.Score!.Value, s.Feedback ?? ""))
                .ToListAsync(cancellationToken);


            var dto = new StudentDashboardDto(
                enrolledTracksCount,
                completedAssignments,
                averageGrade,
                upcomingDeadlines,
                recentGradesQuery
            );

            return EndpointResponse<StudentDashboardDto>.SuccessResponse(dto);
        }
    }
}
