using AutoMapper;
using Dsw2025Tpi.Application.Dtos.Orders;
using Dsw2025Tpi.Application.Dtos.Products;
using Dsw2025Tpi.Domain.Entities;

namespace Dsw2025Tpi.Application.Mappings;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        // Product
        CreateMap<Product, ProductResponse>();

        CreateMap<ProductRequest, Product>()
            .ConstructUsing(src => Product.Create(
                src.Sku,
                src.InternalCode,
                src.Name,
                src.Description ?? string.Empty,
                src.CurrentUnitPrice,
                src.StockQuantity,
                src.ImageUrl
            ));

        // Order with custom mapping
        CreateMap<Order, OrderResponse>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items.ToList()))
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.Name : "N/A"));

        CreateMap<OrderItem, OrderItemResponse>();

        // OrderListItemDto mapping
        CreateMap<Order, OrderListItemDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.TotalAmount))
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.Name : "N/A"));
    }
}
