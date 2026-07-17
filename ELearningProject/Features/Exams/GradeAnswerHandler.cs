using ELearningProject.Contarcts;
using ELearningProject.Features.Shared;
using ELearningProject.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ELearningProject.Features.Exams
{
    /// <summary>
    /// Handler to manually grade an essay or fill-in-the-blank answer.
    /// </summary>
    public class GradeAnswerHandler : IRequestHandler<GradeAnswerCommand, EndpointResponse<string>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GradeAnswerHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<EndpointResponse<string>> Handle(
            GradeAnswerCommand request,
            CancellationToken cancellationToken)
        {
            var answerRepository = _unitOfWork.GetRepository<ExamAnswer>();
            var answer = await answerRepository
                .FindByCondition(currentAnswer => currentAnswer.Id == request.AnswerId)
                .Include(currentAnswer => currentAnswer.ExamQuestion)
                    .ThenInclude(question => question.Exam)
                .Include(currentAnswer => currentAnswer.ExamAttempt)
                .FirstOrDefaultAsync(cancellationToken);

            if (answer == null)
            {
                return EndpointResponse<string>.NotFoundResponse("Answer not found.");
            }

            var questionType = answer.ExamQuestion.Type;
            if (questionType != QuestionType.Essay &&
                questionType != QuestionType.FillInBlank)
            {
                return EndpointResponse<string>.ErrorResponse(
                    "Only Essay and FillInBlank answers can be manually graded.",
                    400);
            }

            if (answer.ExamAttempt.Status == AttemptStatus.InProgress)
            {
                return EndpointResponse<string>.ErrorResponse(
                    "The exam attempt has not been submitted yet.",
                    400);
            }

            if (request.PointsAwarded > answer.ExamQuestion.Points)
            {
                return EndpointResponse<string>.ErrorResponse(
                    $"PointsAwarded cannot exceed {answer.ExamQuestion.Points}.",
                    400);
            }

            answer.PointsAwarded = request.PointsAwarded;
            answer.Feedback = request.Feedback;

            answerRepository.Update(answer);
            await _unitOfWork.SaveChangesAsync();

            var attemptAnswers = await answerRepository
                .FindByCondition(currentAnswer =>
                    currentAnswer.ExamAttemptId == answer.ExamAttemptId)
                .ToListAsync(cancellationToken);

            if (attemptAnswers.All(currentAnswer => currentAnswer.PointsAwarded.HasValue))
            {
                var attemptRepository = _unitOfWork.GetRepository<ExamAttempt>();
                var attempt = await attemptRepository.GetByIdAsync(answer.ExamAttemptId);

                if (attempt != null)
                {
                    attempt.Score = attemptAnswers.Sum(currentAnswer =>
                        currentAnswer.PointsAwarded ?? 0);
                    attempt.Status = AttemptStatus.Graded;
                    attemptRepository.Update(attempt);
                    await _unitOfWork.SaveChangesAsync();
                }
            }

            return EndpointResponse<string>.SuccessResponse(
                "Answer graded successfully.");
        }
    }
}
