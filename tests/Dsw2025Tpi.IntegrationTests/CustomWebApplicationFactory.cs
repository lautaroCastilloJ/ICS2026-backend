using Dsw2025Tpi.Api;
using Dsw2025Tpi.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Dsw2025Tpi.IntegrationTests;

/// <summary>
/// Levanta la API completa (Program) en memoria contra una base SQLite en
/// memoria, sin SQL Server. Se comparte entre todas las pruebas de la
/// coleccion "IntegrationTests" (ver IntegrationTestsCollection).
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    // Valores ficticios, solo para pruebas. No sirven fuera de este proyecto.
    public const string JwtKey = "clave-de-pruebas-de-integracion-dsw2025tpi-32+";
    public const string AdminUserName = "admin.tests";
    public const string AdminEmail = "admin.tests@dsw2025tpi.local";
    public const string AdminPassword = "Admin123!";

    private readonly string _connectionString;

    // Una base SQLite en memoria vive mientras haya al menos una conexion
    // abierta. Esta conexion "ancla" la mantiene viva durante toda la sesion
    // de pruebas; cada DbContext de la API abre la suya contra la misma base
    // gracias a Cache=Shared.
    private readonly SqliteConnection _connection;

    public CustomWebApplicationFactory()
    {
        // Nombre unico: si hubiera dos factories, no compartirian la base.
        _connectionString = $"DataSource=Dsw2025TpiTests_{Guid.NewGuid():N};Mode=Memory;Cache=Shared";

        _connection = new SqliteConnection(_connectionString);
        _connection.Open();

        // El esquema se crea ANTES de que arranque la API: Program ejecuta
        // IdentitySeeder.SeedAsync antes de RunAsync y necesita las tablas de
        // Identity. EnsureCreated (no Migrate): las migraciones son de SQL Server.
        using var context = CreateDbContext();
        context.Database.EnsureCreated();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // UseSetting y no ConfigureAppConfiguration: Program lee la configuracion
        // mientras registra los servicios (antes de Build) y estos valores llegan
        // desde el inicio. Con "Testing" tampoco se cargan los User Secrets.
        builder.UseSetting("ConnectionStrings:DefaultConnection", _connectionString);
        builder.UseSetting("Jwt:Key", JwtKey);
        builder.UseSetting("SeedAdmin:UserName", AdminUserName);
        builder.UseSetting("SeedAdmin:Email", AdminEmail);
        builder.UseSetting("SeedAdmin:Password", AdminPassword);

        // Todas las requests de TestServer comparten "IP": con los limites reales
        // (100 y 5 por minuto) las pruebas recibirian 429. El rate limiting ya
        // tiene sus propias pruebas en Dsw2025Tpi.Tests.
        builder.UseSetting("RateLimiting:Global:PermitLimit", "100000");
        builder.UseSetting("RateLimiting:Auth:PermitLimit", "100000");

        // ConfigureTestServices corre DESPUES de los registros de Program, asi
        // que puede reemplazar el UseSqlServer de AddDomainServices por SQLite.
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<Dsw2025TpiDbContext>>();
            services.AddDbContext<Dsw2025TpiDbContext>(options => options.UseSqlite(_connectionString));
        });
    }

    /// <summary>
    /// Contexto contra la misma base que usa la API, para preparar datos o
    /// verificar lo que se guardo. Quien lo pide debe liberarlo (using).
    /// </summary>
    public Dsw2025TpiDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<Dsw2025TpiDbContext>()
            .UseSqlite(_connectionString)
            .Options;

        return new Dsw2025TpiDbContext(options);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
            _connection.Dispose();
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
