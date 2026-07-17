using ELearningProject.Contarcts;
using ELearningProject.Extensions;
using ELearningProject.Features.Coins.Services;
using ELearningProject.Features.Shared;
using ELearningProject.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace ELearningProject.Features.Exams
{
    /// <summary>
    /// Handler to submit an exam attempt and grade objective answers.
    /// </summary>
    public class SubmitExamHandler : IRequestHandler<SubmitExamCommand, EndpointResponse<ExamAttemptDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICoinAwardService _coinAwardService;

        public SubmitExamHandler(
            IUnitOfWork unitOfWork,
            IHttpContextAccessor httpContextAccessor,
            ICoinAwardService coinAwardService)
        {
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
            _coinAwardService = coinAwardService;
        }

        public async Task<EndpointResponse<ExamAttemptDto>> Handle(
            SubmitExamCommand request,
            CancellationToken cancellationToken)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var studentId = user?.GetUserId() ?? Guid.Empty;

            if (studentId == Guid.Empty)
            {
                return EndpointResponse<ExamAttemptDto>.UnauthorizedResponse();
            }

            var attemptRepository = _unitOfWork.GetRepository<ExamAttempt>();
            var attempt = await attemptRepository
                .FindByCondition(currentAttempt => currentAttempt.Id == request.AttemptId)
                .Include(currentAttempt => currentAttempt.Exam)
                    .ThenInclude(exam => exam.Questions)
                        .ThenInclude(question => question.Options)
                .FirstOrDefaultAsync(cancellationToken);

            if (attempt == null)
            {
                return EndpointResponse<ExamAttemptDto>.NotFoundResponse("Exam attempt not found.");
            }

            if (attempt.StudentId != studentId)
            {
                return EndpointResponse<ExamAttemptDto>.ErrorResponse(
                    "You do not have access to this exam attempt.",
                    403);
            }

            if (attempt.Status != AttemptStatus.InProgress)
            {
                return EndpointResponse<ExamAttemptDto>.ErrorResponse(
                    "This exam attempt has already been submitted.",
                    400);
            }

            var now = DateTime.UtcNow;
            var expirationTime = attempt.StartedAt
                .AddMinutes(attempt.Exam.DurationMinutes)
                .AddSeconds(30);

            if (now > expirationTime)
            {
                return EndpointResponse<ExamAttemptDto>.ErrorResponse(
                    "The exam attempt time has expired.",
                    400);
            }

            var submittedAnswers = request.Answers ?? new List<SubmitAnswerRequest>();
            var questionsById = attempt.Exam.Questions.ToDictionary(question => question.Id);

            if (submittedAnswers
                .GroupBy(answer => answer.ExamQuestionId)
                .Any(group => group.Count() > 1))
            {
                return EndpointResponse<ExamAttemptDto>.ErrorResponse(
                    "Each exam question may only be submitted once.",
                    400);
            }

            var answerRepository = _unitOfWork.GetRepository<ExamAnswer>();
            var answersToCreate = new List<ExamAnswer>();

            foreach (var submittedAnswer in submittedAnswers)
            {
                if (!questionsById.ContainsKey(submittedAnswer.ExamQuestionId))
                {
                    return EndpointResponse<ExamAttemptDto>.ErrorResponse(
                        "One or more submitted questions do not belong to this exam.",
                        400);
                }
            }

            var submittedAnswersByQuestionId = submittedAnswers
                .ToDictionary(answer => answer.ExamQuestionId);

            foreach (var question in attempt.Exam.Questions)
            {
                submittedAnswersByQuestionId.TryGetValue(question.Id, out var submittedAnswer);

                var isObjective = question.Type == QuestionType.MultipleChoice ||
                                  question.Type == QuestionType.TrueFalse;

                if (submittedAnswer == null)
                {
                    answersToCreate.Add(new ExamAnswer
                    {
                        ExamAttemptId = attempt.Id,
                        ExamQuestionId = question.Id,
                        SelectedOptionId = null,
                        AnswerText = null,
                        IsCorrect = isObjective ? false : null,
                        PointsAwarded = isObjective ? 0 : null
                    });
                }
                else if (isObjective)
                {
                    if (submittedAnswer.SelectedOptionId.HasValue &&
                        !question.Options.Any(option =>
                            option.Id == submittedAnswer.SelectedOptionId.Value))
                    {
                        return EndpointResponse<ExamAttemptDto>.ErrorResponse(
                            "One or more selected options do not belong to their questions.",
                            400);
                    }

                    var correctOption = question.Options
                        .FirstOrDefault(option => option.IsCorrect);

                    var isCorrect = correctOption != null &&
                                    submittedAnswer.SelectedOptionId.HasValue &&
                                    correctOption.Id == submittedAnswer.SelectedOptionId.Value;

                    answersToCreate.Add(new ExamAnswer
                    {
                        ExamAttemptId = attempt.Id,
                        ExamQuestionId = question.Id,
                        SelectedOptionId = submittedAnswer.SelectedOptionId,
                        IsCorrect = isCorrect,
                        PointsAwarded = isCorrect ? question.Points : 0
                    });
                }
                else
                {
                    if (submittedAnswer.SelectedOptionId.HasValue)
                    {
                        return EndpointResponse<ExamAttemptDto>.ErrorResponse(
                            "Essay and FillInBlank answers cannot select an option.",
                            400);
                    }

                    answersToCreate.Add(new ExamAnswer
                    {
                        ExamAttemptId = attempt.Id,
                        ExamQuestionId = question.Id,
                        AnswerText = submittedAnswer.AnswerText,
                        IsCorrect = null,
                        PointsAwarded = null
                    });
                }
            }

            foreach (var answer in answersToCreate)
            {
                await answerRepository.CreateAsync(answer);
            }

            attempt.SubmittedAt = now;

            var requiresManualGrading = attempt.Exam.Questions.Any(question =>
                question.Type == QuestionType.Essay ||
                question.Type == QuestionType.FillInBlank);

            if (requiresManualGrading)
            {
                attempt.Status = AttemptStatus.Submitted;
                attempt.Score = null;
            }
            else
            {
                attempt.Status = AttemptStatus.Graded;
                attempt.Score = answersToCreate.Sum(answer => answer.PointsAwarded ?? 0);
            }

            attemptRepository.Update(attempt);
            await _unitOfWork.SaveChangesAsync();

            // ── Stage 2: award coins for passing an auto-graded exam ───────────────
            // Only fires when all questions are objective (Status flipped to Graded above).
            // Manual-grading path (Status = Submitted) is handled by GradeAnswerHandler
            // once the last answer is scored.
            // Wrapped in try/catch: a coin-award failure must never roll back the
            // committed exam result.
            if (attempt.Status == AttemptStatus.Graded)
            {
                try
                {
                    await _coinAwardService.AwardExamCoinsIfEligibleAsync(
                        attempt.Id, cancellationToken);
                }
                catch (Exception ex)
                {
                    Log.Error(
                        ex,
                        "CoinAward | SubmitExamHandler | AttemptId={AttemptId} | {Message}",
                        attempt.Id, ex.Message);
                }
            }

            var dto = new ExamAttemptDto(
                attempt.Id,
                attempt.ExamId,
                attempt.StudentId,
                attempt.StartedAt,
                attempt.SubmittedAt,
                attempt.Status.ToString(),
                attempt.Score);

            return EndpointResponse<ExamAttemptDto>.SuccessResponse(
                dto,
                "Exam submitted successfully.");
        }
    }
}
