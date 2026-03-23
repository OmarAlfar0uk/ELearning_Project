using ELearningProject.Contarcts;
using ELearningProject.Features.Shared;
using ELearningProject.Models;
using MediatR;

namespace ELearningProject.Features.Articles.DeleteArticle
{
    public class DeleteArticleHandler : IRequestHandler<DeleteArticleCommand, EndpointResponse<string>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteArticleHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<EndpointResponse<string>> Handle(DeleteArticleCommand request, CancellationToken cancellationToken)
        {
            var articleRepo = _unitOfWork.GetRepository<Article>();

            // 1. Find Article
            var article = await articleRepo.GetByIdAsync(request.ArticleId);
            if (article == null)
                return EndpointResponse<string>.NotFoundResponse("Article not found.");

            // 2. Soft delete
            article.IsDeleted = true;
            article.UpdatedAt = DateTime.UtcNow;

            articleRepo.Update(article);
            await _unitOfWork.SaveChangesAsync();

            return EndpointResponse<string>.SuccessResponse("Article deleted successfully.");
        }
    }
}
