using Dsw2025Tpi.Domain.Exceptions.AddressExceptions;
using System.Text.RegularExpressions;

namespace Dsw2025Tpi.Domain.ValueObjects;

/// <summary>
/// Value Object que representa una direccion postal.
///
/// - Sin identidad: dos direcciones con los mismos datos son la misma direccion
///   (el record aporta la igualdad por valor).
/// - Inmutable: para "cambiar" una direccion se crea otra.
/// - Siempre valida: la unica forma de obtener una instancia es <see cref="Create"/>.
/// </summary>
public sealed record Address
{
    public const int StreetMaxLength = 150;
    public const int NumberMaxLength = 10;
    public const int CityMaxLength = 100;
    public const int ProvinceMaxLength = 100;
    public const int PostalCodeMaxLength = 8;

    // CP argentino: 4 digitos ("4000") o CPA de 8 caracteres ("T4000ABC").
    private static readonly Regex PostalCodePattern =
        new(@"^(\d{4}|[A-Z]\d{4}[A-Z]{3})$", RegexOptions.Compiled);

    public string Street { get; private init; } = default!;
    public string Number { get; private init; } = default!;
    public string City { get; private init; } = default!;
    public string Province { get; private init; } = default!;
    public string PostalCode { get; private init; } = default!;

    private Address() { } // Ctor para EF Core

    public static Address Create(string street, string number, string city, string province, string postalCode)
    {
        var normalizedPostalCode = Normalize(postalCode).ToUpperInvariant();

        if (!PostalCodePattern.IsMatch(normalizedPostalCode))
            throw new InvalidAddressException("ADDRESS_INVALID_POSTAL_CODE");

        return new Address
        {
            Street = Required(street, StreetMaxLength, "ADDRESS_INVALID_STREET"),
            Number = Required(number, NumberMaxLength, "ADDRESS_INVALID_NUMBER"),
            City = Required(city, CityMaxLength, "ADDRESS_INVALID_CITY"),
            Province = Required(province, ProvinceMaxLength, "ADDRESS_INVALID_PROVINCE"),
            PostalCode = normalizedPostalCode
        };
    }

    public override string ToString()
        => $"{Street} {Number}, {City}, {Province} (CP {PostalCode})";

    private static string Required(string? value, int maxLength, string errorCode)
    {
        var normalized = Normalize(value);

        if (normalized.Length == 0 || normalized.Length > maxLength)
            throw new InvalidAddressException(errorCode);

        return normalized;
    }

    private static string Normalize(string? value) => value?.Trim() ?? string.Empty;
}
