using ELearningProject.Features.Shared;
using MediatR;

namespace ELearningProject.Features.Articles.UpdateArticle
{
    public record UpdateArticleCommand(
        Guid ArticleId,
        string Title,
        string Content,
        string? ImageUrl
    ) : IRequest<EndpointResponse<string>>;
}
