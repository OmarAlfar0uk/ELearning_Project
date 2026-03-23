
using ELearningProject.Contarcts;
using ELearningProject.Models;
using Microsoft.EntityFrameworkCore;

namespace ELearningProject.Features.Progress.Services
{
    public interface IProgressService
    {
        Task UpdateProgressAsync(Guid studentId, Guid trackId, CancellationToken cancellationToken);
    }

    public class ProgressService : IProgressService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProgressService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task UpdateProgressAsync(Guid studentId, Guid trackId, CancellationToken cancellationToken)
        {
            var progressRepository = _unitOfWork.GetRepository<ELearningProject.Models.Progress>();
            var assignmentRepository = _unitOfWork.GetRepository<Assignment>();
            var submissionRepository = _unitOfWork.GetRepository<Submission>();
            var trackRepository = _unitOfWork.GetRepository<Track>();
            var batchStudentRepository = _unitOfWork.GetRepository<BatchStudent>();

            
            var totalAssignments = await assignmentRepository.FindByCondition(a => a.Lecture.TrackId == trackId)
                .CountAsync(cancellationToken);

            if (totalAssignments == 0) return;

            var completedCount = await submissionRepository.FindByCondition(s => 
                s.Assignment.Lecture.TrackId == trackId &&
                s.StudentId == studentId &&
                s.Score != null && 
                s.IsFinalized)
                .CountAsync(cancellationToken);

            var percentage = (int)(((double)completedCount / totalAssignments) * 100);
            if (percentage > 100) percentage = 100;

            var progress = await progressRepository.FindByCondition(p => 
                p.StudentId == studentId && p.TrackId == trackId)
                .FirstOrDefaultAsync(cancellationToken);

            if (progress == null)
            {
                progress = new ELearningProject.Models.Progress
                {
                    StudentId = studentId,
                    TrackId = trackId,
                    CompletionPercentage = percentage
                };
                await progressRepository.CreateAsync(progress);
            }
            else
            {
                progress.CompletionPercentage = percentage;
                progressRepository.Update(progress);
            }

            // 5. Update BatchStudent Average Score and Rank
            var track = await trackRepository.GetByIdAsync(trackId);
            if (track != null)
            {
                // Get all students in the same batch
                var allBatchStudents = await batchStudentRepository
                    .FindByCondition(bs => bs.BatchId == track.BatchId)
                    .ToListAsync(cancellationToken);

                // For each student, recalculate their average score from finalized submissions
                foreach (var bs in allBatchStudents)
                {
                    var grades = await submissionRepository.FindByCondition(s =>
                        s.StudentId == bs.StudentId &&
                        s.Assignment.Lecture.Track.BatchId == track.BatchId &&
                        s.Score != null && s.IsFinalized)
                        .Select(s => s.Score!.Value)
                        .ToListAsync(cancellationToken);

                    bs.AverageScore = grades.Any() ? grades.Average() : 0;
                }

                // Sort in memory by AverageScore descending and assign ranks
                var ranked = allBatchStudents.OrderByDescending(bs => bs.AverageScore).ToList();
                int currentRank = 1;
                for (int i = 0; i < ranked.Count; i++)
                {
                    if (i > 0 && ranked[i].AverageScore < ranked[i - 1].AverageScore)
                        currentRank = i + 1;

                    ranked[i].Rank = currentRank;
                    batchStudentRepository.Update(ranked[i]);
                }
            }
        }
    }
}
