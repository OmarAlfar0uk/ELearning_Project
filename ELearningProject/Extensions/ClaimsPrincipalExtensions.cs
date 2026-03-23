
using System.Security.Claims;

namespace ELearningProject.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid GetUserId(this ClaimsPrincipal user)
        {
            var idClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            return idClaim != null && Guid.TryParse(idClaim.Value, out var id) ? id : Guid.Empty;
        }

        public static string GetUserRole(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        }
    }
}
