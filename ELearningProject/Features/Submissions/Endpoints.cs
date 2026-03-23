
using ELearningProject.Features.Shared;
using ELearningProject.Features.Submissions.DTOs;
using ELearningProject.Features.Submissions.GetMySubmission;
using ELearningProject.Features.Submissions.GradeSubmission;
using ELearningProject.Features.Submissions.SubmitAssignment;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ELearningProject.Features.Submissions
{
    public static class Endpoints
    {
        public static void MapSubmissionEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v1")
                           .WithTags("Submissions");

            group.MapPost("/assignments/{assignmentId:guid}/submit", async (
                Guid assignmentId,
                HttpContext httpContext,
                IMediator mediator,
                ClaimsPrincipal user) =>
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null) return Results.Unauthorized();

                if (!httpContext.Request.HasFormContentType)
                    return Results.BadRequest(EndpointResponse<string>.ErrorResponse("Request must be multipart/form-data.", 400));

                var file = httpContext.Request.Form.Files.GetFile("file");

                if (file == null || file.Length == 0)
                {
                    return Results.BadRequest(EndpointResponse<string>.ErrorResponse("File is required.", 400));
                }

                var command = new SubmitAssignmentCommand(assignmentId, file)
                {
                    StudentId = Guid.Parse(userId)
                };

                var response = await mediator.Send(command);

                return response.IsSuccess
                    ? Results.Created($"/api/v1/assignments/{assignmentId}/my-submission", response)
                    : Results.Json(response, statusCode: response.StatusCode);

            })
            .DisableAntiforgery()
            .Accepts<SubmitAssignmentRequest>("multipart/form-data")
            .WithName("Submit Assignment")
            .WithSummary("Submit a file for an assignment (supports file upload)")
            .Produces<EndpointResponse<string>>(201)
            .Produces<EndpointResponse<string>>(400)
            .Produces<EndpointResponse<string>>(403)
            .Produces<EndpointResponse<string>>(409)
            .RequireAuthorization(policy => policy.RequireRole("Student"));


            group.MapGet("/assignments/{assignmentId:guid}/my-submission", async (
                Guid assignmentId, 
                IMediator mediator,
                ClaimsPrincipal user) =>
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null) return Results.Unauthorized();
                
                var query = new GetMySubmissionQuery(assignmentId)
                {
                    StudentId = Guid.Parse(userId)
                };

                var response = await mediator.Send(query);

                return response.IsSuccess
                    ? Results.Ok(response)
                    : Results.StatusCode(response.StatusCode);
            })
            .WithName("Get My Submission")
            .WithSummary("Get submission for current student and assignment")
            .Produces<EndpointResponse<SubmissionDto>>(200)
            .Produces<EndpointResponse<SubmissionDto>>(404)
            .RequireAuthorization(policy => policy.RequireRole("Student"));
            group.MapPost("/submissions/{submissionId:guid}/grade", async (
                Guid submissionId, 
                [FromBody] GradeSubmissionCommand command, 
                IMediator mediator) =>
            {
                if (command.SubmissionId != submissionId)
                {
                    command = command with { SubmissionId = submissionId };
                }

                var response = await mediator.Send(command);

                return response.IsSuccess
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            })
            .WithName("Grade Submission")
            .WithSummary("Grade a student submission")
            .Produces<RequestResponse<string>>(200)
            .Produces<RequestResponse<string>>(400)
            .RequireAuthorization(policy => policy.RequireRole("Admin", "SuperAdmin"));
        }
    }
}
