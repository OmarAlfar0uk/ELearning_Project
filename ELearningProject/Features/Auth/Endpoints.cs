using ELearningProject.Contracts;
using ELearningProject.Features.Auth.Admin.GetAdmins;
using ELearningProject.Features.Auth.Activate;
using ELearningProject.Features.Auth.Admin.ChangeRole;
using ELearningProject.Features.Auth.Admin.Create;
using ELearningProject.Features.Auth.Admin.CreateStudent;
using ELearningProject.Features.Auth.Admin.GetUsers;
using ELearningProject.Features.Auth.Admin.ToggleUserStatus;
using ELearningProject.Features.Admin.DeleteUser;
using ELearningProject.Features.Auth.ChangePassword;
using ELearningProject.Features.Auth.ForgetPassword.OTP;
using ELearningProject.Features.Auth.ForgetPassword.ResetPassword;
using ELearningProject.Features.Auth.GetCurrentUser;
using ELearningProject.Features.Auth.Login;
using ELearningProject.Features.Auth.Logout;
using ELearningProject.Features.Auth.Refresh;
using ELearningProject.Features.Auth.UpdateUserProfile;
using ELearningProject.Features.Shared;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

namespace ELearningProject.Features.Auth
{
    public static class Endpoints
    {
        public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/v1/auth")
                           .WithTags("Auth");

            group.MapPost("/activate", Activate)
                 .RequireRateLimiting("activate");
            group.MapPost("/login", Login)
                .RequireRateLimiting("login");
            group.MapPost("/refresh", Refresh);
            group.MapPost("/logout", Logout);
            
            
            group.MapPost("/admin/create", CreateAdmin)
                 .RequireAuthorization(policy => policy.RequireRole("SuperAdmin"));
            group.MapPut("/admin/change-role", ChangeUserRole)
                 .RequireAuthorization(policy => policy.RequireRole("SuperAdmin"));
            group.MapGet("/admin/admins", GetAdmins)
                 .RequireAuthorization(policy => policy.RequireRole("Admin", "SuperAdmin"));
            group.MapGet("/admin/users", GetUsers)
                 .RequireAuthorization(policy => policy.RequireRole("Admin", "SuperAdmin"));
            group.MapPut("/admin/users/{userId:guid}/toggle-status", ToggleUserStatus)
                  .RequireAuthorization(policy => policy.RequireRole("Admin", "SuperAdmin"));
            group.MapPost("/admin/students", CreateStudent)
                 .WithTags("Admin – Students")
                .RequireAuthorization(policy =>
                    policy.RequireRole("Admin", "SuperAdmin"));

            group.MapDelete("/admin/users/{userId:guid}", DeleteUser)
                 .WithName("Delete User")
                 .WithSummary("Soft-delete a user by ID (Admin/SuperAdmin)")
                 .RequireAuthorization(policy => policy.RequireRole("Admin", "SuperAdmin"));

            group.MapGet("/me", GetCurrentUser)
                 .RequireAuthorization();

            group.MapPost("/change-password", ChangePassword)
                 .RequireAuthorization();

            group.MapPut("/update-profile", UpdateProfile)
                 .DisableAntiforgery()
                 .RequireAuthorization();

            group.MapPost("/forget-password", ForgetPassword);

            group.MapPost("/verify-otp", VerifyOtp);

            group.MapPost("/reset-password", ResetPassword);


            return app;
        }

        public static IResult ToHttpResult<T>(this EndpointResponse<T> response)
            => Results.Json(response, statusCode: response.StatusCode);

        private static async Task<IResult> Activate(
            ActivateCommand command,
            IMediator mediator)
        {
            var result = await mediator.Send(command);
            return result.ToHttpResult();
        }

        private static async Task<IResult> Login(
            LoginCommand command,
            IMediator mediator)
        {
            var result = await mediator.Send(command);
            return result.ToHttpResult();
        }

        private static async Task<IResult> Refresh(
         RefreshTokenCommand command,
         IMediator mediator)
        {
            var result = await mediator.Send(command);
            return result.ToHttpResult();
        }

        private static async Task<IResult> Logout(
        LogoutCommand command,
        IMediator mediator)
            {
                var result = await mediator.Send(command);
                return result.ToHttpResult();
            }

       


        private static async Task<IResult> CreateAdmin(
        CreateAdminCommand command,
        IMediator mediator)
        {
            var result = await mediator.Send(command);
            return result.ToHttpResult();
        }

        private static async Task<IResult> ChangeUserRole(
        ChangeUserRoleCommand command,
        IMediator mediator)
        {
            var result = await mediator.Send(command);
            return result.ToHttpResult();
        }

        private static async Task<IResult> GetUsers(
        [AsParameters] GetUsersQuery query,
        IMediator mediator)
        {
            var result = await mediator.Send(query);
            return result.ToHttpResult();
        }

        private static async Task<IResult> GetAdmins(
        [AsParameters] GetAdminsQuery query,
        IMediator mediator)
        {
            var result = await mediator.Send(query);
            return result.ToHttpResult();
        }

        private static async Task<IResult> ToggleUserStatus(
        Guid userId,
        bool enable,
        IMediator mediator)
        {
            var command = new ToggleUserStatusCommand
            {
                UserId = userId,
                Enable = enable
            };

            var result = await mediator.Send(command);
            return result.ToHttpResult();
        }

        private static async Task<IResult> CreateStudent(
           CreateStudentCommand command,
           IMediator mediator)
        {
            var result = await mediator.Send(command);
            return result.ToHttpResult();
        }



        private static async Task<IResult> GetCurrentUser(
    HttpContext context,
    IMediator mediator)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Results.Unauthorized();

            var result = await mediator.Send(
                new GetCurrentUserQuery(Guid.Parse(userId))
            );

            return Results.Ok(result);
        }

        private static async Task<IResult> ChangePassword(
    ChangePasswordCommand command,
    IMediator mediator)
        {
            var result = await mediator.Send(command);
            return Results.Ok(new
            {
                success = result,
                message = "Password changed successfully."
            });
        }

        private static async Task<IResult> UpdateProfile(
            HttpContext context,
            [FromForm] string? firstName,
            [FromForm] string? lastName,
            [FromForm] string? phoneNumber,
            IFormFile? profileImage,
            IUpdateUserProfileOrchestrator orchestrator)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Results.Unauthorized();

            var request = new UpdateUserProfileRequest
            {
                FirstName = firstName,
                LastName = lastName,
                PhoneNumber = phoneNumber,
                ProfileImage = profileImage
            };

            var response = await orchestrator.UpdateUserProfileAsync(
                Guid.Parse(userId),
                request
            );

            return Results.Ok(response);
        }



        private static async Task<IResult> ForgetPassword(
    SendOtpCommand command,
    IMediator mediator)
        {
            var result = await mediator.Send(command);
            return Results.Ok(new
            {
                success = result,
                message = "OTP sent successfully."
            });
        }

        private static async Task<IResult> VerifyOtp(
    VerifyOtpCommand command,
    IMediator mediator)
        {
            var result = await mediator.Send(command);
            return Results.Ok(new
            {
                success = result,
                message = "OTP verified successfully."
            });
        }

        private static async Task<IResult> ResetPassword(
    ResetPasswordCommand command,
    IMediator mediator)
        {
            var result = await mediator.Send(command);
            return Results.Ok(new
            {
                success = result,
                message = "Password reset successfully."
            });
        }

        private static async Task<IResult> DeleteUser(
            Guid userId,
            IMediator mediator)
        {
            var result = await mediator.Send(new DeleteUserCommand(userId));

            if (!result.IsSuccess)
                return Results.BadRequest(new { message = result.Message });

            return Results.NoContent();
        }

    }
}
