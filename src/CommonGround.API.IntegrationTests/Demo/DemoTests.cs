using System.Net;
using System.Net.Http.Json;
using CommonGround.API.Contracts.Demo.Requests;
using CommonGround.API.Contracts.Demo.Responses;
using CommonGround.API.Demo;
using CommonGround.API.Contracts.Identity.Requests;
using CommonGround.API.Contracts.Identity.Responses;
using CommonGround.API.Contracts.TrustManagement.Requests;
using CommonGround.API.Contracts.TrustManagement.Responses;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CommonGround.API.IntegrationTests.Demo;

[Collection(IntegrationCollection.Name)]
public class DemoTests(ApiFactory factory)
{
    [Fact]
    public async Task GetMode_DemoModeOff_Returns404()
    {
        var response = await factory.CreateClient().GetAsync("/api/demo/mode");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetMode_DemoModeOnAndSignedOut_ReturnsSandboxDays()
    {
        await using var demo = DemoHost(maxSandboxes: 100);

        var response = await demo.CreateClient().GetAsync("/api/demo/mode");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadFromJsonAsync<DemoModeResponse>()).Should().Be(new DemoModeResponse(14));
    }

    [Fact]
    public async Task GetDemo_DemoModeOff_Returns404()
    {
        var client = await ClientInNewTrustAsync(factory);

        var response = await client.GetAsync("/api/demo");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetDemo_TrustCreatedInDemoMode_ReturnsDeleteDate()
    {
        await using var demo = factory.WithWebHostBuilder(builder => builder.ConfigureAppConfiguration((_, config) =>
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Demo:Enabled"] = "true",
                ["Demo:SandboxDays"] = "14",
            })));
        var client = await ClientInNewTrustAsync(demo);

        var response = await client.GetAsync("/api/demo");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<DemoResponse>();
        var denver = TimeZoneInfo.FindSystemTimeZoneById("America/Denver");
        var expected = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow.AddDays(14), denver).DateTime);
        body.Should().Be(new DemoResponse(expected));
    }

    [Fact]
    public async Task GetCurrentSandbox_DemoModeOff_Returns404()
    {
        var client = factory.CreateClient().WithAuthToken(userId: await EnsureUserAsync(factory), trustId: string.Empty);

        var response = await client.GetAsync("/api/demo/sandboxes/current");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateTrust_DemoFull_Returns422DemoFull()
    {
        await using var room = DemoHost(maxSandboxes: 100);
        await ClientInNewTrustAsync(room);
        await using var full = DemoHost(maxSandboxes: 1);
        var client = full.CreateClient().WithAuthToken(userId: await EnsureUserAsync(full), trustId: string.Empty);

        var response = await client.PostAsJsonAsync("/api/trusts", new CreateTrustRequest("Demo Trust", "America/Denver"));

        await ShouldBeDemoFullAsync(response);
    }

    [Fact]
    public async Task StartSandbox_DemoFull_Returns422DemoFull()
    {
        await using var room = DemoHost(maxSandboxes: 100);
        await ClientInNewTrustAsync(room);
        await using var full = DemoHost(maxSandboxes: 1);
        var client = full.CreateClient().WithAuthToken(userId: await EnsureUserAsync(full), trustId: string.Empty);

        var response = await client.PostAsync("/api/demo/sandboxes", null);

        await ShouldBeDemoFullAsync(response);
    }

    [Fact]
    public async Task SendFeedback_DemoModeOff_Returns404()
    {
        var client = await ClientInNewTrustAsync(factory);

        var response = await client.PostAsJsonAsync("/api/demo/feedback", new SendDemoFeedbackRequest("Hello", "/", MayReply: true));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SendFeedback_MayReply_StoresMessagePageAndSignInEmail()
    {
        await using var demo = DemoHost(maxSandboxes: 100);
        var client = await ClientInNewTrustAsync(demo);
        var message = $"  The resale price is wrong {Guid.NewGuid():N}  ";

        var response = await client.PostAsJsonAsync("/api/demo/feedback", new SendDemoFeedbackRequest(message, "/resales", MayReply: true));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var stored = await StoredFeedbackAsync(demo, message.Trim());
        stored.Should().BeEquivalentTo(new { Message = message.Trim(), Page = "/resales", ReplyTo = "demo@example.org" });
        stored.TrustId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task SendFeedback_MayNotReply_StoresNoEmail()
    {
        await using var demo = DemoHost(maxSandboxes: 100);
        var client = await ClientInNewTrustAsync(demo);
        var message = $"No reply needed {Guid.NewGuid():N}";

        var response = await client.PostAsJsonAsync("/api/demo/feedback", new SendDemoFeedbackRequest(message, null, MayReply: false));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await StoredFeedbackAsync(demo, message)).ReplyTo.Should().BeNull();
    }

    [Fact]
    public async Task SendFeedback_BlankMessage_Returns422FeedbackEmpty()
    {
        await using var demo = DemoHost(maxSandboxes: 100);
        var client = await ClientInNewTrustAsync(demo);

        var response = await client.PostAsJsonAsync("/api/demo/feedback", new SendDemoFeedbackRequest("   ", "/", MayReply: true));

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        var problem = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        problem!["code"].ToString().Should().Be("demo.feedback.empty");
    }

    private static async Task<DemoFeedback> StoredFeedbackAsync(WebApplicationFactory<Program> host, string message)
    {
        await using var session = host.Services.GetRequiredService<IDocumentStore>().QuerySession();
        return await session.Query<DemoFeedback>().SingleAsync(f => f.Message == message);
    }

    private static async Task ShouldBeDemoFullAsync(HttpResponseMessage response)
    {
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        var problem = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        problem!["code"].ToString().Should().Be("demo.full");
    }

    private WebApplicationFactory<Program> DemoHost(int maxSandboxes) =>
        factory.WithWebHostBuilder(builder => builder.ConfigureAppConfiguration((_, config) =>
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Demo:Enabled"] = "true",
                ["Demo:MaxSandboxes"] = maxSandboxes.ToString(System.Globalization.CultureInfo.InvariantCulture),
            })));

    private static async Task<string> EnsureUserAsync(WebApplicationFactory<Program> host)
    {
        var ensured = await (await host.CreateClient().WithAuthToken(trustId: string.Empty).PostAsJsonAsync(
                "/api/users/ensure",
                new EnsureUserRequest($"demo-{Guid.NewGuid():N}", "demo@example.org", "Demo Visitor", TrustId: null)))
            .Content.ReadFromJsonAsync<EnsureUserResponse>();
        return ensured!.UserId;
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
