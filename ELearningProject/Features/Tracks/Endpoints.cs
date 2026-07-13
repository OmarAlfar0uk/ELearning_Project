
using ELearningProject.Features.Shared;
using ELearningProject.Features.Tracks.CreateTrack;
using ELearningProject.Features.Tracks.DeleteTrack;
using ELearningProject.Features.Tracks.DTOs;
using ELearningProject.Features.Tracks.GetBatchTracks;
using ELearningProject.Features.Tracks.GetTrackDetails;
using ELearningProject.Features.Tracks.UpdateTrack;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ELearningProject.Features.Tracks
{
    public static class Endpoints
    {
        public static void MapTrackEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v1")
                           .WithTags("Tracks");

            // 1. Create Track
            group.MapPost("/batches/{batchId:guid}/tracks", async (
                Guid batchId, 
                [FromBody] CreateTrackCommand command, 
                IMediator mediator) =>
            {
                if (command.BatchId != batchId)
                {
                    command = command with { BatchId = batchId };
                }

                var response = await mediator.Send(command);

                return response.IsSuccess
                    ? Results.Created($"/api/v1/tracks/{response.Data}", response)
                    : Results.StatusCode(response.StatusCode);
            })
            .WithName("Create Track")
            .WithSummary("Create new track in a batch")
            .Produces<EndpointResponse<Guid>>(201)
            .Produces<EndpointResponse<Guid>>(404)
            .Produces<EndpointResponse<Guid>>(409)
            .RequireAuthorization(policy => policy.RequireRole("Admin", "SuperAdmin"));

            // 2. Get Batch Tracks
            group.MapGet("/batches/{batchId:guid}/tracks", async (Guid batchId, IMediator mediator) =>
            {
                var response = await mediator.Send(new GetBatchTracksQuery(batchId));

                return response.IsSuccess
                    ? Results.Ok(response)
                    : Results.StatusCode(response.StatusCode);
            })
            .WithName("Get Batch Tracks")
            .WithSummary("Get all tracks for a batch")
            .Produces<EndpointResponse<List<TrackDto>>>(200)
            .RequireAuthorization();

            // 3. Get Track Details
            group.MapGet("/tracks/{trackId:guid}", async (Guid trackId, IMediator mediator) =>
            {
                var response = await mediator.Send(new GetTrackDetailsQuery(trackId));

                return response.IsSuccess
                    ? Results.Ok(response)
                    : Results.StatusCode(response.StatusCode);
            })
            .WithName("Get Track Details")
            .WithSummary("Get detailed info about a track")
            .Produces<EndpointResponse<TrackDetailsDto>>(200)
            .Produces<EndpointResponse<TrackDetailsDto>>(404)
            .RequireAuthorization("TrackOwnership");

            // 4. Update Track
            group.MapPut("/tracks/{trackId:guid}", async (
                Guid trackId, 
                [FromBody] UpdateTrackCommand command, 
                IMediator mediator) =>
            {
                if (command.TrackId != trackId)
                {
                    command = command with { TrackId = trackId };
                }

                var response = await mediator.Send(command);

                return response.IsSuccess
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            })
            .WithName("Update Track")
            .WithSummary("Update track name")
            .Produces<RequestResponse<string>>(200)
            .Produces<RequestResponse<string>>(400)
            .RequireAuthorization(policy => policy.RequireRole("Admin", "SuperAdmin"));

            // 5. Delete Track
            group.MapDelete("/tracks/{trackId:guid}", async (Guid trackId, IMediator mediator) =>
            {
                var response = await mediator.Send(new DeleteTrackCommand(trackId));

                return response.IsSuccess
                    ? Results.NoContent()
                    : Results.BadRequest(response);
            })
            .WithName("Delete Track")
            .WithSummary("Delete track if no lectures")
            .Produces(204)
            .Produces<RequestResponse<string>>(400)
            .RequireAuthorization(policy => policy.RequireRole("Admin", "SuperAdmin"));
        }
    }
}
