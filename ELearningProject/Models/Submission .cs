using Auth.Models;

namespace ELearningProject.Models
{
    public class Submission : BaseEntity
    {
        public Guid AssignmentId { get; set; }
        public Assignment Assignment { get; set; } = default!;

        public Guid StudentId { get; set; }
        public ApplicationUser Student { get; set; } = default!;

        public string FileUrl { get; set; } = default!;

        public double? Score { get; set; }
        public string? Feedback { get; set; }

        public bool IsFinalized { get; set; } = false;
    }
}
