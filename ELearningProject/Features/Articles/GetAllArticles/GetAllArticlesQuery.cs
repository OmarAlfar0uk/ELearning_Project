using ELearningProject.Features.Articles.DTOs;
using ELearningProject.Features.Shared;
using MediatR;

namespace ELearningProject.Features.Articles.GetAllArticles
{
    public record GetAllArticlesQuery(int Page = 1, int PageSize = 10) : IRequest<EndpointResponse<List<ArticleDto>>>;
}
