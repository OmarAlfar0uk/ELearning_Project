using System.Text.Json.Serialization;

namespace ELearningProject.Features.Exams
{
    /// <summary>
    /// Summary representation of an exam.
    /// </summary>
    public record ExamDto(
        Guid Id,
        string Title,
        Guid TrackId,
        int DurationMinutes,
        DateTime DueDate,
        bool IsClosed,
        int QuestionCount);

    /// <summary>
    /// Detailed representation of an exam including its questions.
    /// </summary>
    public record ExamDetailsDto(
        Guid Id,
        string Title,
        Guid TrackId,
        int DurationMinutes,
        DateTime DueDate,
        bool IsClosed,
        List<ExamQuestionDto> Questions);

    /// <summary>
    /// Representation of an exam question including its options.
    /// </summary>
    public record ExamQuestionDto(
        Guid Id,
        string Text,
        string Type,
        int Points,
        int Order,
        List<ExamQuestionOptionDto> Options);

    /// <summary>
    /// Representation of an exam question option.
    /// </summary>
    public class ExamQuestionOptionDto
    {
        public Guid Id { get; init; }
        public string Text { get; init; } = default!;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? IsCorrect { get; init; }
    }
}
