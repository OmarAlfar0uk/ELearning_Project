using Auth.Models;
using ELearningProject.Contarcts;
using ELearningProject.Features.Shared;
using ELearningProject.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace ELearningProject.Features.Articles.CreateArticle
{
    public class CreateArticleHandler : IRequestHandler<CreateArticleCommand, EndpointResponse<Guid>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public CreateArticleHandler(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<EndpointResponse<Guid>> Handle(CreateArticleCommand request, CancellationToken cancellationToken)
        {
            // 1. Validate Author exists
            var author = await _userManager.FindByIdAsync(request.AuthorId.ToString());
            if (author == null)
                return EndpointResponse<Guid>.NotFoundResponse("Author not found.");

            // 2. Create Article
            var article = new Article
            {
                Title    = request.Title,
                Content  = request.Content,
                ImageUrl = request.ImageUrl,
                AuthorId = request.AuthorId
            };

            var articleRepo = _unitOfWork.GetRepository<Article>();
            await articleRepo.CreateAsync(article);
            await _unitOfWork.SaveChangesAsync();

            return EndpointResponse<Guid>.SuccessResponse(article.Id, "Article created successfully.", 201);
        }
    }
}
