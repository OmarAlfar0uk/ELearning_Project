using ELearningProject.Contarcts;
using ELearningProject.Features.Shared;
using ELearningProject.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ELearningProject.Features.Exams
{
    /// <summary>
    /// Handler to retrieve exam summaries for a track.
    /// </summary>
    public class GetTrackExamsHandler : IRequestHandler<GetTrackExamsQuery, EndpointResponse<List<ExamDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetTrackExamsHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<EndpointResponse<List<ExamDto>>> Handle(
            GetTrackExamsQuery request,
            CancellationToken cancellationToken)
        {
            var trackRepository = _unitOfWork.GetRepository<Track>();
            var track = await trackRepository.GetByIdAsync(request.TrackId);

            if (track == null)
            {
                return EndpointResponse<List<ExamDto>>.NotFoundResponse("Track not found.");
            }

            var examRepository = _unitOfWork.GetRepository<Exam>();
            var exams = await examRepository
                .FindByCondition(exam => exam.TrackId == request.TrackId)
                .OrderBy(exam => exam.DueDate)
                .Select(exam => new ExamDto(
                    exam.Id,
                    exam.Title,
                    exam.TrackId,
                    exam.DurationMinutes,
                    exam.DueDate,
                    exam.IsClosed,
                    exam.Questions.Count))
                .ToListAsync(cancellationToken);

            return EndpointResponse<List<ExamDto>>.SuccessResponse(
                exams,
                "Track exams retrieved successfully.");
        }
    }
}
