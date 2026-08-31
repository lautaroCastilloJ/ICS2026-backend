using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Dsw2025Tpi.Api.Filters;

/// <summary>
/// Action Filter que valida y extrae el customerId del token JWT.
/// El customerId validado se almacena en HttpContext.Items para acceso posterior.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public sealed class ValidateCustomerIdAttribute : ActionFilterAttribute
{
    public const string CustomerIdKey = "ValidatedCustomerId";

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var customerIdClaim = context.HttpContext.User.FindFirst("customerId")?.Value;

        if (string.IsNullOrWhiteSpace(customerIdClaim))
        {
            context.Result = new ForbidResult();
            return;
        }

        if (!Guid.TryParse(customerIdClaim, out var customerId) || customerId == Guid.Empty)
        {
            context.Result = new UnauthorizedObjectResult(new 
            { 
                error = "No se pudo resolver el cliente desde el token." 
            });
            return;
        }

        // Almacenar el customerId validado en HttpContext.Items
        context.HttpContext.Items[CustomerIdKey] = customerId;

        base.OnActionExecuting(context);
    }
}
