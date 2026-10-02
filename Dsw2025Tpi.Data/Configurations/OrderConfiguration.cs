using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dsw2025Tpi.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);

        // La fecha se guarda en UTC. datetime2 no guarda la zona horaria, asi que
        // al leerla EF la devuelve como Unspecified y la API la serializaba sin
        // la "Z": el navegador la tomaba como hora local (+3 h en Argentina).
        // La conversion la marca como UTC al leer; el cliente la pasa a su hora.
        builder.Property(o => o.Date)
            .HasConversion(
                date => date.Kind == DateTimeKind.Local ? date.ToUniversalTime() : date,
                date => DateTime.SpecifyKind(date, DateTimeKind.Utc))
            .IsRequired();

        // Value Objects: se guardan como columnas de la propia tabla Orders
        // (ShippingStreet, ShippingCity, ..., BillingStreet, ...), sin tabla
        // ni clave propia.
        builder.ComplexProperty(o => o.ShippingAddress, a => ConfigureAddress(a, "Shipping"));
        builder.ComplexProperty(o => o.BillingAddress, a => ConfigureAddress(a, "Billing"));

        builder.Property(o => o.Notes)
            .HasMaxLength(500);

        builder.Property(o => o.Status)
            .HasConversion<string>()
            .IsRequired();

        // Configurar relación con Customer
        builder.HasOne(o => o.Customer)
            .WithMany()
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureAddress(ComplexPropertyBuilder<Address> address, string prefix)
    {
        address.Property(a => a.Street)
            .HasColumnName($"{prefix}Street")
            .HasMaxLength(Address.StreetMaxLength)
            .IsRequired();

        address.Property(a => a.Number)
            .HasColumnName($"{prefix}Number")
            .HasMaxLength(Address.NumberMaxLength)
            .IsRequired();

        address.Property(a => a.City)
            .HasColumnName($"{prefix}City")
            .HasMaxLength(Address.CityMaxLength)
            .IsRequired();

        address.Property(a => a.Province)
            .HasColumnName($"{prefix}Province")
            .HasMaxLength(Address.ProvinceMaxLength)
            .IsRequired();

        address.Property(a => a.PostalCode)
            .HasColumnName($"{prefix}PostalCode")
            .HasMaxLength(Address.PostalCodeMaxLength)
            .IsRequired();
    }
}
