using ELearningProject.Contarcts;
using ELearningProject.Features.Shared;
using ELearningProject.Models;
using MediatR;

namespace ELearningProject.Features.Exams
{
    /// <summary>
    /// Handler to add a question to an open exam.
    /// </summary>
    public class AddExamQuestionHandler : IRequestHandler<AddExamQuestionCommand, EndpointResponse<ExamQuestionDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public AddExamQuestionHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<EndpointResponse<ExamQuestionDto>> Handle(
            AddExamQuestionCommand request,
            CancellationToken cancellationToken)
        {
            var examRepository = _unitOfWork.GetRepository<Exam>();
            var exam = await examRepository.GetByIdAsync(request.ExamId);

            if (exam == null)
            {
                return EndpointResponse<ExamQuestionDto>.NotFoundResponse("Exam not found.");
            }

            if (exam.IsClosed)
            {
                return EndpointResponse<ExamQuestionDto>.ErrorResponse(
                    "Cannot add questions to a closed exam.",
                    400);
            }

            var options = (request.Options ?? new List<CreateOptionRequest>())
                .Select(option => new ExamQuestionOption
                {
                    Text = option.Text,
                    IsCorrect = option.IsCorrect
                })
                .ToList();

            var question = new ExamQuestion
            {
                ExamId = request.ExamId,
                Text = request.Text,
                Type = request.Type,
                Points = request.Points,
                Order = request.Order,
                Options = options
            };

            var questionRepository = _unitOfWork.GetRepository<ExamQuestion>();
            await questionRepository.CreateAsync(question);
            await _unitOfWork.SaveChangesAsync();

            var dto = new ExamQuestionDto(
                question.Id,
                question.Text,
                question.Type.ToString(),
                question.Points,
                question.Order,
                question.Options.Select(option => new ExamQuestionOptionDto
                {
                    Id = option.Id,
                    Text = option.Text,
                    IsCorrect = option.IsCorrect
                }).ToList());

            return EndpointResponse<ExamQuestionDto>.SuccessResponse(
                dto,
                "Exam question added successfully.",
                201);
        }
    }
}
