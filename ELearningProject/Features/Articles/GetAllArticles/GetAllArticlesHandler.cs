using ELearningProject.Contarcts;
using ELearningProject.Features.Articles.DTOs;
using ELearningProject.Features.Shared;
using ELearningProject.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ELearningProject.Features.Articles.GetAllArticles
{
    public class GetAllArticlesHandler : IRequestHandler<GetAllArticlesQuery, EndpointResponse<List<ArticleDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetAllArticlesHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<EndpointResponse<List<ArticleDto>>> Handle(GetAllArticlesQuery request, CancellationToken cancellationToken)
        {
            var articleRepo = _unitOfWork.GetRepository<Article>();

            var page     = request.Page < 1 ? 1 : request.Page;
            var pageSize = request.PageSize < 1 ? 10 : request.PageSize;

            var articles = await articleRepo
                .FindByCondition(a => !a.IsDeleted)
                .Include(a => a.Author)
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new ArticleDto(
                    a.Id,
                    a.Title,
                    a.Content,
                    a.ImageUrl,
                    a.Author.FirstName + " " + a.Author.LastName,
                    a.CreatedAt
                ))
                .ToListAsync(cancellationToken);

            return EndpointResponse<List<ArticleDto>>.SuccessResponse(articles);
        }
    }
}
