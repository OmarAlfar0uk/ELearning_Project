using System.Net;
using System.Text.Json;
using ELearningProject.Features.Shared;
using FluentValidation;

namespace Auth.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IWebHostEnvironment _env;

        public GlobalExceptionMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger,
            IWebHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                context.Response.ContentType = "application/json";

                var statusCode = HttpStatusCode.InternalServerError;
                var logLevel = LogLevel.Error;
                object errorResponse;

                switch (ex)
                {
                    case ValidationException validationEx:
                        statusCode = HttpStatusCode.BadRequest;
                        logLevel = LogLevel.Warning;
                        errorResponse = new
                        {
                            statusCode = (int)statusCode,
                            message = "Validation failed.",
                            errors = validationEx.Errors
                                .Select(e => new
                                {
                                    Field = e.PropertyName.Replace("RegisterDto.", ""),
                                    e.ErrorMessage
                                })
                        };
                        break;

                    case BadHttpRequestException badReqEx:
                        statusCode = HttpStatusCode.BadRequest;
                        logLevel = LogLevel.Warning;

                        var jsonEx = badReqEx.InnerException as JsonException
                            ?? badReqEx.InnerException?.InnerException as JsonException;

                        if (jsonEx != null)
                        {
                            errorResponse = EndpointResponse<object>.ErrorResponse(
                                "Invalid JSON request body.",
                                (int)statusCode,
                                new List<string>
                                {
                                    jsonEx.Message,
                                    "Ensure all fields are separated by commas and all strings are wrapped correctly."
                                });
                        }
                        else
                        {
                            errorResponse = EndpointResponse<object>.ErrorResponse(
                                badReqEx.Message,
                                (int)statusCode);
                        }
                        break;

                    case KeyNotFoundException keyNotFoundEx:
                        statusCode = HttpStatusCode.NotFound;
                        logLevel = LogLevel.Warning;
                        errorResponse = new
                        {
                            statusCode = (int)statusCode,
                            message = keyNotFoundEx.Message
                        };
                        break;

                    case UnauthorizedAccessException:
                        logLevel = LogLevel.Warning;

                        if (context.User.Identity?.IsAuthenticated ?? false)
                        {
                            statusCode = HttpStatusCode.Forbidden;
                            errorResponse = new
                            {
                                statusCode = (int)statusCode,
                                message = "You do not have permission to perform this action."
                            };
                        }
                        else
                        {
                            statusCode = HttpStatusCode.Unauthorized;
                            errorResponse = new
                            {
                                statusCode = (int)statusCode,
                                message = "Invalid email or password."
                            };
                        }
                        break;

                    default:
                        statusCode = HttpStatusCode.InternalServerError;
                        errorResponse = new
                        {
                            statusCode = (int)statusCode,
                            message = "An unexpected error occurred.",
                            details = _env.IsDevelopment() ? ex.Message : null
                        };
                        break;
                }

                if (logLevel == LogLevel.Warning)
                {
                    _logger.LogWarning(ex,
                        "Exception | Path: {Path} | Method: {Method}",
                        context.Request.Path,
                        context.Request.Method);
                }
                else
                {
                    _logger.LogError(ex,
                        "Exception | Path: {Path} | Method: {Method}",
                        context.Request.Path,
                        context.Request.Method);
                }

                context.Response.StatusCode = (int)statusCode;
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var json = JsonSerializer.Serialize(errorResponse, options);
                await context.Response.WriteAsync(json);
            }
        }
    }
}
