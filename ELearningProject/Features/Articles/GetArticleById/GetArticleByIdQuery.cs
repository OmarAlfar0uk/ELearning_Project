using ELearningProject.Features.Articles.DTOs;
using ELearningProject.Features.Shared;
using MediatR;

namespace ELearningProject.Features.Articles.GetArticleById
{
    public record GetArticleByIdQuery(Guid ArticleId) : IRequest<EndpointResponse<ArticleDto>>;
}
