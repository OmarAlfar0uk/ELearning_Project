using ELearningProject.Contarcts;
using ELearningProject.Features.Shared;
using ELearningProject.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ELearningProject.Features.Articles.UpdateArticle
{
    public class UpdateArticleHandler : IRequestHandler<UpdateArticleCommand, EndpointResponse<string>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateArticleHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<EndpointResponse<string>> Handle(UpdateArticleCommand request, CancellationToken cancellationToken)
        {
            var articleRepo = _unitOfWork.GetRepository<Article>();

            // 1. Find Article
            var article = await articleRepo.GetByIdAsync(request.ArticleId);
            if (article == null)
                return EndpointResponse<string>.NotFoundResponse("Article not found.");

            // 2. Update fields
            article.Title     = request.Title;
            article.Content   = request.Content;
            article.ImageUrl  = request.ImageUrl;
            article.UpdatedAt = DateTime.UtcNow;

            articleRepo.Update(article);
            await _unitOfWork.SaveChangesAsync();

            return EndpointResponse<string>.SuccessResponse("Article updated successfully.");
        }
    }
}
