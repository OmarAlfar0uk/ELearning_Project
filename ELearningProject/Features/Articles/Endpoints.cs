using ELearningProject.Features.Articles.CreateArticle;
using ELearningProject.Features.Articles.DeleteArticle;
using ELearningProject.Features.Articles.DTOs;
using ELearningProject.Features.Articles.GetAllArticles;
using ELearningProject.Features.Articles.GetArticleById;
using ELearningProject.Features.Articles.UpdateArticle;
using ELearningProject.Features.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ELearningProject.Features.Articles
{
    public static class Endpoints
    {
        public static void MapArticleEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v1/articles")
                           .WithTags("Articles");

            // 1. Create Article – Admin only
            group.MapPost("/", async (
                [FromForm] string title,
                [FromForm] string content,
                IFormFile? image,
                IMediator mediator,
                ClaimsPrincipal user) =>
            {
                var authorId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var cmd = new CreateArticleCommand(
                    title,
                    content,
                    image,
                    authorId);

                var response = await mediator.Send(cmd);

                return response.IsSuccess
                    ? Results.Created($"/api/v1/articles/{response.Data}", response)
                    : Results.StatusCode(response.StatusCode);
            })
            .DisableAntiforgery()
            .Accepts<CreateArticleRequest>("multipart/form-data")
            .WithName("Create Article")
            .WithSummary("Create a new article (Admin only)")
            .Produces<EndpointResponse<Guid>>(201)
            .Produces<EndpointResponse<Guid>>(404)
            .RequireAuthorization(policy => policy.RequireRole("Admin", "SuperAdmin"));

            // 2. Update Article – Admin only
            group.MapPut("/{articleId:guid}", async (
                Guid articleId,
                [FromForm] string title,
                [FromForm] string content,
                IFormFile? image,
                IMediator mediator) =>
            {
                var cmd = new UpdateArticleCommand(
                    articleId,
                    title,
                    content,
                    image);
                var response = await mediator.Send(cmd);

                return response.IsSuccess
                    ? Results.Ok(response)
                    : Results.StatusCode(response.StatusCode);
            })
            .DisableAntiforgery()
            .Accepts<UpdateArticleRequest>("multipart/form-data")
            .WithName("Update Article")
            .WithSummary("Update an existing article (Admin only)")
            .Produces<EndpointResponse<string>>(200)
            .Produces<EndpointResponse<string>>(404)
            .RequireAuthorization(policy => policy.RequireRole("Admin", "SuperAdmin"));

            // 3. Delete Article – Admin only
            group.MapDelete("/{articleId:guid}", async (
                Guid articleId,
                IMediator mediator) =>
            {
                var response = await mediator.Send(new DeleteArticleCommand(articleId));

                return response.IsSuccess
                    ? Results.Ok(response)
                    : Results.StatusCode(response.StatusCode);
            })
            .WithName("Delete Article")
            .WithSummary("Soft-delete an article (Admin only)")
            .Produces<EndpointResponse<string>>(200)
            .Produces<EndpointResponse<string>>(404)
            .RequireAuthorization(policy => policy.RequireRole("Admin", "SuperAdmin"));

            // 4. Get All Articles – Public
            group.MapGet("/", async (
                [FromQuery] int page = 1,
                [FromQuery] int pageSize = 10,
                IMediator mediator = null!) =>
            {
                var response = await mediator.Send(new GetAllArticlesQuery(page, pageSize));

                return response.IsSuccess
                    ? Results.Ok(response)
                    : Results.StatusCode(response.StatusCode);
            })
            .WithName("Get All Articles")
            .WithSummary("Get list of all published articles (public)")
            .Produces<EndpointResponse<List<ArticleDto>>>(200);

            // 5. Get Article By Id – Public
            group.MapGet("/{articleId:guid}", async (
                Guid articleId,
                IMediator mediator) =>
            {
                var response = await mediator.Send(new GetArticleByIdQuery(articleId));

                return response.IsSuccess
                    ? Results.Ok(response)
                    : Results.StatusCode(response.StatusCode);
            })
            .WithName("Get Article By Id")
            .WithSummary("Get a single article by ID (public)")
            .Produces<EndpointResponse<ArticleDto>>(200)
            .Produces<EndpointResponse<ArticleDto>>(404);
        }
    }
}
