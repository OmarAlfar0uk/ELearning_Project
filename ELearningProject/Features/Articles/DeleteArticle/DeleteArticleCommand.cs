using ELearningProject.Features.Shared;
using MediatR;

namespace ELearningProject.Features.Articles.DeleteArticle
{
    public record DeleteArticleCommand(Guid ArticleId) : IRequest<EndpointResponse<string>>;
}
