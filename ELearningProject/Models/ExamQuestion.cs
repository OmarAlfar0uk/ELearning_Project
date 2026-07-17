using Auth.Models;

namespace ELearningProject.Models
{
    /// <summary>
    /// Represents a question belonging to an exam.
    /// </summary>
    public class ExamQuestion : BaseEntity
    {
        public Guid ExamId { get; set; }
        public Exam Exam { get; set; } = default!;
        public string Text { get; set; } = default!;
        public QuestionType Type { get; set; }
        public int Points { get; set; }
        public int Order { get; set; }
        public ICollection<ExamQuestionOption> Options { get; set; } = new HashSet<ExamQuestionOption>();
    }
}
