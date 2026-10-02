using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dsw2025Tpi.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        // Las reglas del Value Object Address tambien en la base: ninguna escritura
        // por fuera de Address.Create (SQL manual, otro sistema) puede guardar una
        // direccion invalida.
        builder.ToTable("Orders", table =>
        {
            table.HasCheckConstraint("CK_Orders_ShippingAddress", AddressCheckSql("Shipping"));
            table.HasCheckConstraint("CK_Orders_BillingAddress", AddressCheckSql("Billing"));
        });

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

    // Mismas reglas que Address.Create:
    // - calle, altura, ciudad y provincia no pueden quedar en blanco (Create las
    //   recorta con Trim; los largos maximos ya los imponen las columnas);
    // - codigo postal de 4 digitos ("4000") o CPA en mayusculas ("T4000ABC").
    //   Latin1_General_BIN2 compara por codigo de caracter: la intercalacion de
    //   la base (Modern_Spanish_CI_AS) no distingue mayusculas y aceptaria "t4000abc".
    private static string AddressCheckSql(string prefix) =>
        $"LEN(LTRIM(RTRIM([{prefix}Street]))) > 0 " +
        $"AND LEN(LTRIM(RTRIM([{prefix}Number]))) > 0 " +
        $"AND LEN(LTRIM(RTRIM([{prefix}City]))) > 0 " +
        $"AND LEN(LTRIM(RTRIM([{prefix}Province]))) > 0 " +
        $"AND ([{prefix}PostalCode] COLLATE Latin1_General_BIN2 LIKE '[0-9][0-9][0-9][0-9]' " +
        $"OR [{prefix}PostalCode] COLLATE Latin1_General_BIN2 LIKE '[A-Z][0-9][0-9][0-9][0-9][A-Z][A-Z][A-Z]')";

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
