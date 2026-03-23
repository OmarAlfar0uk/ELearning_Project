using Serilog.Context;

namespace ELearningProject.Middlewares
{
    public class SerilogEnricherMiddleware
    {
        private readonly RequestDelegate _next;

        public SerilogEnricherMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var userId = context.User?.FindFirst("id")?.Value;
            var ip = context.Connection.RemoteIpAddress?.ToString();

            using (LogContext.PushProperty("UserId", userId ?? "Anonymous"))
            using (LogContext.PushProperty("IP", ip))
            {
                await _next(context);
            }
        }
    }
}
