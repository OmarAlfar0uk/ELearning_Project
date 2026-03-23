using Auth.Models;

namespace ELearningProject.Models
{
    public class Article : BaseEntity
    {
        public string Title { get; set; } = default!;
        public string Content { get; set; } = default!;
        public string? ImageUrl { get; set; }

        public Guid AuthorId { get; set; }
        public ApplicationUser Author { get; set; } = default!;
    }
}
