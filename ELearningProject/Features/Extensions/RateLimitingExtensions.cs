using System.Threading.RateLimiting;

namespace ELearningProject.Features.Extensions
{
    public static class RateLimitingExtensions
    {
        public static IServiceCollection AddCustomRateLimiting(
            this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                options.GlobalLimiter =
                    PartitionedRateLimiter.Create<HttpContext, string>(context =>
                    {
                        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                        return RateLimitPartition.GetFixedWindowLimiter(
                            ip,
                            _ => new FixedWindowRateLimiterOptions
                            {
                                PermitLimit = 100,
                                Window = TimeSpan.FromMinutes(1),
                                QueueLimit = 0
                            });
                    });

                options.AddPolicy("login", context =>
                {
                    var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                    return RateLimitPartition.GetFixedWindowLimiter(
                        ip,
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 5,
                            Window = TimeSpan.FromMinutes(1),
                            QueueLimit = 0
                        });
                });

                options.AddPolicy("activate", context =>
                {
                    var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                    return RateLimitPartition.GetFixedWindowLimiter(
                        ip,
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 3,
                            Window = TimeSpan.FromMinutes(1),
                            QueueLimit = 0
                        });
                });

                options.AddPolicy("parent-generate", context =>
                {
                    var userId =
                        context.User?.FindFirst("id")?.Value ??
                        context.Connection.RemoteIpAddress?.ToString() ??
                        "unknown";

                    return RateLimitPartition.GetFixedWindowLimiter(
                        userId,
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 2,
                            Window = TimeSpan.FromHours(1),
                            QueueLimit = 0
                        });
                });

                options.RejectionStatusCode =
                    StatusCodes.Status429TooManyRequests;
            });

            return services;
        }
    }
}
