using ELearningProject.Contarcts;
using ELearningProject.Features.Shared;
using ELearningProject.Models;
using MediatR;

namespace ELearningProject.Features.Exams
{
    /// <summary>
    /// Handler to create an exam after verifying its track exists.
    /// </summary>
    public class CreateExamHandler : IRequestHandler<CreateExamCommand, EndpointResponse<ExamDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateExamHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<EndpointResponse<ExamDto>> Handle(
            CreateExamCommand request,
            CancellationToken cancellationToken)
        {
            var trackRepository = _unitOfWork.GetRepository<Track>();
            var track = await trackRepository.GetByIdAsync(request.TrackId);

            if (track == null)
            {
                return EndpointResponse<ExamDto>.NotFoundResponse("Track not found.");
            }

            var exam = new Exam
            {
                Title = request.Title,
                TrackId = request.TrackId,
                DurationMinutes = request.DurationMinutes,
                DueDate = request.DueDate
            };

            var examRepository = _unitOfWork.GetRepository<Exam>();
            await examRepository.CreateAsync(exam);
            await _unitOfWork.SaveChangesAsync();

            var dto = new ExamDto(
                exam.Id,
                exam.Title,
                exam.TrackId,
                exam.DurationMinutes,
                exam.DueDate,
                exam.IsClosed,
                0);

            return EndpointResponse<ExamDto>.SuccessResponse(
                dto,
                "Exam created successfully.",
                201);
        }
    }
}
