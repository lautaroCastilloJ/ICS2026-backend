using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Application.Services;
using Dsw2025Tpi.Data;
using Dsw2025Tpi.Data.Identity;
using Dsw2025Tpi.Data.Persistence;
using Dsw2025Tpi.Data.Repositories;
using Dsw2025Tpi.Domain.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace Dsw2025Tpi.Api.Configurations;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDomainServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // ========= Conexion compartida =========
        // Ambos contextos apuntan a la misma base de datos. Al compartir la misma
        // DbConnection (con alcance por request) pueden enlistarse en una unica
        // transaccion local de SQL Server, y IUnitOfWork puede coordinarlos sin
        // promover a una transaccion distribuida (MSDTC).
        services.AddScoped<DbConnection>(_ => new SqlConnection(connectionString));

        // ========= Database Contexts =========
        services.AddDbContext<Dsw2025TpiContext>((sp, options) =>
            options.UseSqlServer(sp.GetRequiredService<DbConnection>()));

        services.AddDbContext<AuthenticateContext>((sp, options) =>
            options.UseSqlServer(sp.GetRequiredService<DbConnection>()));

        // ========= Application Services =========
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IOrderService, OrderService>();

        // ========= Repository & UnitOfWork =========
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
