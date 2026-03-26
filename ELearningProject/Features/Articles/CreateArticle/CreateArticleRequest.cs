using Microsoft.AspNetCore.Http;

namespace ELearningProject.Features.Articles.CreateArticle
{
    public sealed class CreateArticleRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public IFormFile? Image { get; set; }
    }
}
