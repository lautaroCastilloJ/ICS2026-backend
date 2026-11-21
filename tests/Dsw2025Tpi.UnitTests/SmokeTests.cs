using FluentAssertions;

namespace Dsw2025Tpi.UnitTests;

/// <summary>
/// Prueba "dummy" (TP4-01): sin logica de negocio. Solo verifica que el
/// proyecto se descubre, compila con sus paquetes y se ejecuta.
/// </summary>
public class SmokeTests
{
    [Fact]
    public void Unit_test_project_is_discovered_and_runs()
    {
        // Arrange
        var expected = 2;

        // Act
        var result = 1 + 1;

        // Assert
        result.Should().Be(expected);
    }
}
