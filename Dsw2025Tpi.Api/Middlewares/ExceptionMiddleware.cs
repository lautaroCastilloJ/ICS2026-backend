using Dsw2025Tpi.Shared.Exceptions;
using System.Text.Json;

namespace Dsw2025Tpi.Api.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionMiddleware(
        RequestDelegate next, 
        ILogger<ExceptionMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ExceptionBase ex)
        {
            _logger.LogWarning(ex, "Domain exception captured: {Code} - {Message}", ex.Code, ex.Message);
            await HandleExceptionAsync(context, ex, StatusCodes.Status400BadRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled server error: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex, StatusCodes.Status500InternalServerError);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception, int statusCode)
    {
        // Check if response has already started
        if (context.Response.HasStarted)
        {
            _logger.LogWarning("Cannot write exception response. Response has already started.");
            return;
        }

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        object response;
        
        if (exception is ExceptionBase domainEx)
        {
            response = new
            {
                code = domainEx.Code,
                message = domainEx.Message,
                status = statusCode,
                traceId = context.TraceIdentifier
            };
        }
        else
        {
            response = new
            {
                code = "UNEXPECTED_ERROR",
                message = _env.IsDevelopment() 
                    ? exception.Message 
                    : "An unexpected error occurred.",
                status = statusCode,
                traceId = context.TraceIdentifier,
                details = _env.IsDevelopment() ? exception.StackTrace : null
            };
        }

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = _env.IsDevelopment()
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}
