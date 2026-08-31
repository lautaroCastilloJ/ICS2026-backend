using Dsw2025Tpi.Data.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Dsw2025Tpi.Data;

public class AuthenticateContext : IdentityDbContext<AppUser>
{
    public AuthenticateContext(DbContextOptions<AuthenticateContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

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
