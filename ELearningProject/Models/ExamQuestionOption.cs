using Auth.Models;

namespace ELearningProject.Models
{
    /// <summary>
    /// Represents an option belonging to an exam question.
    /// </summary>
    public class ExamQuestionOption : BaseEntity
    {
        public Guid ExamQuestionId { get; set; }
        public ExamQuestion ExamQuestion { get; set; } = default!;
        public string Text { get; set; } = default!;
        public bool IsCorrect { get; set; }
    }
}
