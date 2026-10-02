using Dsw2025Tpi.Shared.Resources;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2025Tpi.Api.Errors;

public static class ValidationErrorResponseExtensions
{
    /// <summary>
    /// Reemplaza el ValidationProblemDetails que [ApiController] devuelve cuando
    /// el ModelState es invalido (incluye las reglas de FluentValidation) por el
    /// mismo <see cref="ErrorResponse"/> que usa el middleware global.
    /// </summary>
    public static IMvcBuilder AddValidationErrorResponse(this IMvcBuilder builder)
    {
        return builder.ConfigureApiBehaviorOptions(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(e => e.Value is { Errors.Count: > 0 })
                    .ToDictionary(
                        e => e.Key,
                        e => e.Value!.Errors.Select(x => x.ErrorMessage).ToArray());

                var response = new ErrorResponse(
                    ErrorMessages.ValidationError,
                    ErrorMessages.Get(ErrorMessages.ValidationError),
                    StatusCodes.Status400BadRequest,
                    context.HttpContext.TraceIdentifier)
                {
                    Errors = errors
                };

                return new BadRequestObjectResult(response);
            };
        });
    }
}
