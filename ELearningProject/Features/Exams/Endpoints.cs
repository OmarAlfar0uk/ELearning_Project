using ELearningProject.Features.Shared;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace ELearningProject.Features.Exams
{
    /// <summary>
    /// Extension class to map all endpoints related to Exams.
    /// </summary>
    public static class Endpoints
    {
        public static void MapExamEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v1")
                           .WithTags("Exams");

            group.MapPost("/tracks/{trackId:guid}/exams", async (
                Guid trackId,
                [FromBody] CreateExamCommand command,
                IMediator mediator) =>
            {
                command = command with { TrackId = trackId };
                var response = await mediator.Send(command);

                return response.IsSuccess
                    ? Results.Created($"/api/v1/exams/{response.Data?.Id}", response)
                    : Results.Json(response, statusCode: response.StatusCode);
            })
            .WithName("Create Exam")
            .WithSummary("Create a new exam for a track")
            .Produces<EndpointResponse<ExamDto>>(201)
            .Produces<EndpointResponse<ExamDto>>(400)
            .Produces<EndpointResponse<ExamDto>>(404)
            .RequireAuthorization("TrackOwnership");

            group.MapGet("/tracks/{trackId:guid}/exams", async (
                Guid trackId,
                IMediator mediator) =>
            {
                var response = await mediator.Send(new GetTrackExamsQuery(trackId));

                return response.IsSuccess
                    ? Results.Ok(response)
                    : Results.Json(response, statusCode: response.StatusCode);
            })
            .WithName("Get Track Exams")
            .WithSummary("Get exam summaries for a track")
            .Produces<EndpointResponse<List<ExamDto>>>(200)
            .Produces<EndpointResponse<List<ExamDto>>>(404)
            .RequireAuthorization();

            group.MapGet("/exams/{examId:guid}", async (
                Guid examId,
                IMediator mediator) =>
            {
                var response = await mediator.Send(new GetExamDetailsQuery(examId));

                return response.IsSuccess
                    ? Results.Ok(response)
                    : Results.Json(response, statusCode: response.StatusCode);
            })
            .WithName("Get Exam Details")
            .WithSummary("Get an exam with its questions and options")
            .Produces<EndpointResponse<ExamDetailsDto>>(200)
            .Produces<EndpointResponse<ExamDetailsDto>>(404)
            .RequireAuthorization();

            group.MapPut("/exams/{examId:guid}", async (
                Guid examId,
                [FromBody] UpdateExamCommand command,
                IMediator mediator) =>
            {
                command = command with { ExamId = examId };
                var response = await mediator.Send(command);

                return response.IsSuccess
                    ? Results.Ok(response)
                    : Results.Json(response, statusCode: response.StatusCode);
            })
            .WithName("Update Exam")
            .WithSummary("Update an open exam")
            .Produces<EndpointResponse<ExamDto>>(200)
            .Produces<EndpointResponse<ExamDto>>(400)
            .Produces<EndpointResponse<ExamDto>>(404)
            .RequireAuthorization("TrackOwnership");

            group.MapPost("/exams/{examId:guid}/questions", async (
                Guid examId,
                [FromBody] AddExamQuestionCommand command,
                IMediator mediator) =>
            {
                command = command with { ExamId = examId };
                var response = await mediator.Send(command);

                return response.IsSuccess
                    ? Results.Created($"/api/v1/exams/{examId}", response)
                    : Results.Json(response, statusCode: response.StatusCode);
            })
            .WithName("Add Exam Question")
            .WithSummary("Add a question to an open exam")
            .Produces<EndpointResponse<ExamQuestionDto>>(201)
            .Produces<EndpointResponse<ExamQuestionDto>>(400)
            .Produces<EndpointResponse<ExamQuestionDto>>(404)
            .RequireAuthorization("TrackOwnership");

            group.MapPost("/exams/{examId:guid}/close", async (
                Guid examId,
                IMediator mediator) =>
            {
                var response = await mediator.Send(new CloseExamCommand(examId));

                return response.IsSuccess
                    ? Results.Ok(response)
                    : Results.Json(response, statusCode: response.StatusCode);
            })
            .WithName("Close Exam")
            .WithSummary("Close an exam to prevent further authoring changes")
            .Produces<EndpointResponse<string>>(200)
            .Produces<EndpointResponse<string>>(400)
            .Produces<EndpointResponse<string>>(404)
            .RequireAuthorization("TrackOwnership");
        }
    }
}
