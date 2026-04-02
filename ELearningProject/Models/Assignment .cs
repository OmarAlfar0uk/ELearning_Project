using Auth.Models;

namespace ELearningProject.Models
{
    public class Assignment : BaseEntity
    {
        public string Title { get; set; } = default!;
        public int MaxScore { get; set; } = 10;
        public DateTime? DueDate { get; set; }
        public bool IsClosed { get; set; } = false;
        public string? FileUrl { get; set; }

        public Guid LectureId { get; set; }
        public Lecture Lecture { get; set; } = default!;

        public ICollection<Submission> Submissions { get; set; } = new HashSet<Submission>();
        public Guid TrackId { get; internal set; }
    }
}
