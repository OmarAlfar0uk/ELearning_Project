using ELearningProject.Contarcts;
using ELearningProject.Features.Shared;
using ELearningProject.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ELearningProject.Features.Batche.GetTopBatchesByScore
{
    public class GetTopBatchesByScoreHandler
        : IRequestHandler<GetTopBatchesByScoreQuery, EndpointResponse<List<TopBatchByScoreDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetTopBatchesByScoreHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<EndpointResponse<List<TopBatchByScoreDto>>> Handle(
            GetTopBatchesByScoreQuery request,
            CancellationToken cancellationToken)
        {
            var limit = Math.Clamp(request.Limit, 1, 50);
            var batchRepository = _unitOfWork.GetRepository<Batch>();
            var submissionRepository = _unitOfWork.GetRepository<Submission>();

            var batchStudentAverages = await submissionRepository
                .FindByCondition(s => s.Score != null && s.IsFinalized)
                .GroupBy(s => new { BatchId = s.Assignment.Lecture.Track.BatchId, s.StudentId })
                .Select(g => new
                {
                    g.Key.BatchId,
                    AverageScore = g.Average(s => s.Score!.Value)
                })
                .ToListAsync(cancellationToken);

            var averageScoreByBatch = batchStudentAverages
                .GroupBy(x => x.BatchId)
                .ToDictionary(g => g.Key, g => g.Average(x => x.AverageScore));

            var batches = await batchRepository
                .GetAll()
                .Select(b => new
                {
                    b.Id,
                    b.Name,
                    StudentCount = b.Students.Count()
                })
                .ToListAsync(cancellationToken);

            var result = batches
                .Select(b => new TopBatchByScoreDto(
                    b.Id,
                    b.Name,
                    Math.Round(averageScoreByBatch.TryGetValue(b.Id, out var averageScore) ? averageScore : 0d, 2),
                    b.StudentCount
                ))
                .OrderByDescending(b => b.AverageScore)
                .ThenByDescending(b => b.StudentCount)
                .ThenBy(b => b.BatchName)
                .Take(limit)
                .ToList();

            return EndpointResponse<List<TopBatchByScoreDto>>.SuccessResponse(
                result,
                "Top batches retrieved successfully.");
        }
    }
}
