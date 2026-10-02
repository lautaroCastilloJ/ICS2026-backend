using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dsw2025Tpi.Data.Identity;

/// <summary>
/// Datos iniciales de Identity, ejecutados al arrancar la aplicacion:
/// 1. Crea los roles que falten.
/// 2. Si no existe ningun administrador, crea el primero con los datos de la
///    seccion "SeedAdmin" de la configuracion (User Secrets en desarrollo,
///    variables de entorno en produccion). Nunca desde el codigo versionado.
///
/// Es idempotente: si ya hay un administrador no hace nada, por lo que puede
/// ejecutarse en cada inicio.
/// </summary>
public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;

        var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = provider.GetRequiredService<UserManager<AppUser>>();
        var configuration = provider.GetRequiredService<IConfiguration>();
        var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(IdentitySeeder));

        foreach (var role in AppRoles.AllowedRoles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                logger.LogInformation("Rol {Role} creado", role);
            }
        }

        var admins = await userManager.GetUsersInRoleAsync(AppRoles.Administrador);
        if (admins.Count > 0)
            return;

        var seed = configuration.GetSection("SeedAdmin");
        var userName = seed["UserName"];
        var email = seed["Email"];
        var password = seed["Password"];

        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning(
                "No existe ningun administrador y no hay datos en 'SeedAdmin' (UserName, Email, Password). " +
                "Configurelos con User Secrets o variables de entorno para crear el primero.");
            return;
        }

        var admin = new AppUser
        {
            UserName = userName,
            Email = email,
            DisplayName = seed["DisplayName"] ?? userName
        };

        var result = await userManager.CreateAsync(admin, password);
        if (result.Succeeded)
            result = await userManager.AddToRoleAsync(admin, AppRoles.Administrador);

        // Fallar al iniciar es preferible a quedar sin administrador sin saberlo.
        if (!result.Succeeded)
            throw new InvalidOperationException(
                "No se pudo crear el administrador inicial: " +
                string.Join(" | ", result.Errors.Select(e => e.Description)));

        logger.LogWarning("Administrador inicial {UserName} creado desde la configuracion 'SeedAdmin'", userName);
    }
}
