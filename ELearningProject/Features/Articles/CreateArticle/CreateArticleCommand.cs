using ELearningProject.Features.Shared;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace ELearningProject.Features.Articles.CreateArticle
{
    public record CreateArticleCommand(
        string Title,
        string Content,
        IFormFile? Image,
        Guid AuthorId
    ) : IRequest<EndpointResponse<Guid>>;
}
