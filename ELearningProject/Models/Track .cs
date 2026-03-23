using Auth.Models;

namespace ELearningProject.Models
{
    public class Track : BaseEntity
    {
        public string Name { get; set; } = default!;

        public Guid BatchId { get; set; }
        public Batch Batch { get; set; } = default!;

        public ICollection<Lecture> Lectures { get; set; } = new HashSet<Lecture>();
        public ICollection<InstructorTrack> InstructorTracks { get; set; } = new HashSet<InstructorTrack>();
    }
}
