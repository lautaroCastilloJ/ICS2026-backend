using Dsw2025Tpi.Api.Filters;

namespace Dsw2025Tpi.Api.Extensions;

/// <summary>
/// Extension methods para HttpContext
/// </summary>
public static class HttpContextExtensions
{
    /// <summary>
    /// Obtiene el customerId validado por el ValidateCustomerIdAttribute.
    /// Lanza una excepción si no se encuentra el customerId.
    /// </summary>
    /// <param name="httpContext">El HttpContext actual</param>
    /// <returns>El customerId validado</returns>
    /// <exception cref="InvalidOperationException">Si no se encuentra el customerId validado</exception>
    public static Guid GetCustomerId(this HttpContext httpContext)
    {
        if (httpContext.Items.TryGetValue(ValidateCustomerIdAttribute.CustomerIdKey, out var customerIdObj) 
            && customerIdObj is Guid customerId)
        {
            return customerId;
        }

        throw new InvalidOperationException(
            "CustomerId no encontrado. Asegúrese de que el action esté decorado con [ValidateCustomerId].");
    }

    /// <summary>
    /// Intenta obtener el customerId validado por el ValidateCustomerIdAttribute.
    /// </summary>
    /// <param name="httpContext">El HttpContext actual</param>
    /// <param name="customerId">El customerId validado si existe</param>
    /// <returns>true si se encontró el customerId, false en caso contrario</returns>
    public static bool TryGetCustomerId(this HttpContext httpContext, out Guid customerId)
    {
        if (httpContext.Items.TryGetValue(ValidateCustomerIdAttribute.CustomerIdKey, out var customerIdObj) 
            && customerIdObj is Guid id)
        {
            customerId = id;
            return true;
        }

        customerId = Guid.Empty;
        return false;
    }
}
