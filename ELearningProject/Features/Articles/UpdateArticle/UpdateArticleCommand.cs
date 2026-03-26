using ELearningProject.Features.Shared;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace ELearningProject.Features.Articles.UpdateArticle
{
    public record UpdateArticleCommand(
        Guid ArticleId,
        string Title,
        string Content,
        IFormFile? Image
    ) : IRequest<EndpointResponse<string>>;
}
