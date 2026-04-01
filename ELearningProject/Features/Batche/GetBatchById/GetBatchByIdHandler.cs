using ELearningProject.Contarcts;
using ELearningProject.Features.Shared;
using ELearningProject.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ELearningProject.Features.Batche.GetBatchById
{
    public class GetBatchByIdHandler : IRequestHandler<GetBatchByIdQuery, EndpointResponse<BatchDetailsDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetBatchByIdHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<EndpointResponse<BatchDetailsDto>> Handle(GetBatchByIdQuery request, CancellationToken cancellationToken)
        {
            var batchRepository = _unitOfWork.GetRepository<Batch>();

            var batch = await batchRepository.FindByCondition(b => b.Id == request.Id && !b.IsDeleted)
                .Include(b => b.Tracks)
                    .ThenInclude(t => t.Lectures)
                .Include(b => b.Students.Where(bs => !bs.IsDeleted))
                    .ThenInclude(bs => bs.Student)
                .FirstOrDefaultAsync(cancellationToken);

            if (batch == null)
            {
                return EndpointResponse<BatchDetailsDto>.NotFoundResponse("Batch not found.");
            }

            var submissionRepo = _unitOfWork.GetRepository<Submission>();

            var activeStudents = batch.Students.Where(bs => !bs.IsDeleted).ToList();
            var studentIds = activeStudents.Select(bs => bs.StudentId).ToList();

            var allScores = await submissionRepo
                .FindByCondition(s =>
                    studentIds.Contains(s.StudentId) &&
                    s.Assignment.Lecture.Track.BatchId == request.Id &&
                    s.Score != null && s.IsFinalized)
                .Select(s => new { s.StudentId, Score = s.Score!.Value })
                .ToListAsync(cancellationToken);

            var scoreMap = allScores
                .GroupBy(s => s.StudentId)
                .ToDictionary(g => g.Key, g => g.Average(s => s.Score));

            var orderedStudents = activeStudents
                .Select(bs => new
                {
                    bs.Student,
                    AverageScore = scoreMap.TryGetValue(bs.StudentId, out var avg) ? avg : 0.0
                })
                .OrderByDescending(x => x.AverageScore)
                .ToList();

            var studentDetails = new List<BatchStudentDetailDto>();
            int currentRank = 1;
            for (int i = 0; i < orderedStudents.Count; i++)
            {
                if (i > 0 && orderedStudents[i].AverageScore < orderedStudents[i - 1].AverageScore)
                    currentRank = i + 1;

                studentDetails.Add(new BatchStudentDetailDto(
                    orderedStudents[i].Student.Id,
                    $"{orderedStudents[i].Student.FirstName} {orderedStudents[i].Student.LastName}",
                    orderedStudents[i].Student.Email!,
                    currentRank,
                    orderedStudents[i].AverageScore
                ));
            }

            var batchDto = new BatchDetailsDto(
                batch.Id,
                batch.Name,
                batch.StartDate,
                studentDetails,
                batch.Tracks.Select(t => new BatchTrackDto(
                    t.Id,
                    t.Name,
                    t.Lectures.Count // Assuming Lectures are not loaded, this might require explicit Include if lazy loading is off.
                )).ToList()
            );


            return EndpointResponse<BatchDetailsDto>.SuccessResponse(batchDto);
        }
    }
}
