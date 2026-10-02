using Dsw2025Tpi.Api.Errors;
using Dsw2025Tpi.Shared.Exceptions;
using Dsw2025Tpi.Shared.Resources;

namespace Dsw2025Tpi.Api.Middlewares;

/// <summary>
/// Manejo global de errores. Traduce las excepciones de la aplicacion
/// (<see cref="ExceptionBase"/>, definidas en Shared) a un <see cref="ErrorResponse"/>
/// con el status HTTP que corresponde a su <see cref="ErrorType"/>, y oculta el
/// detalle de las excepciones inesperadas fuera de Development.
/// </summary>
public class ExceptionMiddleware
{
    // Status no estandar (convencion de nginx) para "el cliente cerro la conexion".
    private const int ClientClosedRequest = 499;

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
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // El cliente abandono la request: no es un error del servidor y no
            // hay a quien responderle.
            _logger.LogInformation("Request cancelled by the client: {Method} {Path}",
                context.Request.Method, context.Request.Path);

            if (!context.Response.HasStarted)
                context.Response.StatusCode = ClientClosedRequest;
        }
        catch (ExceptionBase ex)
        {
            var statusCode = MapStatusCode(ex.Type);
            _logger.LogWarning(ex, "Domain exception captured: {Code} ({Type}) - {Message}",
                ex.Code, ex.Type, ex.Message);

            await WriteResponseAsync(context, new ErrorResponse(
                ex.Code, ex.Message, statusCode, context.TraceIdentifier));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled server error: {Message}", ex.Message);

            await WriteResponseAsync(context, new ErrorResponse(
                ErrorMessages.UnexpectedError,
                _env.IsDevelopment() ? ex.Message : ErrorMessages.Get(ErrorMessages.UnexpectedError),
                StatusCodes.Status500InternalServerError,
                context.TraceIdentifier)
            {
                Details = _env.IsDevelopment() ? ex.StackTrace : null
            });
        }
    }

    private static int MapStatusCode(ErrorType type) => type switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        ErrorType.BusinessRule => StatusCodes.Status422UnprocessableEntity,
        _ => StatusCodes.Status400BadRequest
    };

    private async Task WriteResponseAsync(HttpContext context, ErrorResponse response)
    {
        if (context.Response.HasStarted)
        {
            _logger.LogWarning("Cannot write exception response. Response has already started.");
            return;
        }

        context.Response.Clear();
        context.Response.StatusCode = response.Status;

        // WriteAsJsonAsync usa las opciones JSON de la app (camelCase por defecto).
        await context.Response.WriteAsJsonAsync(response);
    }
}
