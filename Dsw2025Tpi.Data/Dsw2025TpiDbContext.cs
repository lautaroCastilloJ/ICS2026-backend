using Dsw2025Tpi.Data.Configurations;
using Dsw2025Tpi.Data.Identity;
using Dsw2025Tpi.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Dsw2025Tpi.Data;

/// <summary>
/// Contexto unico de la aplicacion: entidades de dominio e Identity en la misma
/// base y el mismo change tracker, de modo que cualquier SaveChanges confirma
/// ambos grupos de tablas en una sola transaccion.
/// </summary>
public class Dsw2025TpiDbContext : IdentityDbContext<AppUser>
{
    public Dsw2025TpiDbContext(DbContextOptions<Dsw2025TpiDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Customer> Customers { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ========= Dominio =========
        builder.ApplyConfiguration(new CustomerConfiguration());
        builder.ApplyConfiguration(new ProductConfiguration());
        builder.ApplyConfiguration(new OrderConfiguration());
        builder.ApplyConfiguration(new OrderItemConfiguration());

        // ========= Identity =========
        builder.Entity<AppUser>(b =>
        {
            b.ToTable("Usuarios");

            b.Property(u => u.DisplayName)
             .HasMaxLength(100)
             .IsRequired(false);

            // Registrar la FK como columna normal
            b.Property(u => u.CustomerId)
             .IsRequired(false); // nullable, porque el Customer se crea antes

            // (Opcional) Forzar 1:1 lógico desde Identity
            b.HasIndex(u => u.CustomerId)
             .IsUnique()
             .HasFilter("[CustomerId] IS NOT NULL");
        });

        builder.Entity<IdentityRole>(b => { b.ToTable("Roles"); });
        builder.Entity<IdentityUserRole<string>>(b => { b.ToTable("UsuariosRoles"); });
        builder.Entity<IdentityUserClaim<string>>(b => { b.ToTable("UsuariosClaims"); });
        builder.Entity<IdentityUserLogin<string>>(b => { b.ToTable("UsuariosLogins"); });
        builder.Entity<IdentityRoleClaim<string>>(b => { b.ToTable("RolesClaims"); });
        builder.Entity<IdentityUserToken<string>>(b => { b.ToTable("UsuariosTokens"); });
    }
}
