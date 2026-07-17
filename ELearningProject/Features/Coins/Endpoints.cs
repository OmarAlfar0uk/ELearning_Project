using ELearningProject.Features.Shared;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace ELearningProject.Features.Coins
{
    /// <summary>
    /// Extension class to map all endpoints related to the Coins feature.
    /// </summary>
    public static class Endpoints
    {
        public static void MapCoinEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v1")
                           .WithTags("Coins");

            // ── POST /api/v1/students/{studentId:guid}/coins/grant ─────────────────
            // Manually grants coins to a student.
            // Auth: Admin, SuperAdmin, or Instructor.
            // NOTE: The TrackOwnership policy is intentionally NOT used here because
            // that policy resolves ownership through a trackId/lectureId/examId route
            // parameter, and this route only carries a studentId.  The broader role
            // requirement below is a temporary guard; tighter instructor-track-scoped
            // enforcement is deferred to Stage 2 (see GetStudentCoinsHandler TODO).
            group.MapPost("/students/{studentId:guid}/coins/grant", async (
                Guid studentId,
                [FromBody] GrantCoinsCommand command,
                IMediator mediator) =>
            {
                command = command with { StudentId = studentId };
                var response = await mediator.Send(command);

                return response.IsSuccess
                    ? Results.Created($"/api/v1/students/{studentId}/coins", response)
                    : Results.Json(response, statusCode: response.StatusCode);
            })
            .WithName("Grant Coins")
            .WithSummary("Manually grant coins to a student (Admin/Instructor only)")
            .Produces<EndpointResponse<CoinTransactionDto>>(201)
            .Produces<EndpointResponse<CoinTransactionDto>>(400)
            .Produces<EndpointResponse<CoinTransactionDto>>(401)
            .Produces<EndpointResponse<CoinTransactionDto>>(404)
            .RequireAuthorization(policy => policy.RequireRole("Admin", "SuperAdmin", "Instructor"));

            // ── GET /api/v1/students/{studentId:guid}/coins ───────────────────────
            // Returns the student's coin balance and paginated transaction history.
            // Auth: any authenticated user (fine-grained authorization is inside the handler).
            group.MapGet("/students/{studentId:guid}/coins", async (
                Guid studentId,
                IMediator mediator,
                [FromQuery] int page = 1,
                [FromQuery] int pageSize = 20) =>
            {
                var response = await mediator.Send(new GetStudentCoinsQuery(studentId, page, pageSize));

                return response.IsSuccess
                    ? Results.Ok(response)
                    : Results.Json(response, statusCode: response.StatusCode);
            })
            .WithName("Get Student Coins")
            .WithSummary("Get a student's coin balance and transaction history")
            .Produces<EndpointResponse<StudentCoinsDto>>(200)
            .Produces<EndpointResponse<StudentCoinsDto>>(401)
            .Produces<EndpointResponse<StudentCoinsDto>>(403)
            .Produces<EndpointResponse<StudentCoinsDto>>(404)
            .RequireAuthorization();

            // ── GET /api/v1/coins/distribution ────────────────────────────────────
            // Returns a system-wide distribution snapshot (total, top 10, recent 20).
            // Auth: Admin, SuperAdmin, or Instructor.
            group.MapGet("/coins/distribution", async (IMediator mediator) =>
            {
                var response = await mediator.Send(new GetCoinsDistributionQuery());

                return response.IsSuccess
                    ? Results.Ok(response)
                    : Results.Json(response, statusCode: response.StatusCode);
            })
            .WithName("Get Coins Distribution")
            .WithSummary("Get system-wide coin distribution statistics (Admin/Instructor only)")
            .Produces<EndpointResponse<CoinsDistributionDto>>(200)
            .Produces<EndpointResponse<CoinsDistributionDto>>(401)
            .RequireAuthorization(policy => policy.RequireRole("Admin", "SuperAdmin", "Instructor"));
        }
    }
}
