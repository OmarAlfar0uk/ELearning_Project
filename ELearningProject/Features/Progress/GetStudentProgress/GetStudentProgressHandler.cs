using ELearningProject.Contarcts;
using ELearningProject.Features.Progress.DTOs;
using ELearningProject.Features.Shared;
using ELearningProject.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ELearningProject.Features.Progress.GetStudentProgress
{
    public class GetStudentProgressHandler : IRequestHandler<GetStudentProgressQuery, EndpointResponse<ProgressDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetStudentProgressHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<EndpointResponse<ProgressDto>> Handle(GetStudentProgressQuery request, CancellationToken cancellationToken)
        {
            var trackRepository      = _unitOfWork.GetRepository<Track>();
            var assignmentRepository = _unitOfWork.GetRepository<Assignment>();
            var submissionRepository = _unitOfWork.GetRepository<Submission>();
            var batchStudentRepo     = _unitOfWork.GetRepository<BatchStudent>();
            var progressRepo         = _unitOfWork.GetRepository<ELearningProject.Models.Progress>();

            // 1. Validate track
            var track = await trackRepository.GetByIdAsync(request.TrackId);
            if (track == null)
                return EndpointResponse<ProgressDto>.NotFoundResponse("Track not found.");

            // 2. CompletionPercentage - calculate from finalized submissions per assignment
            var totalAssignments = await assignmentRepository
                .FindByCondition(a => a.Lecture.TrackId == request.TrackId)
                .Select(a => a.Id)
                .CountAsync(cancellationToken);

            int completionPercentage = 0;
            if (totalAssignments > 0)
            {
                var completedAssignmentsCount = await submissionRepository.FindByCondition(s =>
                    s.StudentId == request.StudentId &&
                    s.Assignment.Lecture.TrackId == request.TrackId &&
                    s.Score != null && s.IsFinalized)
                    .Select(s => s.AssignmentId)
                    .Distinct()
                    .CountAsync(cancellationToken);

                var computed = (int)Math.Round(
                    ((double)completedAssignmentsCount / totalAssignments) * 100,
                    MidpointRounding.AwayFromZero);

                // Ensure visible progress for at least one completed assignment.
                if (completedAssignmentsCount > 0 && computed == 0)
                    computed = 1;

                completionPercentage = Math.Clamp(computed, 0, 100);
            }

            // 3. AverageScore - calculated live from all finalized submissions in batch
            var allGrades = await submissionRepository.FindByCondition(s =>
                s.StudentId == request.StudentId &&
                s.Assignment.Lecture.Track.BatchId == track.BatchId &&
                s.Score != null && s.IsFinalized)
                .Select(s => s.Score!.Value)
                .ToListAsync(cancellationToken);

            double averageScore = allGrades.Any() ? allGrades.Average() : 0;

            // 4. Rank - compare against other students in the same batch
            var allBatchStudents = await batchStudentRepo
                .FindByCondition(bs => bs.BatchId == track.BatchId)
                .ToListAsync(cancellationToken);

            // Get each student's average from DB (or recalc in memory for accuracy)
            var studentAverages = new List<(Guid StudentId, double Avg)>();
            foreach (var bs in allBatchStudents)
            {
                if (bs.StudentId == request.StudentId)
                {
                    studentAverages.Add((bs.StudentId, averageScore));
                }
                else
                {
                    var otherGrades = await submissionRepository.FindByCondition(s =>
                        s.StudentId == bs.StudentId &&
                        s.Assignment.Lecture.Track.BatchId == track.BatchId &&
                        s.Score != null && s.IsFinalized)
                        .Select(s => s.Score!.Value)
                        .ToListAsync(cancellationToken);

                    studentAverages.Add((bs.StudentId, otherGrades.Any() ? otherGrades.Average() : 0));
                }
            }

            var sortedAverages = studentAverages.OrderByDescending(x => x.Avg).ToList();
            int rank = 1;
            for (int i = 0; i < sortedAverages.Count; i++)
            {
                if (i > 0 && sortedAverages[i].Avg < sortedAverages[i - 1].Avg)
                    rank = i + 1;

                if (sortedAverages[i].StudentId == request.StudentId)
                {
                    rank = (i > 0 && sortedAverages[i].Avg < sortedAverages[i - 1].Avg) ? i + 1 : rank;
                    break;
                }
            }

            return EndpointResponse<ProgressDto>.SuccessResponse(new ProgressDto(
                request.StudentId,
                request.TrackId,
                completionPercentage,
                rank,
                averageScore
            ));
        }
    }
}
