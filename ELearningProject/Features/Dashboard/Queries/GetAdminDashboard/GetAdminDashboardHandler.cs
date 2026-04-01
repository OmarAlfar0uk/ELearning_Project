using Auth.Models;
using ELearningProject.Contarcts;
using ELearningProject.Features.Dashboard.DTOs;
using ELearningProject.Features.Shared;
using ELearningProject.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ELearningProject.Features.Dashboard.Queries.GetAdminDashboard
{
    public class GetAdminDashboardHandler : IRequestHandler<GetAdminDashboardQuery, EndpointResponse<AdminDashboardDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> _userManager;

        public GetAdminDashboardHandler(IUnitOfWork unitOfWork, Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<EndpointResponse<AdminDashboardDto>> Handle(GetAdminDashboardQuery request, CancellationToken cancellationToken)
        {
            var userRepo = _unitOfWork.GetRepository<ApplicationUser>();
            var trackRepo = _unitOfWork.GetRepository<Track>();
            var batchRepo = _unitOfWork.GetRepository<Batch>();

            // 1. Basic Counts
            // Note: Using IQueryable to ensure global filters (IsDeleted) are applied and to avoid fetching all users
            var totalStudents = await _userManager.GetUsersInRoleAsync("Student");
            var filteredStudentsCount = totalStudents.Count(u => !u.IsDeleted);

            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            var superAdmins = await _userManager.GetUsersInRoleAsync("SuperAdmin");
            var totalAdmins = admins.Count(u => !u.IsDeleted) + superAdmins.Count(u => !u.IsDeleted);
            var totalTracks = await trackRepo.GetAll().CountAsync(cancellationToken);
            var totalBatches = await batchRepo.GetAll().CountAsync(cancellationToken);

            // 2. Recent Users
            var recentUsers = await _unitOfWork.GetRepository<ApplicationUser>().GetAll()
                .Where(u => !u.IsDeleted) // Force filter again
                .OrderByDescending(u => u.CreatedAt)
                .Take(5)
                .Select(u => new RecentUserDto(u.FullName, u.Email!, "User", u.CreatedAt))
                .ToListAsync(cancellationToken);

            // 3. Top Tracks (real avg score per track)
            var submissionRepoForTracks = _unitOfWork.GetRepository<Submission>();
            var batchStudentRepo = _unitOfWork.GetRepository<BatchStudent>();

            var topTracks = await trackRepo.FindByCondition(t => !t.IsDeleted)
                .Select(t => new
                {
                    t.Name,
                    t.Id,
                    t.BatchId
                })
                .Take(10)
                .ToListAsync(cancellationToken);

            var topTrackDtos = new List<TrackPerformanceDto>();
            foreach (var t in topTracks)
            {
                var avgScore = await submissionRepoForTracks
                    .FindByCondition(s => s.Assignment.Lecture.TrackId == t.Id && s.Score.HasValue && !s.Student.IsDeleted)
                    .AverageAsync(s => (double?)s.Score, cancellationToken) ?? 0;

                var studentCount = await batchStudentRepo
                    .FindByCondition(bs => bs.BatchId == t.BatchId)
                    .CountAsync(cancellationToken);

                topTrackDtos.Add(new TrackPerformanceDto(t.Name, Math.Round(avgScore, 2), studentCount));
            }

            var topTracksSorted = topTrackDtos.OrderByDescending(t => t.AverageScore).Take(5).ToList();

            // 4. Top Students
            var submissionRepo = _unitOfWork.GetRepository<Submission>();

            // Step 1: Get top 5 students by average score from DB (ensure student is not deleted)
            var topStudentScores = await submissionRepo.FindByCondition(s => s.Score.HasValue && !s.Student.IsDeleted)
                .GroupBy(s => s.StudentId)
                .Select(g => new
                {
                    StudentId = g.Key,
                    AverageScore = g.Average(s => (double)s.Score!.Value)
                })
                .OrderByDescending(x => x.AverageScore)
                .Take(5)
                .ToListAsync(cancellationToken);

            // Step 2: Load student info in-memory (ensure not deleted again)
            var studentIds = topStudentScores.Select(x => x.StudentId).ToList();
            var studentInfos = await _unitOfWork.GetRepository<ApplicationUser>().GetAll()
                .Where(u => studentIds.Contains(u.Id) && !u.IsDeleted)
                .Select(u => new { u.Id, u.FullName, u.Email })
                .ToListAsync(cancellationToken);

            // Step 3: Resolve each student's latest batch name
            var studentBatchRows = await batchStudentRepo
                .FindByCondition(bs => studentIds.Contains(bs.StudentId))
                .OrderByDescending(bs => bs.CreatedAt)
                .Select(bs => new { bs.StudentId, BatchName = bs.Batch.Name })
                .ToListAsync(cancellationToken);

            var batchNameByStudent = studentBatchRows
                .GroupBy(x => x.StudentId)
                .ToDictionary(g => g.Key, g => g.First().BatchName);

            var topStudents = topStudentScores
                .Join(studentInfos,
                    score => score.StudentId,
                    info => info.Id,
                    (score, info) =>
                    {
                        var batchName = batchNameByStudent.TryGetValue(score.StudentId, out var name)
                            ? name
                            : string.Empty;

                        return new TopStudentDto(info.FullName, info.Email!, score.AverageScore, batchName);
                    })
                .ToList();

            var dto = new AdminDashboardDto(
                filteredStudentsCount,
                totalAdmins,
                totalTracks,
                totalBatches,
                topTracksSorted,
                recentUsers,
                topStudents
            );

            return EndpointResponse<AdminDashboardDto>.SuccessResponse(dto);
        }
    }
}
