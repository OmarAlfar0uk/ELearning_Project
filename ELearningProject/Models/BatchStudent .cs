using Auth.Models;

namespace ELearningProject.Models
{
    public class BatchStudent : BaseEntity
    {
        public Guid StudentId { get; set; }
        public ApplicationUser Student { get; set; } = default!;

        public Guid BatchId { get; set; }
        public Batch Batch { get; set; } = default!;

        public int Rank { get; set; }
        public double AverageScore { get; set; }
    }
}
