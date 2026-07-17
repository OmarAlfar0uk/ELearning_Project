using Auth.Models;

namespace ELearningProject.Models
{
    /// <summary>
    /// Represents an exam associated with a track.
    /// </summary>
    public class Exam : BaseEntity
    {
        public string Title { get; set; } = default!;
        public Guid TrackId { get; set; }
        public Track Track { get; set; } = default!;
        public int DurationMinutes { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsClosed { get; set; } = false;
        public ICollection<ExamQuestion> Questions { get; set; } = new HashSet<ExamQuestion>();
    }
}
