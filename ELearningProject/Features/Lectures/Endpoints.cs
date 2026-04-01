
using ELearningProject.Features.Lectures.CreateLecture;
using ELearningProject.Features.Lectures.DeleteLecture;
using ELearningProject.Features.Lectures.DTOs;
using ELearningProject.Features.Lectures.GetLectureDetails;
using ELearningProject.Features.Lectures.GetTrackLectures;
using ELearningProject.Features.Lectures.UpdateLecture;
using ELearningProject.Features.Shared;
using MediatR;

namespace ELearningProject.Features.Lectures
{
    public static class Endpoints
    {
        public static void MapLectureEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v1")
                           .WithTags("Lectures");

            // 1. Create Lecture
            // 1. Create Lecture
            group.MapPost("/tracks/{trackId:guid}/lectures", async (
                Guid trackId,
                HttpContext httpContext,
                IMediator mediator,
                ELearningProject.Contracts.IFileService fileService) =>
            {
                if (!httpContext.Request.HasFormContentType)
                    return Results.BadRequest(EndpointResponse<Guid>.ErrorResponse("Request must be multipart/form-data.", 400));

                var form = httpContext.Request.Form;

                var title = form["title"].FirstOrDefault();
                if (string.IsNullOrWhiteSpace(title))
                    return Results.BadRequest(EndpointResponse<Guid>.ErrorResponse("Title is required.", 400));

                var contentText = form["contentText"].FirstOrDefault();
                var driveLink   = form["driveLink"].FirstOrDefault();
                var file        = form.Files.GetFile("file");

                string? fileUrl = null;
                if (file != null && file.Length > 0)
                {
                    fileUrl = await fileService.SaveFileAsync(file, "Lectures");
                }

                var command = new CreateLectureCommand(
                    trackId,
                    title,
                    contentText,
                    driveLink,
                    fileUrl
                );

                var response = await mediator.Send(command);

                return response.IsSuccess
                    ? Results.Created($"/api/v1/lectures/{response.Data}", response)
                    : Results.StatusCode(response.StatusCode);
            })
            .DisableAntiforgery()
            .Accepts<CreateLectureRequest>("multipart/form-data")
            .WithName("Create Lecture")
            .WithSummary("Create new lecture in a track (supports file upload)")
            .Produces<EndpointResponse<Guid>>(201)
            .Produces<EndpointResponse<Guid>>(404)
            .Produces<EndpointResponse<Guid>>(409);

            // 2. Get Track Lectures
            group.MapGet("/tracks/{trackId:guid}/lectures", async (Guid trackId, IMediator mediator) =>
            {
                var response = await mediator.Send(new GetTrackLecturesQuery(trackId));

                return response.IsSuccess
                    ? Results.Ok(response)
                    : Results.StatusCode(response.StatusCode);
            })
            .WithName("Get Track Lectures")
            .WithSummary("Get all lectures for a track")
            .Produces<EndpointResponse<List<LectureDto>>>(200);

            // 3. Get Lecture Details
            group.MapGet("/lectures/{lectureId:guid}", async (Guid lectureId, IMediator mediator) =>
            {
                var response = await mediator.Send(new GetLectureDetailsQuery(lectureId));

                return response.IsSuccess
                    ? Results.Ok(response)
                    : Results.StatusCode(response.StatusCode);
            })
            .WithName("Get Lecture Details")
            .WithSummary("Get detailed info about a lecture")
            .Produces<EndpointResponse<LectureDetailsDto>>(200)
            .Produces<EndpointResponse<LectureDetailsDto>>(404);

            // 4. Update Lecture
            // 4. Update Lecture
            group.MapPut("/lectures/{lectureId:guid}", async (
                Guid lectureId,
                HttpContext httpContext,
                IMediator mediator,
                ELearningProject.Contracts.IFileService fileService) =>
            {
                if (!httpContext.Request.HasFormContentType)
                    return Results.BadRequest(EndpointResponse<Guid>.ErrorResponse("Request must be multipart/form-data.", 400));

                var form = httpContext.Request.Form;

                var title       = form["title"].FirstOrDefault();
                var contentText = form["contentText"].FirstOrDefault();
                var driveLink   = form["driveLink"].FirstOrDefault();
                var file        = form.Files.GetFile("file");

                string? fileUrl = null;
                if (file != null && file.Length > 0)
                {
                    fileUrl = await fileService.SaveFileAsync(file, "Lectures");
                }

                var command = new UpdateLectureCommand(
                    lectureId,
                    title,
                    contentText,
                    driveLink,
                    fileUrl
                );

                var response = await mediator.Send(command);

                return response.IsSuccess
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            })
            .DisableAntiforgery()
            .Accepts<UpdateLectureRequest>("multipart/form-data")
            .WithName("Update Lecture")
            .WithSummary("Update lecture details")
            .Produces<RequestResponse<string>>(200)
            .Produces<RequestResponse<string>>(400);

            // 5. Delete Lecture
            group.MapDelete("/lectures/{lectureId:guid}", async (Guid lectureId, IMediator mediator) =>
            {
                var response = await mediator.Send(new DeleteLectureCommand(lectureId));

                return response.IsSuccess
                    ? Results.NoContent()
                    : Results.BadRequest(response);
            })
            .WithName("Delete Lecture")
            .WithSummary("Delete lecture if no assignments")
            .Produces(204)
            .Produces<RequestResponse<string>>(400);
        }
    }
}
