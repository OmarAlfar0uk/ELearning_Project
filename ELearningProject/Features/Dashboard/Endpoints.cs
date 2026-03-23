
using ELearningProject.Features.Dashboard.DTOs;
using ELearningProject.Features.Dashboard.Queries.GetAdminDashboard;
using ELearningProject.Features.Dashboard.Queries.GetInstructorDashboard;
using ELearningProject.Features.Dashboard.Queries.GetStudentDashboard;
using ELearningProject.Features.Shared;
using MediatR;
using System.Security.Claims;

namespace ELearningProject.Features.Dashboard
{
    public static class Endpoints
    {
        public static void MapDashboardEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v1/dashboard")
                           .WithTags("Dashboard");

            // 1. Admin Dashboard
            group.MapGet("/admin", async (IMediator mediator) =>
            {
                var response = await mediator.Send(new GetAdminDashboardQuery());
                return response.IsSuccess ? Results.Ok(response) : Results.StatusCode(response.StatusCode);
            })
            .WithName("Get Admin Dashboard")
            .Produces<EndpointResponse<AdminDashboardDto>>(200)
            .RequireAuthorization(policy => policy.RequireRole("Admin", "SuperAdmin"));

            // 2. Instructor Dashboard
            group.MapGet("/instructor", async (IMediator mediator, ClaimsPrincipal user) =>
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null) return Results.Unauthorized();

                var response = await mediator.Send(new GetInstructorDashboardQuery(Guid.Parse(userId)));
                return response.IsSuccess ? Results.Ok(response) : Results.StatusCode(response.StatusCode);
            })
            .WithName("Get Instructor Dashboard")
            .Produces<EndpointResponse<InstructorDashboardDto>>(200)
            .RequireAuthorization(policy => policy.RequireRole( "Admin", "SuperAdmin"));

            // 3. Student Dashboard
            group.MapGet("/student", async (IMediator mediator, ClaimsPrincipal user) =>
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null) return Results.Unauthorized();

                var response = await mediator.Send(new GetStudentDashboardQuery(Guid.Parse(userId)));
                return response.IsSuccess ? Results.Ok(response) : Results.StatusCode(response.StatusCode);
            })
            .WithName("Get Student Dashboard")
            .Produces<EndpointResponse<StudentDashboardDto>>(200)
            .RequireAuthorization(policy => policy.RequireRole("Student"));
        }
    }
}
