using AutoMapper;
using Dsw2025Tpi.Api.Controllers;
using Dsw2025Tpi.Api.Middlewares;
using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Application.Mappings;
using Dsw2025Tpi.Application.Services;
using Dsw2025Tpi.Data;
using Dsw2025Tpi.Data.Repositories;
using Dsw2025Tpi.Domain.Interfaces;
using Dsw2025Tpi.Shared.Exceptions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Xunit;

namespace Dsw2025Tpi.Tests;

// SQL Server real para probar rollback y escrituras concurrentes, sin usar la
// cadena de conexion de la aplicacion ni tocar su base de desarrollo.
public sealed class OrdersDatabaseFixture : IAsyncLifetime
{
    private readonly string _databaseName = $"ICS_OrdersTests_{Guid.NewGuid():N}";
    private WebApplication? _app;
    private bool _created;
    public string ConnectionString { get; }
    public HttpClient Client { get; private set; } = null!;
    public IMapper Mapper { get; } = new MapperConfiguration(c => c.AddProfile<MappingProfiles>()).CreateMapper();

    public OrdersDatabaseFixture()
    {
        ConnectionString = new SqlConnectionStringBuilder
        {
            DataSource = Environment.GetEnvironmentVariable("ICS_TEST_SQLSERVER") ?? "localhost",
            InitialCatalog = _databaseName,
            IntegratedSecurity = true,
            TrustServerCertificate = true,
            ConnectTimeout = 10,
        }.ConnectionString;
    }

    public Dsw2025TpiDbContext CreateContext(params IInterceptor[] interceptors)
    {
        var options = new DbContextOptionsBuilder<Dsw2025TpiDbContext>()
            .UseSqlServer(ConnectionString)
            .AddInterceptors(interceptors);
        return new Dsw2025TpiDbContext(options.Options);
    }

    public OrderService CreateService(Dsw2025TpiDbContext db) => new(
        new EfRepository<Dsw2025Tpi.Domain.Entities.Order>(db),
        new EfRepository<Dsw2025Tpi.Domain.Entities.Product>(db),
        new EfRepository<Dsw2025Tpi.Domain.Entities.Customer>(db), Mapper);

    public async Task InitializeAsync()
    {
        await using var db = CreateContext();
        _created = await db.Database.EnsureCreatedAsync();
        if (!_created) throw new InvalidOperationException("La base temporal debe ser nueva.");

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Production" });
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        builder.Services.AddControllers().AddApplicationPart(typeof(OrdersController).Assembly);
        builder.Services.AddAuthentication("Tests").AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>("Tests", _ => { });
        builder.Services.AddAuthorization();
        builder.Services.AddDbContext<Dsw2025TpiDbContext>(o => o.UseSqlServer(ConnectionString));
        builder.Services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        builder.Services.AddScoped<IOrderService, OrderService>();
        builder.Services.AddSingleton(Mapper);
        _app = builder.Build();
        _app.UseMiddleware<ExceptionMiddleware>();
        _app.UseAuthentication();
        _app.UseAuthorization();
        _app.MapControllers();
        _app.MapGet("/test-conflict", () => Task.FromException(new ConcurrentUpdateException(new Exception("test"))));
        await _app.StartAsync();
        Client = new HttpClient { BaseAddress = new Uri(_app.Urls.Single()) };
    }

    public async Task DisposeAsync()
    {
        Client?.Dispose();
        if (_app is not null) await _app.DisposeAsync();

        // Solo eliminar la base aleatoria creada por ESTE fixture.
        if (_created && new SqlConnectionStringBuilder(ConnectionString).InitialCatalog == _databaseName
            && _databaseName.StartsWith("ICS_OrdersTests_", StringComparison.Ordinal))
        {
            await using var db = CreateContext();
            await db.Database.EnsureDeletedAsync();
        }
    }
}

// Autenticacion exclusivamente del host de pruebas; los roles/claims pasan
// por los atributos y filtros reales del controlador.
public sealed class TestAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder) : base(options, logger, encoder) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("X-Test-Role", out var role))
            return Task.FromResult(AuthenticateResult.NoResult());

        var claims = new List<Claim> { new(ClaimTypes.Role, role.ToString()) };
        if (Request.Headers.TryGetValue("X-Test-Customer", out var customer))
            claims.Add(new Claim("customerId", customer.ToString()));
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme.Name));
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name)));
    }
}
