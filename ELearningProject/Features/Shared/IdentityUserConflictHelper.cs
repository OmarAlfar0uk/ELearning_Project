using Auth.Models;
using ELearningProject.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ELearningProject.Features.Shared
{
    public static class IdentityUserConflictHelper
    {
        public static Task<bool> ExistsByEmailOrUsernameIncludingDeletedAsync(
            UniversitySystemAuthContext context,
            UserManager<ApplicationUser> userManager,
            string emailOrUserName,
            CancellationToken cancellationToken)
        {
            var normalizedEmail = userManager.NormalizeEmail(emailOrUserName);
            var normalizedUserName = userManager.NormalizeName(emailOrUserName);

            return context.Users
                .IgnoreQueryFilters()
                .AnyAsync(
                    user => user.NormalizedEmail == normalizedEmail
                         || user.NormalizedUserName == normalizedUserName,
                    cancellationToken);
        }

        public static bool IsDuplicateUserNameConflict(DbUpdateException exception)
        {
            return exception.InnerException is SqlException sqlException
                && (sqlException.Number == 2601 || sqlException.Number == 2627)
                && sqlException.Message.Contains("UserNameIndex", StringComparison.OrdinalIgnoreCase);
        }
    }
}
