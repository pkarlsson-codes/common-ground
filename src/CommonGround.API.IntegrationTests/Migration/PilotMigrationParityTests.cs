using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CommonGround.API.Contracts.Identity.Requests;
using CommonGround.API.Contracts.Identity.Responses;
using CommonGround.API.Contracts.TrustManagement.Requests;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace CommonGround.API.IntegrationTests.Migration;

public sealed class PilotMigrationParityTests : IClassFixture<MigrationEnabledApiFactory>
{
    private readonly MigrationEnabledApiFactory _factory;

    public PilotMigrationParityTests(MigrationEnabledApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task UsersEnsure_WhenMigrationEnabled_AllowsMissingTenantClaimAndUsesRequestTrustId()
    {
        var client = _factory.CreateClient();
        var token = TestJwtFactory.CreateToken(CommonGround.SharedKernel.Domain.UserId.Generate(), trustId: null);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsJsonAsync(
            "/api/users/ensure",
            new EnsureUserRequest($"pilot-{Guid.NewGuid()}", "pilot@example.com", "Pilot User", ApiFactory.TestTrustId));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<EnsureUserResponse>();
        body.Should().NotBeNull();
        body!.TrustId.Should().Be(ApiFactory.TestTrustId);
    }

    [Fact]
    public async Task TrustsCreate_WhenMigrationEnabled_UsesControllerParityRoute()
    {
        var client = _factory.CreateClient().WithServiceToken();

        var createResponse = await client.PostAsJsonAsync(
            "/api/trusts",
            new CreateTrustRequest($"Pilot Trust {Guid.NewGuid():N}"));

        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var legacyRouteResponse = await client.PostAsJsonAsync(
            "/api/trusts/trust-not-used-in-route",
            new CreateTrustRequest($"Pilot Trust Legacy {Guid.NewGuid():N}"));

        legacyRouteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

public sealed class MigrationEnabledApiFactory : ApiFactory
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            var overrides = new Dictionary<string, string?>
            {
                ["ApiMigration:Enabled"] = "true",
            };

            configurationBuilder.AddInMemoryCollection(overrides);
        });
    }
}
