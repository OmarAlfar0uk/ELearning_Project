using ELearningProject.Contarcts;
using ELearningProject.Extensions;
using ELearningProject.Features.Shared;
using ELearningProject.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ELearningProject.Features.Exams
{
    /// <summary>
    /// Handler to retrieve an exam with questions and role-aware options.
    /// </summary>
    public class GetExamDetailsHandler : IRequestHandler<GetExamDetailsQuery, EndpointResponse<ExamDetailsDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GetExamDetailsHandler(
            IUnitOfWork unitOfWork,
            IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<EndpointResponse<ExamDetailsDto>> Handle(
            GetExamDetailsQuery request,
            CancellationToken cancellationToken)
        {
            var examRepository = _unitOfWork.GetRepository<Exam>();
            var exam = await examRepository
                .FindByCondition(e => e.Id == request.ExamId)
                .Include(e => e.Questions)
                    .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(cancellationToken);

            if (exam == null)
            {
                return EndpointResponse<ExamDetailsDto>.NotFoundResponse("Exam not found.");
            }

            var isStudent = _httpContextAccessor.HttpContext?.User.GetUserRole() == "Student";

            var questions = exam.Questions
                .OrderBy(question => question.Order)
                .Select(question => new ExamQuestionDto(
                    question.Id,
                    question.Text,
                    question.Type.ToString(),
                    question.Points,
                    question.Order,
                    question.Options
                        .OrderBy(option => option.CreatedAt)
                        .Select(option => new ExamQuestionOptionDto
                        {
                            Id = option.Id,
                            Text = option.Text,
                            IsCorrect = isStudent ? null : option.IsCorrect
                        })
                        .ToList()))
                .ToList();

            var dto = new ExamDetailsDto(
                exam.Id,
                exam.Title,
                exam.TrackId,
                exam.DurationMinutes,
                exam.DueDate,
                exam.IsClosed,
                questions);

            return EndpointResponse<ExamDetailsDto>.SuccessResponse(
                dto,
                "Exam details retrieved successfully.");
        }
    }
}
