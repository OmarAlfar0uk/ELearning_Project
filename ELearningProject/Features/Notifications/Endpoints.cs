
using ELearningProject.Features.Notifications.Commands.MarkAllAsRead;
using ELearningProject.Features.Notifications.Commands.MarkAsRead;
using ELearningProject.Features.Notifications.DTOs;
using ELearningProject.Features.Notifications.Queries.GetMyNotifications;
using ELearningProject.Features.Notifications.Queries.GetUnreadCount;
using ELearningProject.Features.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ELearningProject.Features.Notifications
{
    public static class Endpoints
    {
        public static void MapNotificationEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v1/notifications")
                           .WithTags("Notifications")
                           .RequireAuthorization(); // Helper to require auth for all

            // 1. Get My Notifications (Paginated)
            group.MapGet("/", async (
                IMediator mediator,
                ClaimsPrincipal user,
                int pageNumber = 1,
                int pageSize = 10) =>
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null) return Results.Unauthorized();

                var query = new GetMyNotificationsQuery
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    UserId = Guid.Parse(userId)
                };

                var response = await mediator.Send(query);

                return response.IsSuccess
                    ? Results.Ok(response)
                    : Results.StatusCode(response.StatusCode);
            })
            .WithName("Get My Notifications")
            .WithSummary("Get user notifications with pagination")
            .Produces<EndpointResponse<PaginatedResult<NotificationDto>>>(200);

            // 2. Get Unread Count
            group.MapGet("/unread-count", async (
                IMediator mediator,
                ClaimsPrincipal user) =>
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null) return Results.Unauthorized();

                var response = await mediator.Send(new GetUnreadCountQuery(Guid.Parse(userId)));

                return response.IsSuccess
                    ? Results.Ok(response)
                    : Results.StatusCode(response.StatusCode);
            })
            .WithName("Get Unread Count")
            .WithSummary("Get count of unread notifications")
            .Produces<EndpointResponse<int>>(200);

            // 3. Mark As Read
            group.MapPost("/{notificationId:guid}/read", async (
                Guid notificationId,
                IMediator mediator,
                ClaimsPrincipal user) =>
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null) return Results.Unauthorized();

                var command = new MarkAsReadCommand
                {
                    NotificationId = notificationId,
                    UserId = Guid.Parse(userId)
                };

                var response = await mediator.Send(command);

                return response.IsSuccess
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            })
            .WithName("Mark Notification As Read")
            .WithSummary("Mark a specific notification as read")
            .Produces<RequestResponse<string>>(200)
            .Produces<RequestResponse<string>>(400);

            // 4. Mark All As Read
            group.MapPost("/read-all", async (
                IMediator mediator,
                ClaimsPrincipal user) =>
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null) return Results.Unauthorized();

                var response = await mediator.Send(new MarkAllAsReadCommand(Guid.Parse(userId)));

                return response.IsSuccess
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            })
            .WithName("Mark All As Read")
            .WithSummary("Mark all user notifications as read")
            .Produces<RequestResponse<string>>(200);
        }
    }
}
