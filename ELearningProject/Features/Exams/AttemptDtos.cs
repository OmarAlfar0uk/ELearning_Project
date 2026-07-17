namespace ELearningProject.Features.Exams
{
    /// <summary>
    /// Representation of a student's exam attempt.
    /// </summary>
    public record ExamAttemptDto(
        Guid Id,
        Guid ExamId,
        Guid StudentId,
        DateTime StartedAt,
        DateTime? SubmittedAt,
        string Status,
        int? Score);

    /// <summary>
    /// Request model for one submitted exam answer.
    /// </summary>
    public record SubmitAnswerRequest(
        Guid ExamQuestionId,
        Guid? SelectedOptionId,
        string? AnswerText);

    /// <summary>
    /// Representation of one answer in an attempt result.
    /// </summary>
    public record ExamAttemptAnswerDto(
        Guid AnswerId,
        Guid ExamQuestionId,
        string QuestionText,
        Guid? SelectedOptionId,
        string? AnswerText,
        bool? IsCorrect,
        int? PointsAwarded);

    /// <summary>
    /// Detailed result representation for an exam attempt.
    /// </summary>
    public record ExamAttemptResultDto(
        Guid AttemptId,
        Guid ExamId,
        string Status,
        string GradingStatus,
        int? Score,
        List<ExamAttemptAnswerDto> Answers);

    /// <summary>
    /// Summary representation of an exam attempt for teaching staff.
    /// </summary>
    public record ExamAttemptSummaryDto(
        Guid AttemptId,
        Guid ExamId,
        Guid StudentId,
        string StudentName,
        string Status,
        int? Score,
        DateTime StartedAt,
        DateTime? SubmittedAt);
}
