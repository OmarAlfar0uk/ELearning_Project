using Auth.Models;

namespace ELearningProject.Models
{
    /// <summary>
    /// Represents a material associated with a lecture.
    /// </summary>
    public class Material : BaseEntity
    {
        public string Name { get; set; } = default!;
        public MaterialType Type { get; set; }
        public string? FileUrl { get; set; }
        public string? ExternalLink { get; set; }

        public Guid LectureId { get; set; }
        public Lecture Lecture { get; set; } = default!;
    }
}
