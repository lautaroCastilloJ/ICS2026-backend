using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Dsw2025Tpi.Data;

/// <summary>
/// Cadena de conexion usada por las herramientas de EF Core en tiempo de diseno
/// (dotnet ef migrations / database update).
///
/// Hace falta porque los DbContext se registran con una DbConnection inyectada
/// (ver ServiceCollectionExtensions.AddDomainServices) y no con una cadena literal:
/// sin estas factorias, las herramientas dependerian de poder construir el host de
/// la aplicacion para resolver esa dependencia.
///
/// Prioridad: variable de entorno ConnectionStrings__DefaultConnection y, si no
/// esta definida, la cadena de desarrollo por defecto.
/// </summary>
internal static class DesignTimeConnection
{
    private const string Fallback =
        @"Server=(localdb)\MSSQLLocalDB;Database=Dsw2025TpiDbV1;Trusted_Connection=true;TrustServerCertificate=true;";

    public static string Get()
    {
        var fromEnv = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        return string.IsNullOrWhiteSpace(fromEnv) ? Fallback : fromEnv;
    }
}

public class Dsw2025TpiContextFactory : IDesignTimeDbContextFactory<Dsw2025TpiContext>
{
    public Dsw2025TpiContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<Dsw2025TpiContext>()
            .UseSqlServer(DesignTimeConnection.Get())
            .Options;

        return new Dsw2025TpiContext(options);
    }
}

public class AuthenticateContextFactory : IDesignTimeDbContextFactory<AuthenticateContext>
{
    public AuthenticateContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AuthenticateContext>()
            .UseSqlServer(DesignTimeConnection.Get())
            .Options;

        return new AuthenticateContext(options);
    }
}
