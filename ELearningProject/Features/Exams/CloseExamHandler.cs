using ELearningProject.Contarcts;
using ELearningProject.Features.Shared;
using ELearningProject.Models;
using MediatR;

namespace ELearningProject.Features.Exams
{
    /// <summary>
    /// Handler to close an exam.
    /// </summary>
    public class CloseExamHandler : IRequestHandler<CloseExamCommand, EndpointResponse<string>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CloseExamHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<EndpointResponse<string>> Handle(
            CloseExamCommand request,
            CancellationToken cancellationToken)
        {
            var examRepository = _unitOfWork.GetRepository<Exam>();
            var exam = await examRepository.GetByIdAsync(request.ExamId);

            if (exam == null)
            {
                return EndpointResponse<string>.NotFoundResponse("Exam not found.");
            }

            if (exam.IsClosed)
            {
                return EndpointResponse<string>.ErrorResponse(
                    "Exam is already closed.",
                    400);
            }

            exam.IsClosed = true;
            examRepository.Update(exam);
            await _unitOfWork.SaveChangesAsync();

            return EndpointResponse<string>.SuccessResponse(
                "Exam closed successfully.");
        }
    }
}
