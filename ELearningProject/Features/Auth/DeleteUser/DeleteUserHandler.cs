using Auth.Models;
using ELearningProject.Data;
using ELearningProject.Features.Admin.DeleteUser;
using ELearningProject.Features.Shared;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace ELearningProject.Features.Auth.DeleteUser
{
    public class DeleteUserHandler
        : IRequestHandler<DeleteUserCommand, RequestResponse<bool>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly UniversitySystemAuthContext _context;

        public DeleteUserHandler(
            UserManager<ApplicationUser> userManager,
            UniversitySystemAuthContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<RequestResponse<bool>> Handle(
            DeleteUserCommand request,
            CancellationToken cancellationToken)
        {
            var user = await _context.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user == null)
                return RequestResponse<bool>.Fail("User not found");

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("SuperAdmin"))
                return RequestResponse<bool>.Fail("Cannot delete SuperAdmin");

            await using var transaction =
                await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                // Remove related rows that would otherwise block deleting the user.
                await _context.BatchStudents
                    .IgnoreQueryFilters()
                    .Where(x => x.StudentId == request.UserId)
                    .ExecuteDeleteAsync(cancellationToken);

                await _context.Submissions
                    .IgnoreQueryFilters()
                    .Where(x => x.StudentId == request.UserId)
                    .ExecuteDeleteAsync(cancellationToken);

                await _context.Progresses
                    .IgnoreQueryFilters()
                    .Where(x => x.StudentId == request.UserId)
                    .ExecuteDeleteAsync(cancellationToken);

                await _context.InstructorTracks
                    .IgnoreQueryFilters()
                    .Where(x => x.InstructorId == request.UserId)
                    .ExecuteDeleteAsync(cancellationToken);

                await _context.Articles
                    .IgnoreQueryFilters()
                    .Where(x => x.AuthorId == request.UserId)
                    .ExecuteDeleteAsync(cancellationToken);

                await _context.ActivationCodes
                    .IgnoreQueryFilters()
                    .Where(x => x.UserId == request.UserId)
                    .ExecuteDeleteAsync(cancellationToken);

                await _context.Notifications
                    .IgnoreQueryFilters()
                    .Where(x => x.UserId == request.UserId)
                    .ExecuteDeleteAsync(cancellationToken);

                await _context.UploadedFiles
                    .IgnoreQueryFilters()
                    .Where(x => x.UploadedById == request.UserId)
                    .ExecuteUpdateAsync(
                        setters => setters.SetProperty(x => x.UploadedById, (Guid?)null),
                        cancellationToken);

                var deleteResult = await _userManager.DeleteAsync(user);
                if (!deleteResult.Succeeded)
                {
                    await transaction.RollbackAsync(cancellationToken);

                    return RequestResponse<bool>.Fail(
                        deleteResult.Errors.FirstOrDefault()?.Description
                        ?? "Failed to permanently delete user");
                }

                await transaction.CommitAsync(cancellationToken);

                return RequestResponse<bool>.Success(
                    true,
                    "User permanently deleted successfully"
                );
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);

                Log.Error(ex, "Hard delete failed for user {UserId}", request.UserId);

                return RequestResponse<bool>.Fail("Failed to permanently delete user");
            }
        }
    }
}
