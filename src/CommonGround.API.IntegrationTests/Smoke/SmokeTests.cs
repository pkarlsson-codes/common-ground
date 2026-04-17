using System.Net;
using FluentAssertions;

namespace CommonGround.API.IntegrationTests.Smoke;

/// <summary>
/// Basic smoke tests that verify the API starts and auth protections are active.
/// </summary>
[Collection("integration")]
public sealed class SmokeTests(ApiFactory factory)
{
    /// <summary>
    /// Verifies that the health endpoint responds with HTTP 200.
    /// </summary>
    /// <returns>A task representing the asynchronous assertion.</returns>
    [Fact]
    public async Task HealthEndpoint_ReturnsHealthy()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Verifies that a protected endpoint rejects anonymous callers.
    /// </summary>
    /// <returns>A task representing the asynchronous assertion.</returns>
    [Fact]
    public async Task AuthenticatedEndpoint_WithNoToken_Returns401()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/test-auth");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
