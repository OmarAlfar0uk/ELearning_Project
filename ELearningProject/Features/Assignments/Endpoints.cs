
using ELearningProject.Features.Assignments.CloseAssignment;
using ELearningProject.Features.Assignments.CreateAssignment;
using ELearningProject.Features.Assignments.DeleteAssignment;
using ELearningProject.Features.Assignments.DTOs;
using ELearningProject.Features.Assignments.GetAssignmentDetails;
using ELearningProject.Features.Assignments.GetAssignmentStats;
using ELearningProject.Features.Assignments.GetAssignmentSubmissions;
using ELearningProject.Features.Assignments.GetLectureAssignments;
using ELearningProject.Features.Assignments.UpdateAssignment;
using ELearningProject.Features.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ELearningProject.Features.Assignments
{
    public static class Endpoints
    {
        public static void MapAssignmentEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v1")
                           .WithTags("Assignments");

            // 1. Create Assignment
            group.MapPost("/lectures/{lectureId:guid}/assignments", async (
                Guid lectureId, 
                [FromBody] CreateAssignmentCommand command, 
                IMediator mediator) =>
            {
                if (command.LectureId != lectureId)
                {
                    command = command with { LectureId = lectureId };
                }

                var response = await mediator.Send(command);

                return response.IsSuccess
                    ? Results.Created($"/api/v1/assignments/{response.Data}", response)
                    : Results.StatusCode(response.StatusCode);
            })
            .WithName("Create Assignment")
            .WithSummary("Create new assignment for a lecture")
            .Produces<EndpointResponse<Guid>>(201)
            .Produces<EndpointResponse<Guid>>(404);

            // 2. Get Lecture Assignments
            group.MapGet("/lectures/{lectureId:guid}/assignments", async (Guid lectureId, IMediator mediator) =>
            {
                var response = await mediator.Send(new GetLectureAssignmentsQuery(lectureId));

                return response.IsSuccess
                    ? Results.Ok(response)
                    : Results.StatusCode(response.StatusCode);
            })
            .WithName("Get Lecture Assignments")
            .WithSummary("Get all assignments for a lecture")
            .Produces<EndpointResponse<List<AssignmentDto>>>(200);

            // 3. Get Assignment Details
            group.MapGet("/assignments/{assignmentId:guid}", async (Guid assignmentId, IMediator mediator) =>
            {
                var response = await mediator.Send(new GetAssignmentDetailsQuery(assignmentId));

                return response.IsSuccess
                    ? Results.Ok(response)
                    : Results.StatusCode(response.StatusCode);
            })
            .WithName("Get Assignment Details")
            .WithSummary("Get detailed info about an assignment")
            .Produces<EndpointResponse<AssignmentDetailsDto>>(200)
            .Produces<EndpointResponse<AssignmentDetailsDto>>(404);

            // 4. Update Assignment
            group.MapPut("/assignments/{assignmentId:guid}", async (
                Guid assignmentId, 
                [FromBody] UpdateAssignmentCommand command, 
                IMediator mediator) =>
            {
                if (command.AssignmentId != assignmentId)
                {
                    command = command with { AssignmentId = assignmentId };
                }

                var response = await mediator.Send(command);

                return response.IsSuccess
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            })
            .WithName("Update Assignment")
            .WithSummary("Update assignment details")
            .Produces<RequestResponse<string>>(200)
            .Produces<RequestResponse<string>>(400);

            // 5. Delete Assignment
            group.MapDelete("/assignments/{assignmentId:guid}", async (Guid assignmentId, IMediator mediator) =>
            {
                var response = await mediator.Send(new DeleteAssignmentCommand(assignmentId));

                return response.IsSuccess
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            })
            .WithName("Delete Assignment")
            .WithSummary("Delete assignment if no submissions")
            .Produces<RequestResponse<string>>(200)
            .Produces<RequestResponse<string>>(400);

            // 6. Get Assignment Submissions (Admin/SuperAdmin)
            group.MapGet("/assignments/{assignmentId:guid}/submissions", async (Guid assignmentId, IMediator mediator) =>
            {
                var response = await mediator.Send(new GetAssignmentSubmissionsQuery(assignmentId));

                return response.IsSuccess
                    ? Results.Ok(response)
                    : Results.StatusCode(response.StatusCode);
            })
            .WithName("Get Assignment Submissions")
            .WithSummary("Get list of student submissions for an assignment")
            .Produces<EndpointResponse<List<AssignmentSubmissionDto>>>(200)
            .RequireAuthorization(policy => policy.RequireRole("Admin", "SuperAdmin"));

            // 7. Get Assignment Stats
            group.MapGet("/assignments/{assignmentId:guid}/stats", async (Guid assignmentId, IMediator mediator) =>
            {
                var response = await mediator.Send(new GetAssignmentStatsQuery(assignmentId));

                return response.IsSuccess
                    ? Results.Ok(response)
                    : Results.StatusCode(response.StatusCode);
            })
            .WithName("Get Assignment Statistics")
            .WithSummary("Get comparison stats for an assignment")
            .Produces<EndpointResponse<AssignmentStatsDto>>(200)
            .Produces<EndpointResponse<AssignmentStatsDto>>(404);

            // 8. Close Assignment
            group.MapPost("/assignments/{assignmentId:guid}/close", async (Guid assignmentId, IMediator mediator) =>
            {
                var response = await mediator.Send(new CloseAssignmentCommand(assignmentId));

                return response.IsSuccess
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            })
            .WithName("Close Assignment")
            .WithSummary("Close assignment to prevent further submissions")
            .Produces<RequestResponse<string>>(200)
            .Produces<RequestResponse<string>>(400);
        }
    }
}
