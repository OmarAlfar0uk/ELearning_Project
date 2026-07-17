using Auth.Models;

namespace ELearningProject.Models
{
    /// <summary>
    /// Represents one student's attempt at an exam.
    /// </summary>
    public class ExamAttempt : BaseEntity
    {
        public Guid ExamId { get; set; }
        public Exam Exam { get; set; } = default!;

        public Guid StudentId { get; set; }
        public ApplicationUser Student { get; set; } = default!;

        public DateTime StartedAt { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public int? Score { get; set; }
        public AttemptStatus Status { get; set; } = AttemptStatus.InProgress;

        public ICollection<ExamAnswer> Answers { get; set; } = new HashSet<ExamAnswer>();
    }
}
