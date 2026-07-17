using ELearningProject.Contarcts;
using ELearningProject.Features.Shared;
using ELearningProject.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ELearningProject.Features.Exams
{
    /// <summary>
    /// Handler to update an exam before it is closed.
    /// </summary>
    public class UpdateExamHandler : IRequestHandler<UpdateExamCommand, EndpointResponse<ExamDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateExamHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<EndpointResponse<ExamDto>> Handle(
            UpdateExamCommand request,
            CancellationToken cancellationToken)
        {
            var examRepository = _unitOfWork.GetRepository<Exam>();
            var exam = await examRepository.GetByIdAsync(request.ExamId);

            if (exam == null)
            {
                return EndpointResponse<ExamDto>.NotFoundResponse("Exam not found.");
            }

            if (exam.IsClosed)
            {
                return EndpointResponse<ExamDto>.ErrorResponse(
                    "Cannot update a closed exam.",
                    400);
            }

            exam.Title = request.Title;
            exam.DurationMinutes = request.DurationMinutes;
            exam.DueDate = request.DueDate;

            examRepository.Update(exam);
            await _unitOfWork.SaveChangesAsync();

            var questionCount = await _unitOfWork
                .GetRepository<ExamQuestion>()
                .FindByCondition(question => question.ExamId == exam.Id)
                .CountAsync(cancellationToken);

            var dto = new ExamDto(
                exam.Id,
                exam.Title,
                exam.TrackId,
                exam.DurationMinutes,
                exam.DueDate,
                exam.IsClosed,
                questionCount);

            return EndpointResponse<ExamDto>.SuccessResponse(
                dto,
                "Exam updated successfully.");
        }
    }
}
