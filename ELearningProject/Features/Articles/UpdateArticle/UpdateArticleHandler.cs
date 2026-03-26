using ELearningProject.Contarcts;
using ELearningProject.Contracts;
using ELearningProject.Features.Shared;
using ELearningProject.Models;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace ELearningProject.Features.Articles.UpdateArticle
{
    public class UpdateArticleHandler : IRequestHandler<UpdateArticleCommand, EndpointResponse<string>>
    {
        private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".png", ".jpg", ".jpeg"
        };

        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileService _fileService;

        public UpdateArticleHandler(IUnitOfWork unitOfWork, IFileService fileService)
        {
            _unitOfWork = unitOfWork;
            _fileService = fileService;
        }

        public async Task<EndpointResponse<string>> Handle(UpdateArticleCommand request, CancellationToken cancellationToken)
        {
            var articleRepo = _unitOfWork.GetRepository<Article>();

            // 1. Find Article
            var article = await articleRepo.GetByIdAsync(request.ArticleId);
            if (article == null)
                return EndpointResponse<string>.NotFoundResponse("Article not found.");

            if (!IsValidImage(request.Image))
                return EndpointResponse<string>.ErrorResponse("Only PNG, JPG, and JPEG image files are allowed.", 400);

            if (request.Image is not null)
            {
                string newImageUrl;
                try
                {
                    newImageUrl = await _fileService.SaveFileAsync(request.Image, "Articles", article.AuthorId);
                }
                catch (ArgumentException ex)
                {
                    return EndpointResponse<string>.ErrorResponse(ex.Message, 400);
                }

                if (!string.IsNullOrWhiteSpace(article.ImageUrl))
                {
                    await _fileService.DeleteFileAsync(article.ImageUrl);
                }

                article.ImageUrl = newImageUrl;
            }

            // 2. Update fields
            article.Title = request.Title;
            article.Content = request.Content;
            article.UpdatedAt = DateTime.UtcNow;

            articleRepo.Update(article);
            await _unitOfWork.SaveChangesAsync();

            return EndpointResponse<string>.SuccessResponse("Article updated successfully.");
        }

        private static bool IsValidImage(IFormFile? image)
        {
            if (image is null || string.IsNullOrWhiteSpace(image.FileName))
            {
                return true;
            }

            var extension = Path.GetExtension(image.FileName);
            return AllowedImageExtensions.Contains(extension);
        }
    }
}
