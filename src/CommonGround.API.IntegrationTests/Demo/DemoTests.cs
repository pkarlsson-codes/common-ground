using System.Net;
using System.Net.Http.Json;
using CommonGround.API.Contracts.Demo.Responses;
using CommonGround.API.Contracts.Identity.Requests;
using CommonGround.API.Contracts.Identity.Responses;
using CommonGround.API.Contracts.TrustManagement.Requests;
using CommonGround.API.Contracts.TrustManagement.Responses;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace CommonGround.API.IntegrationTests.Demo;

[Collection(IntegrationCollection.Name)]
public class DemoTests(ApiFactory factory)
{
    private const string FeedbackUrl = "mailto:demo@example.org";

    [Fact]
    public async Task GetDemo_DemoModeOff_Returns404()
    {
        var client = await ClientInNewTrustAsync(factory);

        var response = await client.GetAsync("/api/demo");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetDemo_TrustCreatedInDemoMode_ReturnsDeleteDateAndFeedbackUrl()
    {
        await using var demo = factory.WithWebHostBuilder(builder => builder.ConfigureAppConfiguration((_, config) =>
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Demo:Enabled"] = "true",
                ["Demo:SandboxDays"] = "14",
                ["Demo:FeedbackUrl"] = FeedbackUrl,
            })));
        var client = await ClientInNewTrustAsync(demo);

        var response = await client.GetAsync("/api/demo");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<DemoResponse>();
        var denver = TimeZoneInfo.FindSystemTimeZoneById("America/Denver");
        var expected = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow.AddDays(14), denver).DateTime);
        body.Should().Be(new DemoResponse(expected, FeedbackUrl));
    }

    private static async Task<HttpClient> ClientInNewTrustAsync(WebApplicationFactory<Program> host)
    {
        var ensured = await (await host.CreateClient().WithAuthToken(trustId: string.Empty).PostAsJsonAsync(
                "/api/users/ensure",
                new EnsureUserRequest($"demo-{Guid.NewGuid():N}", "demo@example.org", "Demo Visitor", TrustId: null)))
            .Content.ReadFromJsonAsync<EnsureUserResponse>();
        var created = await (await host.CreateClient().WithAuthToken(userId: ensured!.UserId, trustId: string.Empty).PostAsJsonAsync(
                "/api/trusts",
                new CreateTrustRequest("Demo Trust", "America/Denver")))
            .Content.ReadFromJsonAsync<CreateTrustResponse>();
        return host.CreateClient().WithAuthToken(userId: ensured.UserId, trustId: created!.TrustId);
    }
}
