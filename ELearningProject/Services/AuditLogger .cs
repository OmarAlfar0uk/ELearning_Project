using ELearningProject.Contracts;
using ELearningProject.Data;
using ELearningProject.Models;
using Serilog;
using System.Security.Claims;

namespace ELearningProject.Services
{
    public class AuditLogger : IAuditLogger
    {
        private readonly UniversitySystemAuthContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditLogger(
            UniversitySystemAuthContext context,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogAsync(
       string action,
       string? userId = null,
       string? targetId = null,
       string? description = null)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;

                var actorId =
                    userId ??
                    httpContext?.User.FindFirstValue("id");

                var ip =
                    httpContext?.Connection.RemoteIpAddress?.ToString();

                var log = new AuditLog
                {
                    Id = Guid.NewGuid(),
                    Action = action,
                    UserId = actorId,
                    TargetId = targetId,
                    Description = description,
                    IpAddress = ip
                };

                _context.AuditLogs.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(
                    ex,
                    "AuditLog failed | Action={Action} | UserId={UserId} | TargetId={TargetId}",
                    action,
                    userId,
                    targetId
                );
            }
        }

    }
}
