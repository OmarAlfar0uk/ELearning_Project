using ELearningProject.Features.Articles.DTOs;
using ELearningProject.Features.Shared;
using MediatR;

namespace ELearningProject.Features.Articles.CreateArticle
{
    public record CreateArticleCommand(
        string Title,
        string Content,
        string? ImageUrl,
        Guid AuthorId
    ) : IRequest<EndpointResponse<Guid>>;
}
