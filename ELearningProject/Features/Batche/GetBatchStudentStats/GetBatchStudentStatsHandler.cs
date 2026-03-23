using Auth.Contarcts;
using ELearningProject.Contarcts;
using ELearningProject.Features.Shared;
using ELearningProject.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ELearningProject.Features.Batche.GetBatchStudentStats
{
    public class GetBatchStudentStatsHandler
        : IRequestHandler<GetBatchStudentStatsQuery, EndpointResponse<BatchStudentStatsDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IImageHelper _imageHelper;

        public GetBatchStudentStatsHandler(IUnitOfWork unitOfWork, IImageHelper imageHelper)
        {
            _unitOfWork = unitOfWork;
            _imageHelper = imageHelper;
        }

        public async Task<EndpointResponse<BatchStudentStatsDto>> Handle(
            GetBatchStudentStatsQuery request,
            CancellationToken cancellationToken)
        {
            var batchRepository = _unitOfWork.GetRepository<Batch>();
            var batchStudentRepository = _unitOfWork.GetRepository<BatchStudent>();
            var submissionRepository = _unitOfWork.GetRepository<Submission>();

            var batch = await batchRepository
                .FindByCondition(b => b.Id == request.BatchId)
                .Select(b => new { b.Id, b.Name })
                .FirstOrDefaultAsync(cancellationToken);

            if (batch == null)
                return EndpointResponse<BatchStudentStatsDto>.NotFoundResponse("Batch not found.");

            var studentsInBatch = await batchStudentRepository
                .FindByCondition(bs => bs.BatchId == request.BatchId)
                .Include(bs => bs.Student)
                .Select(bs => new
                {
                    bs.StudentId,
                    bs.Student.FirstName,
                    bs.Student.LastName,
                    Email = bs.Student.Email,
                    bs.Student.ProfileImageUrl
                })
                .ToListAsync(cancellationToken);

            if (studentsInBatch.Count == 0)
                return EndpointResponse<BatchStudentStatsDto>.NotFoundResponse("No students found in this batch.");

            var targetStudent = studentsInBatch.FirstOrDefault(s => s.StudentId == request.StudentId);
            if (targetStudent == null)
                return EndpointResponse<BatchStudentStatsDto>.NotFoundResponse("Student not found in this batch.");

            var scoreRows = await submissionRepository
                .FindByCondition(s =>
                    s.Assignment.Lecture.Track.BatchId == request.BatchId &&
                    s.Score != null &&
                    s.IsFinalized)
                .GroupBy(s => s.StudentId)
                .Select(g => new
                {
                    StudentId = g.Key,
                    AverageScore = g.Average(s => s.Score!.Value)
                })
                .ToListAsync(cancellationToken);

            var averageScoreByStudent = scoreRows.ToDictionary(x => x.StudentId, x => x.AverageScore);

            var rankedStudents = studentsInBatch
                .Select(s => new
                {
                    s.StudentId,
                    s.FirstName,
                    s.LastName,
                    s.Email,
                    s.ProfileImageUrl,
                    AverageScore = averageScoreByStudent.TryGetValue(s.StudentId, out var averageScore)
                        ? averageScore
                        : 0d
                })
                .OrderByDescending(s => s.AverageScore)
                .ThenBy(s => s.LastName)
                .ThenBy(s => s.FirstName)
                .ToList();

            var submittedAssignmentsCount = await submissionRepository
                .FindByCondition(s =>
                    s.StudentId == request.StudentId &&
                    s.Assignment.Lecture.Track.BatchId == request.BatchId)
                .CountAsync(cancellationToken);

            var currentRank = 1;
            for (var index = 0; index < rankedStudents.Count; index++)
            {
                if (index > 0 && rankedStudents[index].AverageScore < rankedStudents[index - 1].AverageScore)
                    currentRank = index + 1;

                if (rankedStudents[index].StudentId != request.StudentId)
                    continue;

                var studentStats = new BatchStudentStatsDto(
                    rankedStudents[index].StudentId,
                    $"{rankedStudents[index].FirstName} {rankedStudents[index].LastName}".Trim(),
                    rankedStudents[index].Email ?? string.Empty,
                    batch.Name,
                    currentRank,
                    Math.Round(rankedStudents[index].AverageScore, 2),
                    submittedAssignmentsCount,
                    _imageHelper.GetImageUrl(rankedStudents[index].ProfileImageUrl)
                );

                return EndpointResponse<BatchStudentStatsDto>.SuccessResponse(
                    studentStats,
                    "Student stats retrieved successfully.");
            }

            return EndpointResponse<BatchStudentStatsDto>.NotFoundResponse("Student ranking could not be calculated.");
        }
    }
}
