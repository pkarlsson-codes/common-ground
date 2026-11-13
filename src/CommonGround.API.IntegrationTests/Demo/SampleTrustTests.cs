using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using CommonGround.API.Contracts.Demo.Responses;
using CommonGround.API.Contracts.Documents.Requests;
using CommonGround.API.Contracts.Documents.Responses;
using CommonGround.API.Contracts.Identity.Requests;
using CommonGround.API.Contracts.Identity.Responses;
using CommonGround.API.Contracts.TrustManagement.Responses;
using CommonGround.API.Demo;
using CommonGround.API.IntegrationTests.Documents;
using CommonGround.Documents.Application.ReadModels;
using CommonGround.Documents.Domain.Documents;
using FluentAssertions;
using Marten;
using Marten.Events;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Wolverine.Persistence.Durability.ScheduledMessageManagement;
using Wolverine.Runtime;
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
        var started = await StartSampleTrustAsync(client);

        output.WriteLine($"Sample trust ready after {elapsed.Elapsed.TotalSeconds:0.0} s");

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

    [Fact]
    public async Task DeleteDue_SandboxPastItsDate_DeletesEveryRecordFileAndScheduledMessageAndRefusesTheMember()
    {
        var userId = await EnsureUserAsync();
        var trustId = (await StartSampleTrustAsync(factory.CreateClient().WithAuthToken(userId: userId, trustId: string.Empty))).TrustId;
        var member = factory.CreateClient().WithAuthToken(userId: userId, trustId: trustId);
        var blobKey = await UploadDocumentAsync(member, trustId);
        var store = factory.Services.GetRequiredService<IDocumentStore>();
        var blobs = (InMemoryDocumentBlobStore)factory.Services.GetRequiredService<IDocumentBlobStore>();
        await store.WaitForNonStaleProjectionDataAsync(TimeSpan.FromSeconds(30));
        (await CountRowsAsync(store, trustId, userId)).Should().BeGreaterThan(0, "the sample trust has records");
        (await ScheduledMessagesAsync(trustId)).Should().NotBeEmpty("the sample households have prequalifications that expire later");

        await MakeDueAsync(store, trustId);
        int deleted;
        using (var scope = factory.Services.CreateScope())
        {
            deleted = await scope.ServiceProvider.GetRequiredService<DemoSandboxDeletion>().DeleteDueAsync(CancellationToken.None);
        }

        await store.WaitForNonStaleProjectionDataAsync(TimeSpan.FromSeconds(30));
        deleted.Should().Be(1);
        (await CountRowsAsync(store, trustId, userId)).Should().Be(0);
        blobs.Contains(blobKey).Should().BeFalse();
        (await ScheduledMessagesAsync(trustId)).Should().BeEmpty();
        (await member.GetAsync("/api/homes")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        var current = await factory.CreateClient().WithAuthToken(userId: userId, trustId: string.Empty)
            .GetFromJsonAsync<CurrentDemoSandboxResponse>("/api/demo/sandboxes/current");
        current!.Sandbox.Should().BeNull("the visitor can start a new sandbox");
    }

    [Fact]
    public async Task DeleteAsync_SandboxWhoseTrustWasNeverCreated_DeletesTheRecord()
    {
        var store = factory.Services.GetRequiredService<IDocumentStore>();
        var sandbox = DemoSandbox.Start($"trust-never{Guid.NewGuid():N}", "user-1", DateTimeOffset.UtcNow.AddDays(-15), sandboxDays: 14, DemoSandboxStatus.Ready);
        await using (var session = store.LightweightSession())
        {
            session.Store(sandbox);
            await session.SaveChangesAsync();
        }

        using (var scope = factory.Services.CreateScope())
        {
            await scope.ServiceProvider.GetRequiredService<DemoSandboxDeletion>().DeleteAsync(sandbox, CancellationToken.None);
        }

        await using var query = store.QuerySession();
        (await query.LoadAsync<DemoSandbox>(sandbox.Id)).Should().BeNull();
    }

    private static async Task<DemoSandboxResponse> StartSampleTrustAsync(HttpClient visitor)
    {
        var elapsed = Stopwatch.StartNew();
        var started = await (await visitor.PostAsync("/api/demo/sandboxes", null)).Content.ReadFromJsonAsync<DemoSandboxResponse>();
        started!.Status.Should().Be("Seeding");

        var again = await (await visitor.PostAsync("/api/demo/sandboxes", null)).Content.ReadFromJsonAsync<DemoSandboxResponse>();
        again.Should().Be(started, "a second click while the trust is filled gets the same sandbox");

        DemoSandboxResponse? current;
        do
        {
            await Task.Delay(TimeSpan.FromMilliseconds(250));
            current = (await visitor.GetFromJsonAsync<CurrentDemoSandboxResponse>("/api/demo/sandboxes/current"))!.Sandbox;
            elapsed.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(60));
        }
        while (current?.Status == "Seeding");

        current.Should().Be(started with { Status = "Ready" });
        return started;
    }

    private async Task<string> UploadDocumentAsync(HttpClient member, string trustId)
    {
        using var form = new MultipartFormDataContent();
        form.Add(JsonContent.Create(new UploadDocumentMetadata("Sample deed", "Internal", "Permanent", null)), "metadata");
        var file = new ByteArrayContent(Encoding.UTF8.GetBytes("sample deed"));
        file.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
        form.Add(file, "file", "deed.txt");
        var upload = await (await member.PostAsync("/api/documents", form)).Content.ReadFromJsonAsync<UploadDocumentResponse>();

        await using var session = factory.Services.GetRequiredService<IDocumentStore>().QuerySession(trustId);
        var document = await session.LoadAsync<DocumentView>(upload!.DocumentId);
        return document!.Versions.Single().BlobKey;
    }

    private static async Task MakeDueAsync(IDocumentStore store, string trustId)
    {
        await using var session = store.LightweightSession();
        var sandbox = (await session.LoadAsync<DemoSandbox>(trustId))!;
        sandbox.DeletesAt = DateTimeOffset.UtcNow.AddMinutes(-1);
        session.Store(sandbox);
        await session.SaveChangesAsync();
    }

    // Counts rows in every Marten table that mention the trust. The owner's own event stream keeps the record
    // that they joined and left it.
    private static async Task<long> CountRowsAsync(IDocumentStore store, string trustId, string ownerUserId)
    {
        await using var connection = store.Storage.Database.CreateConnection();
        await connection.OpenAsync();

        var tables = new List<string>();
        await using (var list = new NpgsqlCommand(
            "select quote_ident(table_schema) || '.' || quote_ident(table_name) from information_schema.tables where table_type = 'BASE TABLE' and table_name like 'mt\\_%'",
            connection))
        await using (var reader = await list.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                tables.Add(reader.GetString(0));
            }
        }

        long total = 0;
        foreach (var table in tables)
        {
            var ownerStream = table.EndsWith(".mt_events", StringComparison.Ordinal) ? " and t.stream_id <> @owner" : string.Empty;
            await using var count = new NpgsqlCommand($"select count(*) from {table} t where t::text like @pattern{ownerStream}", connection);
            count.Parameters.AddWithValue("pattern", $"%{trustId}%");
            count.Parameters.AddWithValue("owner", ownerUserId);
            total += (long)(await count.ExecuteScalarAsync())!;
        }

        return total;
    }

    private async Task<IReadOnlyList<ScheduledMessageSummary>> ScheduledMessagesAsync(string trustId)
    {
        var scheduled = factory.Services.GetRequiredService<IWolverineRuntime>().Storage.ScheduledMessages;
        var results = await scheduled.QueryAsync(new ScheduledMessageQuery { PageSize = 1000 }, CancellationToken.None);
        return results.Messages.Where(m => m.TenantId == trustId).ToList();
    }

    private async Task<string> EnsureUserAsync()
    {
        var response = await factory.CreateClient().WithAuthToken(trustId: string.Empty).PostAsJsonAsync(
            "/api/users/ensure", new EnsureUserRequest($"sample-{Guid.NewGuid():N}", $"{Guid.NewGuid():N}@example.org", "Sample Visitor", TrustId: null));
        return (await response.Content.ReadFromJsonAsync<EnsureUserResponse>())!.UserId;
    }
}
