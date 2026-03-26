using Auth.Models;
using ELearningProject.Contarcts;
using ELearningProject.Contracts;
using ELearningProject.Features.Shared;
using ELearningProject.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace ELearningProject.Features.Articles.CreateArticle
{
    public class CreateArticleHandler : IRequestHandler<CreateArticleCommand, EndpointResponse<Guid>>
    {
        private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".png", ".jpg", ".jpeg"
        };

        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IFileService _fileService;

        public CreateArticleHandler(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            IFileService fileService)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _fileService = fileService;
        }

        public async Task<EndpointResponse<Guid>> Handle(CreateArticleCommand request, CancellationToken cancellationToken)
        {
            // 1. Validate Author exists
            var author = await _userManager.FindByIdAsync(request.AuthorId.ToString());
            if (author == null)
                return EndpointResponse<Guid>.NotFoundResponse("Author not found.");

            if (!IsValidImage(request.Image))
                return EndpointResponse<Guid>.ErrorResponse("Only PNG, JPG, and JPEG image files are allowed.", 400);

            string? imageUrl;
            try
            {
                imageUrl = request.Image is null
                    ? null
                    : await _fileService.SaveFileAsync(request.Image, "Articles", request.AuthorId);
            }
            catch (ArgumentException ex)
            {
                return EndpointResponse<Guid>.ErrorResponse(ex.Message, 400);
            }

            // 2. Create Article
            var article = new Article
            {
                Title = request.Title,
                Content = request.Content,
                ImageUrl = imageUrl,
                AuthorId = request.AuthorId
            };

            var articleRepo = _unitOfWork.GetRepository<Article>();
            await articleRepo.CreateAsync(article);
            await _unitOfWork.SaveChangesAsync();

            return EndpointResponse<Guid>.SuccessResponse(article.Id, "Article created successfully.", 201);
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
