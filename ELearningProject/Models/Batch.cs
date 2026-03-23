using Auth.Models;

namespace ELearningProject.Models
{
    public class Batch : BaseEntity
    {
        public string Name { get; set; } = default!;
        public DateTime StartDate { get; set; }

        public ICollection<BatchStudent> Students { get; set; } = new HashSet<BatchStudent>();
        public ICollection<Track> Tracks { get; set; } = new HashSet<Track>();
    }
}
