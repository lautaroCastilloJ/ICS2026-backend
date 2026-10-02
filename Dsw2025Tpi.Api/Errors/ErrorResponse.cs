using System.Text.Json.Serialization;

namespace Dsw2025Tpi.Api.Errors;

/// <summary>
/// Contrato unico de error de la API. Lo producen el middleware global, la
/// validacion automatica de modelos y los filtros, de modo que el frontend
/// siempre recibe la misma forma: { code, message, status, traceId, ... }.
/// </summary>
public sealed record ErrorResponse(
    string Code,
    string Message,
    int Status,
    string TraceId)
{
    /// <summary>Errores por campo (solo en errores de validacion).</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IDictionary<string, string[]>? Errors { get; init; }

    /// <summary>Stack trace (solo en Development y para errores inesperados).</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Details { get; init; }
}
