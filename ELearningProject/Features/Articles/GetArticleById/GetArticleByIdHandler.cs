using ELearningProject.Contarcts;
using ELearningProject.Features.Articles.DTOs;
using ELearningProject.Features.Shared;
using ELearningProject.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ELearningProject.Features.Articles.GetArticleById
{
    public class GetArticleByIdHandler : IRequestHandler<GetArticleByIdQuery, EndpointResponse<ArticleDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetArticleByIdHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<EndpointResponse<ArticleDto>> Handle(GetArticleByIdQuery request, CancellationToken cancellationToken)
        {
            var articleRepo = _unitOfWork.GetRepository<Article>();

            var article = await articleRepo
                .FindByCondition(a => a.Id == request.ArticleId && !a.IsDeleted)
                .Include(a => a.Author)
                .Select(a => new ArticleDto(
                    a.Id,
                    a.Title,
                    a.Content,
                    a.ImageUrl,
                    a.Author.FirstName + " " + a.Author.LastName,
                    a.CreatedAt
                ))
                .FirstOrDefaultAsync(cancellationToken);

            if (article == null)
                return EndpointResponse<ArticleDto>.NotFoundResponse("Article not found.");

            return EndpointResponse<ArticleDto>.SuccessResponse(article);
        }
    }
}
