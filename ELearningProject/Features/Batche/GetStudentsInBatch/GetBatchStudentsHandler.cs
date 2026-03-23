
using ELearningProject.Contarcts;
using ELearningProject.Features.Shared;
using ELearningProject.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ELearningProject.Features.Batche.GetStudentsInBatch
{
    public class GetBatchStudentsHandler : IRequestHandler<GetBatchStudentsQuery, EndpointResponse<List<BatchStudentItemDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetBatchStudentsHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<EndpointResponse<List<BatchStudentItemDto>>> Handle(GetBatchStudentsQuery request, CancellationToken cancellationToken)
        {
            var batchRepository = _unitOfWork.GetRepository<Batch>();
            var submissionRepo = _unitOfWork.GetRepository<Submission>();

            var batch = await batchRepository.FindByCondition(b => b.Id == request.BatchId)
                .Include(b => b.Students.Where(bs => !bs.IsDeleted))
                    .ThenInclude(bs => bs.Student)
                .FirstOrDefaultAsync(cancellationToken);

            if (batch == null)
                return EndpointResponse<List<BatchStudentItemDto>>.NotFoundResponse("Batch not found.");

            var activeStudents = batch.Students.Where(bs => !bs.IsDeleted).ToList();

            // ── Step 1: Get all finalized submission scores for this batch ──
            var studentIds = activeStudents.Select(bs => bs.StudentId).ToList();

            var allScores = await submissionRepo
                .FindByCondition(s =>
                    studentIds.Contains(s.StudentId) &&
                    s.Assignment.Lecture.Track.BatchId == request.BatchId &&
                    s.Score != null && s.IsFinalized)
                .Select(s => new { s.StudentId, Score = s.Score!.Value })
                .ToListAsync(cancellationToken);

            // ── Step 2: Calculate AverageScore per student in memory ──
            var scoreMap = allScores
                .GroupBy(s => s.StudentId)
                .ToDictionary(g => g.Key, g => g.Average(s => s.Score));

            // ── Step 3: Build result list with dynamic AverageScore ──
            var result = activeStudents
                .Select(bs => new
                {
                    bs.StudentId,
                    FullName = $"{bs.Student.FirstName} {bs.Student.LastName}",
                    bs.Student.Email,
                    AverageScore = scoreMap.TryGetValue(bs.StudentId, out var avg) ? avg : 0.0
                })
                .OrderByDescending(x => x.AverageScore)
                .ToList();

            // ── Step 4: Assign Rank dynamically (ties get same rank) ──
            var students = new List<BatchStudentItemDto>();
            int currentRank = 1;
            for (int i = 0; i < result.Count; i++)
            {
                if (i > 0 && result[i].AverageScore < result[i - 1].AverageScore)
                    currentRank = i + 1;

                students.Add(new BatchStudentItemDto(
                    result[i].StudentId,
                    result[i].FullName,
                    result[i].Email!,
                    currentRank,
                    Math.Round(result[i].AverageScore, 2)
                ));
            }

            return EndpointResponse<List<BatchStudentItemDto>>.SuccessResponse(students);
        }
    }
}
