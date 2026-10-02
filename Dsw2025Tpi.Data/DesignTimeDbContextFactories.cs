using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Dsw2025Tpi.Data;

/// <summary>
/// Factoria usada por las herramientas de EF Core en tiempo de diseno
/// (dotnet ef migrations / database update), para no depender de poder construir
/// el host de la aplicacion.
///
/// Prioridad: variable de entorno ConnectionStrings__DefaultConnection y, si no
/// esta definida, la cadena de desarrollo por defecto.
/// </summary>
public class Dsw2025TpiDbContextFactory : IDesignTimeDbContextFactory<Dsw2025TpiDbContext>
{
    private const string Fallback =
        @"Server=(localdb)\MSSQLLocalDB;Database=Dsw2025TpiDbV1;Trusted_Connection=true;TrustServerCertificate=true;";

    public Dsw2025TpiDbContext CreateDbContext(string[] args)
    {
        var fromEnv = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        var connectionString = string.IsNullOrWhiteSpace(fromEnv) ? Fallback : fromEnv;

        var options = new DbContextOptionsBuilder<Dsw2025TpiDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new Dsw2025TpiDbContext(options);
    }
}
