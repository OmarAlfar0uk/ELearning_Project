using ELearningProject.Contracts;
using ELearningProject.Services;

namespace ELearningProject.Features.Extensions
{
    public static class AuditExtensions
    {
        public static IServiceCollection AddAuditLogging(
            this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddScoped<IAuditLogger, AuditLogger>();

            return services;
        }
    }
}
