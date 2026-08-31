using Dsw2025Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dsw2025Tpi.Data.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.ProductId)
            .IsRequired();

        builder.Property(o => o.ProductName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(o => o.UnitPrice)
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Property(o => o.Quantity)
            .IsRequired();

        builder.Property(o => o.OrderId)
            .IsRequired();

        // Configure the relationship with Order
        builder.HasOne(o => o.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(o => o.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
