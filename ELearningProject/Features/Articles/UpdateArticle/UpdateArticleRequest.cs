using Microsoft.AspNetCore.Http;

namespace ELearningProject.Features.Articles.UpdateArticle
{
    public sealed class UpdateArticleRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public IFormFile? Image { get; set; }
    }
}
