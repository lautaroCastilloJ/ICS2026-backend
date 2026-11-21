namespace Dsw2025Tpi.IntegrationTests;

/// <summary>
/// Todas las clases marcadas con [Collection(IntegrationTestsCollection.Name)]
/// comparten UNA sola CustomWebApplicationFactory (una API y una base) y se
/// ejecutan en serie, sin pisarse los datos entre si.
/// </summary>
[CollectionDefinition(Name)]
public class IntegrationTestsCollection : ICollectionFixture<CustomWebApplicationFactory>
{
    public const string Name = "IntegrationTests";
}
