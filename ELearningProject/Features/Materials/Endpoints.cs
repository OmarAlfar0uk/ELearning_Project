using ELearningProject.Features.Shared;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;

namespace ELearningProject.Features.Materials
{
    /// <summary>
    /// Extension class to map all endpoints related to Materials.
    /// </summary>
    public static class Endpoints
    {
        public static void MapMaterialEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v1")
                           .WithTags("Materials");

            // 1. Add Material
            group.MapPost("/lectures/{lectureId:guid}/materials", async (
                Guid lectureId,
                [FromBody] AddMaterialCommand command,
                IMediator mediator) =>
            {
                if (command.LectureId != lectureId)
                {
                    command = command with { LectureId = lectureId };
                }

                var response = await mediator.Send(command);

                return response.IsSuccess
                    ? Results.Created($"/api/v1/lectures/{lectureId}/materials", response)
                    : Results.StatusCode(response.StatusCode);
            })
            .WithName("Add Material")
            .WithSummary("Add a new study material to a lecture")
            .Produces<EndpointResponse<MaterialDto>>(201)
            .Produces<EndpointResponse<MaterialDto>>(400)
            .Produces<EndpointResponse<MaterialDto>>(404)
            .RequireAuthorization("TrackOwnership");

            // 2. Get Lecture Materials
            group.MapGet("/lectures/{lectureId:guid}/materials", async (
                Guid lectureId,
                IMediator mediator) =>
            {
                var response = await mediator.Send(new GetLectureMaterialsQuery(lectureId));

                return response.IsSuccess
                    ? Results.Ok(response)
                    : Results.StatusCode(response.StatusCode);
            })
            .WithName("Get Lecture Materials")
            .WithSummary("Get all materials associated with a lecture")
            .Produces<EndpointResponse<List<MaterialDto>>>(200)
            .Produces<EndpointResponse<List<MaterialDto>>>(404);

            // 3. Delete Material
            group.MapDelete("/materials/{materialId:guid}", async (
                Guid materialId,
                IMediator mediator) =>
            {
                var response = await mediator.Send(new DeleteMaterialCommand(materialId));

                return response.IsSuccess
                    ? Results.Ok(response)
                    : Results.StatusCode(response.StatusCode);
            })
            .WithName("Delete Material")
            .WithSummary("Soft-delete a lecture material")
            .Produces<EndpointResponse<string>>(200)
            .Produces<EndpointResponse<string>>(404)
            .RequireAuthorization("TrackOwnership");
        }
    }
}
