namespace ELearningProject.Middlewares
{
    public class CorrelationIdMiddleware
    {
        public const string HeaderName = "X-Correlation-Id";

        private readonly RequestDelegate _next;
        private readonly ILogger<CorrelationIdMiddleware> _logger;

        public CorrelationIdMiddleware(
            RequestDelegate next,
            ILogger<CorrelationIdMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // 1) Get or create CorrelationId
            if (!context.Request.Headers.TryGetValue(HeaderName, out var correlationId)
                || string.IsNullOrWhiteSpace(correlationId))
            {
                correlationId = Guid.NewGuid().ToString();
            }

            // 2) Store it for later usage
            context.Items[HeaderName] = correlationId.ToString();

            // 3) Add to response headers
            context.Response.OnStarting(() =>
            {
                context.Response.Headers[HeaderName] = correlationId.ToString();
                return Task.CompletedTask;
            });

            // 4) Push to log scope
            using (_logger.BeginScope("{CorrelationId}", correlationId.ToString()))
            {
                await _next(context);
            }
        }
    }
}
