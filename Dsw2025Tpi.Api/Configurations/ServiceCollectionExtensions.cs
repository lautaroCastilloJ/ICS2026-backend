using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Application.Services;
using Dsw2025Tpi.Data;
using Dsw2025Tpi.Data.Identity;
using Dsw2025Tpi.Data.Repositories;
using Dsw2025Tpi.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dsw2025Tpi.Api.Configurations;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDomainServices(this IServiceCollection services, IConfiguration configuration)
    {
        // ========= Database Context =========
        // La cadena de conexion es un secreto: no esta en appsettings.json (ver README).
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                "Falta 'ConnectionStrings:DefaultConnection'. En desarrollo: dotnet user-secrets set \"ConnectionStrings:DefaultConnection\" \"<cadena>\" --project Dsw2025Tpi.Api");

        services.AddDbContext<Dsw2025TpiDbContext>(options =>
            options.UseSqlServer(connectionString));

        // ========= Application Services =========
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IOrderService, OrderService>();

        return services;
    }
}
