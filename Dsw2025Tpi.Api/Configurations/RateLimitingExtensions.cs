using System.Globalization;
using System.Threading.RateLimiting;
using Dsw2025Tpi.Api.Errors;
using Dsw2025Tpi.Shared.Resources;
using Microsoft.AspNetCore.RateLimiting;

namespace Dsw2025Tpi.Api.Configurations;

/// <summary>Nombres de las politicas, para usar en [EnableRateLimiting].</summary>
public static class RateLimitPolicies
{
    /// <summary>Login, registro y cambio de contraseña: limite estricto por IP.</summary>
    public const string Auth = "auth";
}

/// <summary>Limites de una ventana fija: PermitLimit requests cada WindowSeconds.</summary>
public sealed class RateLimitWindowOptions
{
    public int PermitLimit { get; set; }
    public int WindowSeconds { get; set; }
}

/// <summary>Seccion "RateLimiting" de appsettings.json.</summary>
public sealed class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";

    /// <summary>Para toda la API, por IP.</summary>
    public RateLimitWindowOptions Global { get; set; } = new() { PermitLimit = 100, WindowSeconds = 60 };

    /// <summary>Para los endpoints con la politica <see cref="RateLimitPolicies.Auth"/>, por IP.</summary>
    public RateLimitWindowOptions Auth { get; set; } = new() { PermitLimit = 5, WindowSeconds = 60 };
}

public static class RateLimitingExtensions
{
    public const string ErrorCode = "RATE_LIMIT_EXCEEDED";

    /// <summary>
    /// Rate limiting por IP con el middleware incluido en ASP.NET Core:
    /// - un limite global para todas las requests;
    /// - la politica "auth", mas estricta, para los endpoints que validan
    ///   contraseñas o crean cuentas (frena fuerza bruta y altas masivas).
    /// Al superarlo se responde 429 con el ErrorResponse de siempre y el header
    /// Retry-After.
    ///
    /// La IP es la de la conexion. Si la API se publica detras de un proxy o
    /// balanceador, configurar ForwardedHeaders antes; si no, todos los clientes
    /// compartirian la IP del proxy y el mismo limite.
    /// </summary>
    public static IServiceCollection AddApiRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection(RateLimitingOptions.SectionName).Get<RateLimitingOptions>()
            ?? new RateLimitingOptions();

        services.AddRateLimiter(limiter =>
        {
            limiter.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            limiter.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                FixedWindowPartition($"global:{ClientIp(context)}", options.Global));

            limiter.AddPolicy(RateLimitPolicies.Auth, context =>
                FixedWindowPartition($"auth:{ClientIp(context)}", options.Auth));

            limiter.OnRejected = WriteRejectionAsync;
        });

        return services;
    }

    private static string ClientIp(HttpContext context)
        => context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

    private static RateLimitPartition<string> FixedWindowPartition(string key, RateLimitWindowOptions window)
        => RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = window.PermitLimit,
            Window = TimeSpan.FromSeconds(window.WindowSeconds),
            // Sin cola: lo que excede se rechaza enseguida en lugar de quedar esperando.
            QueueLimit = 0,
        });

    private static async ValueTask WriteRejectionAsync(OnRejectedContext rejected, CancellationToken cancellationToken)
    {
        var context = rejected.HttpContext;

        if (rejected.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.Response.Headers.RetryAfter =
                ((int)Math.Ceiling(retryAfter.TotalSeconds)).ToString(CultureInfo.InvariantCulture);
        }

        context.RequestServices.GetRequiredService<ILoggerFactory>()
            .CreateLogger(typeof(RateLimitingExtensions))
            .LogWarning("Rate limit superado: {Method} {Path} desde {Ip}",
                context.Request.Method, context.Request.Path, ClientIp(context));

        var error = new ErrorResponse(
            ErrorCode,
            ErrorMessages.Get(ErrorCode),
            StatusCodes.Status429TooManyRequests,
            context.TraceIdentifier);

        await context.Response.WriteAsJsonAsync(error, cancellationToken);
    }
}
