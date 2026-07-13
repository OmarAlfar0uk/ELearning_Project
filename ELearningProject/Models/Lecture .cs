using Auth.Models;
using Serilog.Parsing;

namespace ELearningProject.Models
{
    public class Lecture : BaseEntity
    {
        public string Title { get; set; } = default!;
        public string? ContentText { get; set; }
        public string? DriveLink { get; set; }
        public string? FileUrl { get; set; }

        public Guid TrackId { get; set; }
        public Track Track { get; set; } = default!;
        public ICollection<Assignment> Assignments { get; set; } = new HashSet<Assignment>();
        public ICollection<Material> Materials { get; set; } = new HashSet<Material>();
    }
}
