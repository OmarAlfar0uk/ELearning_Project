using Auth.Models;

namespace ELearningProject.Models
{
    /// <summary>
    /// Represents a student's answer to one exam question.
    /// </summary>
    public class ExamAnswer : BaseEntity
    {
        public Guid ExamAttemptId { get; set; }
        public ExamAttempt ExamAttempt { get; set; } = default!;

        public Guid ExamQuestionId { get; set; }
        public ExamQuestion ExamQuestion { get; set; } = default!;

        public Guid? SelectedOptionId { get; set; }
        public string? AnswerText { get; set; }
        public bool? IsCorrect { get; set; }
        public int? PointsAwarded { get; set; }
        public string? Feedback { get; set; }
    }
}
