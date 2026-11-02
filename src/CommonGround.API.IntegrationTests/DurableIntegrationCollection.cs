namespace CommonGround.API.IntegrationTests;

/// <summary>
/// Defines the xUnit collection that shares a single <see cref="DurableApiFactory"/> instance.
/// </summary>
[CollectionDefinition(Name)]
public class DurableIntegrationCollection : ICollectionFixture<DurableApiFactory>
{
	public const string Name = "durable-integration";
}
