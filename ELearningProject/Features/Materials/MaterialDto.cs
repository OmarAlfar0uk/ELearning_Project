using ELearningProject.Models;

namespace ELearningProject.Features.Materials
{
    /// <summary>
    /// Data Transfer Object representing a lecture material.
    /// </summary>
    public class MaterialDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string Type { get; set; } = default!;
        public string? FileUrl { get; set; }
        public string? ExternalLink { get; set; }
        public Guid LectureId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
