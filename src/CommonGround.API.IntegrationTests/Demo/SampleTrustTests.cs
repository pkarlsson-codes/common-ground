using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using CommonGround.API.Contracts.Demo.Responses;
using CommonGround.API.Contracts.Identity.Requests;
using CommonGround.API.Contracts.Identity.Responses;
using CommonGround.API.Contracts.TrustManagement.Responses;
using FluentAssertions;
using Xunit.Abstractions;

namespace CommonGround.API.IntegrationTests.Demo;

/// <summary>A visitor with no trust starts a sample trust and ends up a member of a filled trust.</summary>
[Collection(DemoIntegrationCollection.Name)]
public class SampleTrustTests(DemoApiFactory factory, ITestOutputHelper output)
{
    [Fact]
    public async Task StartSandbox_VisitorWithNoTrust_FillsSampleTrustAndMakesVisitorAMember()
    {
        var userId = await EnsureUserAsync();
        var client = factory.CreateClient().WithAuthToken(userId: userId, trustId: string.Empty);

        var elapsed = Stopwatch.StartNew();
        var started = await (await client.PostAsync("/api/demo/sandboxes", null)).Content.ReadFromJsonAsync<DemoSandboxResponse>();
        started!.Status.Should().Be("Seeding");

        var again = await (await client.PostAsync("/api/demo/sandboxes", null)).Content.ReadFromJsonAsync<DemoSandboxResponse>();
        again.Should().Be(started, "a second click while the trust is filled gets the same sandbox");

        DemoSandboxResponse? current;
        do
        {
            await Task.Delay(TimeSpan.FromMilliseconds(250));
            current = (await client.GetFromJsonAsync<CurrentDemoSandboxResponse>("/api/demo/sandboxes/current"))!.Sandbox;
            elapsed.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(60));
        }
        while (current?.Status == "Seeding");

        output.WriteLine($"Sample trust ready after {elapsed.Elapsed.TotalSeconds:0.0} s");
        current.Should().Be(started with { Status = "Ready" });

        var member = factory.CreateClient().WithAuthToken(userId: userId, trustId: started.TrustId);
        var trust = await member.GetFromJsonAsync<TrustResponse>("/api/trusts/current");
        trust!.Name.Should().Be("Sample Land Trust");

        var demo = await member.GetFromJsonAsync<DemoResponse>("/api/demo");
        demo!.FeedbackUrl.Should().Be(DemoApiFactory.FeedbackUrl);

        var homes = await member.GetAsync("/api/homes");
        homes.StatusCode.Should().Be(HttpStatusCode.OK);
        (await homes.Content.ReadAsStringAsync()).Length.Should().BeGreaterThan(100, "the sample trust has homes");
    }

    [Fact]
    public async Task GetCurrentSandbox_VisitorWithNone_ReturnsNoSandbox()
    {
        var client = factory.CreateClient().WithAuthToken(userId: await EnsureUserAsync(), trustId: string.Empty);

        var current = await client.GetFromJsonAsync<CurrentDemoSandboxResponse>("/api/demo/sandboxes/current");

        current!.Sandbox.Should().BeNull();
    }

    private async Task<string> EnsureUserAsync()
    {
        var response = await factory.CreateClient().WithAuthToken(trustId: string.Empty).PostAsJsonAsync(
            "/api/users/ensure", new EnsureUserRequest($"sample-{Guid.NewGuid():N}", $"{Guid.NewGuid():N}@example.org", "Sample Visitor", TrustId: null));
        return (await response.Content.ReadFromJsonAsync<EnsureUserResponse>())!.UserId;
    }
}
