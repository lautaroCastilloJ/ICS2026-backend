namespace Dsw2025Tpi.Application.Dtos.Orders;

public sealed record AddressDto(
    string Street,
    string Number,
    string City,
    string Province,
    string PostalCode
);
