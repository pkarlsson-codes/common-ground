using System.Net.Http.Json;
using CommonGround.API.Contracts.Identity.Requests;
using CommonGround.API.Contracts.Identity.Responses;
using CommonGround.API.Infrastructure.Seeding;
using CommonGround.SharedKernel.Domain.Events;
using CommonGround.SharedKernel.Infrastructure.Seeding;
using FluentAssertions;
using Marten;
using Microsoft.Extensions.DependencyInjection;

namespace CommonGround.API.IntegrationTests.Seeding;

/// <summary>
/// Seed runs fill new trusts inside the running API. Runs on the durable host, so the seeders' events reach
/// the other contexts' handlers as they do in the API.
/// </summary>
[Collection(DurableIntegrationCollection.Name)]
public class SeedRunTests(DurableApiFactory factory)
{
    [Fact]
    public async Task TwoRunsAtOnce_FillTwoTrustsWithTheSameDatedHistory_WhileARequestUsesTheWallClock()
    {
        var runner = factory.Services.GetRequiredService<SeedRunner>();
        var first = runner.RunAsync(new SeedRun($"first-{Guid.NewGuid():N}"), CancellationToken.None);
        var second = runner.RunAsync(new SeedRun($"second-{Guid.NewGuid():N}"), CancellationToken.None);

        var requestedAt = DateTimeOffset.UtcNow;
        var userId = await EnsureUserAsync();
        var registeredAt = await OccurredAtOfFirstEventAsync(userId);

        var trusts = await Task.WhenAll(first, second);

        registeredAt.Should().BeCloseTo(requestedAt, TimeSpan.FromMinutes(1), "seeding moves time only for its own call chain");
        var firstTrust = trusts[0].Should().ContainKey("dev-default").WhoseValue;
        var secondTrust = trusts[1].Should().ContainKey("dev-default").WhoseValue;
        firstTrust.Should().NotBe(secondTrust);

        var firstHistory = await DatedHistoryAsync(firstTrust);
        firstHistory.Should().HaveCountGreaterThan(100);
        firstHistory.Should().Contain(e => e.Date < DateOnly.FromDateTime(requestedAt.UtcDateTime.AddDays(-30)), "seeded records are backdated");
        (await DatedHistoryAsync(secondTrust)).Should().Equal(firstHistory);
    }

    private async Task<List<(string Type, DateOnly Date)>> DatedHistoryAsync(string trustId)
    {
        await using var session = factory.Services.GetRequiredService<IDocumentStore>().QuerySession(trustId);
        var events = await session.Events.QueryAllRawEvents().ToListAsync();
        return [.. events
            .Select(e => (Type: e.EventTypeName, Date: DateOnly.FromDateTime(((IDomainEvent)e.Data).OccurredAt.UtcDateTime)))
            .OrderBy(e => e.Type, StringComparer.Ordinal)
            .ThenBy(e => e.Date)];
    }

    private async Task<DateTimeOffset> OccurredAtOfFirstEventAsync(string streamId)
    {
        await using var session = factory.Services.GetRequiredService<IDocumentStore>().QuerySession();
        var events = await session.Events.FetchStreamAsync(streamId);
        return ((IDomainEvent)events[0].Data).OccurredAt;
    }

    private async Task<string> EnsureUserAsync()
    {
        var response = await factory.CreateClient().WithAuthToken(trustId: string.Empty).PostAsJsonAsync(
            "/api/users/ensure", new EnsureUserRequest($"seed-run-{Guid.NewGuid():N}", $"{Guid.NewGuid():N}@example.org", "Seed Run Bystander", TrustId: null));
        return (await response.Content.ReadFromJsonAsync<EnsureUserResponse>())!.UserId;
    }
}
