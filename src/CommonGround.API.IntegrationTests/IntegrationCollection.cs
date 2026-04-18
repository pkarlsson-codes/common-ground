namespace CommonGround.API.IntegrationTests;

/// <summary>
/// Defines the xUnit collection that shares a single <see cref="ApiFactory"/> instance.
/// </summary>
[CollectionDefinition(Name)]
public class IntegrationCollection : ICollectionFixture<ApiFactory>
{
	public const string Name = "integration";
}
