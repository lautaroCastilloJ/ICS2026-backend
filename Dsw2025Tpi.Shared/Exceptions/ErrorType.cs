namespace Dsw2025Tpi.Shared.Exceptions;

/// <summary>
/// Categoria semantica de un error. Es agnostica del transporte: cada capa de
/// presentacion decide como traducirla (la API la mapea a un status HTTP).
/// </summary>
public enum ErrorType
{
    /// <summary>Datos de entrada invalidos.</summary>
    Validation,

    /// <summary>La operacion viola una regla de negocio sobre datos validos.</summary>
    BusinessRule,

    /// <summary>El recurso solicitado no existe.</summary>
    NotFound,

    /// <summary>La operacion choca con el estado actual del recurso.</summary>
    Conflict,

    /// <summary>No se pudo autenticar al usuario.</summary>
    Unauthorized
}
