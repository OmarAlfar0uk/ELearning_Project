using ELearningProject.Features.Batche.AssignTrackToBatch;
using ELearningProject.Features.Batche.CreateBatch;
using ELearningProject.Features.Batche.DeleteBatch;
using ELearningProject.Features.Batche.GetAllBatches;
using ELearningProject.Features.Batche.GetBatchById;
using ELearningProject.Features.Batche.GetBatchStudentStats;
using ELearningProject.Features.Batche.GetStudentsInBatch;
using ELearningProject.Features.Batche.GetTopBatchesByScore;
using ELearningProject.Features.Batche.RemoveStudentFromBatch;
using ELearningProject.Features.Batche.UpdateBatch;
using ELearningProject.Features.Shared;
using ELearningProject.Contarcts;
using ELearningProject.Models;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace ELearningProject.Features.Batche
{
    public static class Endpoints
    {
        public static void MapBatchEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v1/batches")
                           .WithTags("Batches");

            // 🔹 Create Batch
            group.MapPost("/", async (CreateBatchCommand command, IMediator mediator) =>
            {
                var response = await mediator.Send(command);

                return response.IsSuccess
                    ? Results.Created($"/api/v1/batches/{response.Data}", response)
                    : Results.StatusCode(response.StatusCode);
            })
            .WithName("Create Batch")
            .WithSummary("Create new batch")
            .Produces<EndpointResponse<Guid>>(201)
            .Produces<EndpointResponse<Guid>>(400);

            // 🔹 Update Batch
            group.MapPut("/", async (UpdateBatchCommand command, IMediator mediator) =>
            {
                var response = await mediator.Send(command);

                return response.IsSuccess
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            })
            .WithName("Update Batch")
            .WithSummary("Update existing batch")
            .Produces<RequestResponse<string>>(200)
            .Produces<RequestResponse<string>>(400);

            // 🔹 Delete Batch
            group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator) =>
            {
                var response = await mediator.Send(new DeleteBatchCommand(id));

                return response.IsSuccess
                    ? Results.NoContent()
                    : Results.BadRequest(response);
            })
            .WithName("Delete Batch")
            .WithSummary("Delete batch by id")
            .Produces(204)
            .Produces<RequestResponse<string>>(400);

            // 🔹 Get By Id
            group.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
            {
                var response = await mediator.Send(new GetBatchByIdQuery(id));

                return response.IsSuccess
                    ? Results.Ok(response)
                    : Results.StatusCode(response.StatusCode);
            })
            .WithName("Get Batch By Id")
            .WithSummary("Get batch details including students and tracks")
            .Produces<EndpointResponse<BatchDetailsDto>>(200)
            .Produces<EndpointResponse<BatchDetailsDto>>(404);

            // 🔹 Get All (Paginated + Search)
            group.MapGet("/", async ([AsParameters] GetAllBatchesQuery query, IMediator mediator) =>
            {
                var response = await mediator.Send(query);

                return response.IsSuccess
                    ? Results.Ok(response)
                    : Results.StatusCode(response.StatusCode);
            })
            .WithName("Get All Batches")
            .WithSummary("Get paginated list of batches with search")
            .Produces<EndpointResponse<PaginatedResult<BatchDto>>>(200);

            // 🔹 Top batches by average score
            group.MapGet("/leaderboard", async (int? limit, IMediator mediator) =>
            {
                var normalizedLimit = limit is > 0 ? limit.Value : 5;
                var response = await mediator.Send(new GetTopBatchesByScoreQuery(normalizedLimit));

                return response.IsSuccess
                    ? Results.Ok(response)
                    : Results.StatusCode(response.StatusCode);
            })
            .WithName("Get Top Batches By Score")
            .WithSummary("Get top batches ordered by average student score")
            .Produces<EndpointResponse<List<TopBatchByScoreDto>>>(200);

            // 🔹 Get Students In Batch
            group.MapGet("/{batchId:guid}/students", async (Guid batchId, IMediator mediator) =>
            {
                var response = await mediator.Send(new GetBatchStudentsQuery(batchId));

                return response.IsSuccess
                    ? Results.Ok(response)
                    : Results.StatusCode(response.StatusCode);
            })
            .WithName("Get Students In Batch")
            .WithSummary("Get list of students assigned to a batch")
            .Produces<EndpointResponse<List<BatchStudentItemDto>>>(200)
            .Produces<EndpointResponse<List<BatchStudentItemDto>>>(404);

            // 🔹 Get Top Students In Batch
            group.MapGet("/{batchId:guid}/top-students", async (Guid batchId, int? limit, IMediator mediator) =>
            {
                var response = await mediator.Send(new GetBatchStudentsQuery(batchId));
                if (!response.IsSuccess)
                    return Results.Json(response, statusCode: response.StatusCode);

                var normalizedLimit = Math.Clamp(limit is > 0 ? limit.Value : 5, 1, 50);
                var topStudents = response.Data?.Take(normalizedLimit).ToList() ?? new List<BatchStudentItemDto>();

                return Results.Ok(EndpointResponse<List<BatchStudentItemDto>>.SuccessResponse(
                    topStudents,
                    "Top students retrieved successfully."));
            })
            .WithName("Get Top Students In Batch")
            .WithSummary("Get top students in a batch with their rank")
            .Produces<EndpointResponse<List<BatchStudentItemDto>>>(200)
            .Produces<EndpointResponse<List<BatchStudentItemDto>>>(404);

            // 🔹 Get Batch Student Stats
            group.MapGet("/{batchId:guid}/students/{studentId:guid}/stats", async (
                Guid batchId,
                Guid studentId,
                IMediator mediator) =>
            {
                var response = await mediator.Send(new GetBatchStudentStatsQuery(batchId, studentId));

                return response.IsSuccess
                    ? Results.Ok(response)
                    : Results.Json(response, statusCode: response.StatusCode);
            })
            .WithName("Get Batch Student Stats")
            .WithSummary("Get rank, average score, and submissions count for a student in a batch")
            .Produces<EndpointResponse<BatchStudentStatsDto>>(200)
            .Produces<EndpointResponse<BatchStudentStatsDto>>(404);

            // 🔹 Remove Student From Batch
            group.MapDelete("/{batchId:guid}/students/{studentId:guid}", async (Guid batchId, Guid studentId, IMediator mediator) =>
            {
                var response = await mediator.Send(new RemoveStudentFromBatchCommand(batchId, studentId));

                return response.IsSuccess
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            })
            .WithName("Remove Student From Batch")
            .WithSummary("Remove a student from a batch")
            .Produces<RequestResponse<string>>(200)
            .Produces<RequestResponse<string>>(400);

            // 🔹 Assign Track To Batch
            group.MapPost("/{batchId:guid}/tracks/{trackId:guid}", async (Guid batchId, Guid trackId, IMediator mediator) =>
            {
                var response = await mediator.Send(new AssignTrackToBatchCommand(batchId, trackId));

                return response.IsSuccess
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            })
            .WithName("Assign Track To Batch")
            .WithSummary("Link a track to a batch")
            .Produces<RequestResponse<string>>(200)
            .Produces<RequestResponse<string>>(400);

            // 🔹 Recalculate Ranks + Progress (fix existing data for all students in batch)
            group.MapPost("/{batchId:guid}/recalculate-ranks", async (
                Guid batchId,
                IUnitOfWork unitOfWork,
                CancellationToken cancellationToken) =>
            {
                var batchStudentRepo = unitOfWork.GetRepository<BatchStudent>();
                var submissionRepo = unitOfWork.GetRepository<Submission>();
                var assignmentRepo = unitOfWork.GetRepository<Assignment>();
                var trackRepo = unitOfWork.GetRepository<Track>();
                var progressRepo = unitOfWork.GetRepository<ELearningProject.Models.Progress>();

                var allBatchStudents = await batchStudentRepo
                    .FindByCondition(bs => bs.BatchId == batchId)
                    .ToListAsync(cancellationToken);

                if (!allBatchStudents.Any())
                    return Results.NotFound(new { message = "No students found in this batch." });

                // Get all tracks in this batch
                var tracks = await trackRepo
                    .FindByCondition(t => t.BatchId == batchId)
                    .ToListAsync(cancellationToken);

                // ── Step 1: Recalculate CompletionPercentage for each student × track ──
                foreach (var bs in allBatchStudents)
                {
                    foreach (var track in tracks)
                    {
                        var totalAssignments = await assignmentRepo
                            .FindByCondition(a => a.Lecture.TrackId == track.Id)
                            .CountAsync(cancellationToken);

                        if (totalAssignments == 0) continue;

                        var completedCount = await submissionRepo.FindByCondition(s =>
                            s.StudentId == bs.StudentId &&
                            s.Assignment.Lecture.TrackId == track.Id &&
                            s.Score != null && s.IsFinalized)
                            .CountAsync(cancellationToken);

                        var percentage = Math.Min((int)(((double)completedCount / totalAssignments) * 100), 100);

                        var progressRecord = await progressRepo
                            .FindByCondition(p => p.StudentId == bs.StudentId && p.TrackId == track.Id)
                            .FirstOrDefaultAsync(cancellationToken);

                        if (progressRecord == null)
                        {
                            await progressRepo.CreateAsync(new ELearningProject.Models.Progress
                            {
                                StudentId = bs.StudentId,
                                TrackId = track.Id,
                                CompletionPercentage = percentage
                            });
                        }
                        else
                        {
                            progressRecord.CompletionPercentage = percentage;
                            progressRepo.Update(progressRecord);
                        }
                    }
                }

                // ── Step 2: Recalculate AverageScore for each student ──
                foreach (var bs in allBatchStudents)
                {
                    var grades = await submissionRepo.FindByCondition(s =>
                        s.StudentId == bs.StudentId &&
                        s.Assignment.Lecture.Track.BatchId == batchId &&
                        s.Score != null && s.IsFinalized)
                        .Select(s => s.Score!.Value)
                        .ToListAsync(cancellationToken);

                    bs.AverageScore = grades.Any() ? grades.Average() : 0;
                }

                // ── Step 3: Assign Ranks in memory ──
                var ranked = allBatchStudents.OrderByDescending(bs => bs.AverageScore).ToList();
                int currentRank = 1;
                for (int i = 0; i < ranked.Count; i++)
                {
                    if (i > 0 && ranked[i].AverageScore < ranked[i - 1].AverageScore)
                        currentRank = i + 1;

                    ranked[i].Rank = currentRank;
                    batchStudentRepo.Update(ranked[i]);
                }

                await unitOfWork.SaveChangesAsync();

                return Results.Ok(new { message = $"Recalculated ranks & progress for {allBatchStudents.Count} students across {tracks.Count} tracks." });
            })
            .WithName("Recalculate Batch Ranks")
            .WithSummary("Recalculate AverageScore, Rank and CompletionPercentage for all students in a batch")
            .RequireAuthorization(policy => policy.RequireRole("Admin", "SuperAdmin"));
        }
    }
}
