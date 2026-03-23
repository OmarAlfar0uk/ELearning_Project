
using ELearningProject.Features.Progress.DTOs;
using ELearningProject.Features.Progress.GetMyProgress;
using ELearningProject.Features.Progress.GetStudentProgress;
using ELearningProject.Features.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ELearningProject.Features.Progress
{
    public static class Endpoints
    {
        public static void MapProgressEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v1")
                           .WithTags("Progress");

            // 1. Get My Progress
            group.MapGet("/tracks/{trackId:guid}/progress", async (
                Guid trackId, 
                IMediator mediator,
                ClaimsPrincipal user) =>
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null) return Results.Unauthorized();

                var userRole = user.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

                var query = new GetMyProgressQuery(trackId)
                {
                    StudentId = Guid.Parse(userId),
                    UserRole = userRole
                };

                var response = await mediator.Send(query);

                return response.IsSuccess
                    ? Results.Ok(response)
                    : Results.StatusCode(response.StatusCode);
            })
            .WithName("Get My Progress")
            .WithSummary("Get progress for current student in a track")
            .Produces<EndpointResponse<ProgressDto>>(200)
            .Produces<EndpointResponse<ProgressDto>>(403)
            .Produces<EndpointResponse<ProgressDto>>(404)
            .RequireAuthorization(policy => policy.RequireRole("Student", "Admin", "SuperAdmin"));

            // 2. Get Student Progress (Admin/SuperAdmin)
            group.MapGet("/tracks/{trackId:guid}/students/{studentId:guid}/progress", async (
                Guid trackId, 
                Guid studentId,
                IMediator mediator) =>
            {
                var query = new GetStudentProgressQuery(trackId, studentId);
                var response = await mediator.Send(query);

                return response.IsSuccess
                    ? Results.Ok(response)
                    : Results.StatusCode(response.StatusCode);
            })
            .WithName("Get Student Progress")
            .WithSummary("Get progress for a specific student in a track (Admin/SuperAdmin)")
            .Produces<EndpointResponse<ProgressDto>>(200)
            .Produces<EndpointResponse<ProgressDto>>(404)
            .RequireAuthorization(policy => policy.RequireRole("Admin", "SuperAdmin"));
        }
    }
}
