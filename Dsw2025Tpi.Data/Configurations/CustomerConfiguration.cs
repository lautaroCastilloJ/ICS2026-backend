using Dsw2025Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dsw2025Tpi.Data.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        // PK
        builder.HasKey(c => c.Id);

        // Guid generado desde el dominio (no por la BD)
        builder.Property(c => c.Id)
               .ValueGeneratedNever();

        // Campos obligatorios
        builder.Property(c => c.Email)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(c => c.Name)
               .IsRequired()
               .HasMaxLength(100);

        // Opcional
        builder.Property(c => c.PhoneNumber)
               .HasMaxLength(20);

        // Reglas útiles (opcionales pero recomendadas)
        builder.HasIndex(c => c.Email).IsUnique();
    }
}
