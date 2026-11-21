using System.Net;
using FluentAssertions;

namespace Dsw2025Tpi.IntegrationTests;

/// <summary>
/// Prueba "dummy" (TP4-01): si /health responde 200, la API completa arranco
/// con la configuracion de pruebas, SQLite reemplazo a SQL Server, el esquema
/// existe y el seeder de Identity corrio sin errores.
/// </summary>
[Collection(IntegrationTestsCollection.Name)]
public class HealthCheckTests
{
    private readonly CustomWebApplicationFactory _factory;

    public HealthCheckTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Health_endpoint_returns_200_without_sql_server()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
