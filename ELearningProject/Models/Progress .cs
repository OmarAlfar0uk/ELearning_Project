using Auth.Models;

namespace ELearningProject.Models
{
    public class Progress : BaseEntity
    {
        public Guid StudentId { get; set; }
        public ApplicationUser Student { get; set; } = default!;

        public Guid TrackId { get; set; }
        public Track Track { get; set; } = default!;

        public int CompletionPercentage { get; set; }
    }
}
