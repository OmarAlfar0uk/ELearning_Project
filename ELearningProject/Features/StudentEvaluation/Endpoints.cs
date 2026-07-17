using ELearningProject.Features.Shared;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace ELearningProject.Features.StudentEvaluation
{
    /// <summary>
    /// Extension class to map Minimal API endpoints for Student Evaluation.
    /// </summary>
    public static class Endpoints
    {
        public static void MapStudentEvaluationEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v1")
                           .WithTags("Student Evaluation");

            // ── GET /api/v1/students/{studentId:guid}/evaluation?trackId={guid?} ──
            // Returns aggregate evaluation metrics for a student.
            // Fine-grained authorization is enforced inside GetStudentEvaluationHandler.
            group.MapGet("/students/{studentId:guid}/evaluation", async (
                Guid studentId,
                [FromQuery] Guid? trackId,
                IMediator mediator) =>
            {
                var response = await mediator.Send(new GetStudentEvaluationQuery(studentId, trackId));

                return response.IsSuccess
                    ? Results.Ok(response)
                    : Results.Json(response, statusCode: response.StatusCode);
            })
            .WithName("Get Student Evaluation")
            .WithSummary("Get aggregate performance and progress metrics for a student")
            .Produces<EndpointResponse<StudentEvaluationDto>>(200)
            .Produces<EndpointResponse<StudentEvaluationDto>>(401)
            .Produces<EndpointResponse<StudentEvaluationDto>>(403)
            .Produces<EndpointResponse<StudentEvaluationDto>>(404)
            .RequireAuthorization();
        }
    }
}
