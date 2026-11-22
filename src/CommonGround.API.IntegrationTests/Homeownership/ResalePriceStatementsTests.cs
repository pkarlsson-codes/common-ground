using System.Net;
using System.Net.Http.Json;
using CommonGround.API.Contracts.Homeownership.Requests;
using CommonGround.API.Contracts.Homeownership.Responses;
using CommonGround.API.Contracts.Today.Responses;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace CommonGround.API.IntegrationTests.Homeownership;

[Collection(IntegrationCollection.Name)]
public class ResalePriceStatementsTests(ApiFactory factory)
{
    [Fact]
    public async Task RecordingAStatement_ListsItOnTheLeaseAndMovesTheNextOneAYearOn()
    {
        var client = factory.CreateClient().WithAuthToken();
        var today = (await client.GetFromJsonAsync<TodayResponse>("/api/today"))!.Today;
        var homeowner = UniqueName();
        var leaseStart = today.AddYears(-3);
        var leaseId = await CreateIndexLease(client, homeowner, leaseStart);

        var due = await client.GetFromJsonAsync<ResalePriceStatementsDueResponse>(
            $"/api/resale-price-statements/due?search={Uri.EscapeDataString(homeowner)}");
        due!.Leases.Should().ContainSingle().Which.Should().BeEquivalentTo(new
        {
            GroundLeaseId = leaseId,
            IndexType = "CPI-U",
            BaselineIndexValue = 290.0m,
            LastStatementDate = (DateOnly?)null,
            NextDueOn = leaseStart.AddYears(1),
        });
        (await client.GetFromJsonAsync<TodayResponse>("/api/today"))!.Items.Should().ContainSingle(i => i.RecordId == leaseId && !i.Kind.StartsWith("insurance-", StringComparison.Ordinal))
            .Which.Should().BeEquivalentTo(new
            {
                Area = "stewardship",
                Kind = "resale-price-statement-due",
                RecordType = "ground-lease",
                DueDate = (DateOnly?)leaseStart.AddYears(1),
                Actions = new[] { "record-resale-price-statement" },
            });

        var preview = await client.PostAsJsonAsync($"/api/ground-leases/{leaseId}/resale-price-statements/preview",
            new PreviewResalePriceStatementRequest(today, null, null, 312.5m, null));
        (await preview.Content.ReadFromJsonAsync<ResalePriceStatementPriceResponse>())!
            .Should().Be(new ResalePriceStatementPriceResponse(today, 3, null, null, 312.5m, null, 193_965.52m));

        var recorded = await client.PostAsJsonAsync($"/api/ground-leases/{leaseId}/resale-price-statements",
            new RecordResalePriceStatementRequest(today, null, null, 312.5m, null, "FirstClassMail"));
        recorded.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var lease = await client.GetFromJsonAsync<GroundLeaseResponse>($"/api/ground-leases/{leaseId}");
        lease!.ResalePriceStatements.Should().BeEquivalentTo(new
        {
            NextDueOn = (DateOnly?)today.AddYears(1),
            ResaleInProgress = false,
            Statements = new[]
            {
                new { StatementDate = today, Delivery = "FirstClassMail", YearsOwned = 3, IndexValue = (decimal?)312.5m, Price = 193_965.52m },
            },
        });
        (await client.GetFromJsonAsync<ResalePriceStatementsDueResponse>(
            $"/api/resale-price-statements/due?search={Uri.EscapeDataString(homeowner)}"))!.Leases.Should().BeEmpty();
        (await client.GetFromJsonAsync<ResalePriceStatementsDueResponse>(
            $"/api/resale-price-statements/due?includeAll=true&search={Uri.EscapeDataString(homeowner)}"))!.Leases
            .Should().ContainSingle().Which.Should().BeEquivalentTo(new { LastStatementDate = (DateOnly?)today, NextDueOn = today.AddYears(1) });
        (await client.GetFromJsonAsync<TodayResponse>("/api/today"))!.Items.Should().NotContain(i => i.RecordId == leaseId && !i.Kind.StartsWith("insurance-", StringComparison.Ordinal));
    }

    [Fact]
    public async Task LeaseStartedLessThanAYearAgo_IsNotDueYet()
    {
        var client = factory.CreateClient().WithAuthToken();
        var today = (await client.GetFromJsonAsync<TodayResponse>("/api/today"))!.Today;
        var homeowner = UniqueName();
        var leaseId = await CreateIndexLease(client, homeowner, today.AddMonths(-6));

        (await client.GetFromJsonAsync<ResalePriceStatementsDueResponse>(
            $"/api/resale-price-statements/due?search={Uri.EscapeDataString(homeowner)}"))!.Leases.Should().BeEmpty();
        (await client.GetFromJsonAsync<GroundLeaseResponse>($"/api/ground-leases/{leaseId}"))!.ResalePriceStatements.NextDueOn
            .Should().Be(today.AddMonths(-6).AddYears(1));
    }

    [Fact]
    public async Task RecordingAStatementDatedTomorrow_Returns422WithItsMessage()
    {
        var client = factory.CreateClient().WithAuthToken();
        var today = (await client.GetFromJsonAsync<TodayResponse>("/api/today"))!.Today;
        var leaseId = await CreateIndexLease(client, UniqueName(), today.AddYears(-2));

        var response = await client.PostAsJsonAsync($"/api/ground-leases/{leaseId}/resale-price-statements",
            new RecordResalePriceStatementRequest(today.AddDays(1), null, null, 312.5m, null, "Email"));

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await response.Content.ReadFromJsonAsync<ProblemDetails>())!.Detail.Should().Be("Statement date can't be in the future.");
    }

    [Fact]
    public async Task RecordingAStatementWithAnUnknownDelivery_Returns400()
    {
        var client = factory.CreateClient().WithAuthToken();
        var today = (await client.GetFromJsonAsync<TodayResponse>("/api/today"))!.Today;
        var leaseId = await CreateIndexLease(client, UniqueName(), today.AddYears(-2));

        var response = await client.PostAsJsonAsync($"/api/ground-leases/{leaseId}/resale-price-statements",
            new RecordResalePriceStatementRequest(today, null, null, 312.5m, null, "Pigeon"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PreviewingARound_PricesEachLeaseAndGivesTheRefusalForOneThatCantBePriced()
    {
        var client = factory.CreateClient().WithAuthToken();
        var today = (await client.GetFromJsonAsync<TodayResponse>("/api/today"))!.Today;
        var priced = await CreateIndexLease(client, UniqueName(), today.AddYears(-2));
        var missingIndex = await CreateIndexLease(client, UniqueName(), today.AddYears(-2));

        var response = await client.PostAsJsonAsync("/api/resale-price-statements/preview",
            new PreviewResalePriceStatementsRequest(today,
            [
                new ResalePriceStatementLeaseFiguresRequest(priced, null, null, 319.0m),
                new ResalePriceStatementLeaseFiguresRequest(missingIndex, null, null, null),
            ]));

        response.EnsureSuccessStatusCode();
        var leases = (await response.Content.ReadFromJsonAsync<ResalePriceStatementPreviewsResponse>())!.Leases;
        leases.Select(l => l.GroundLeaseId).Should().Equal(priced, missingIndex);
        leases[0].Price!.Price.Should().Be(198_000m);
        leases[0].ErrorCode.Should().BeNull();
        leases[1].Price.Should().BeNull();
        leases[1].ErrorCode.Should().Be("homeownership.ground_lease.record_resale_price_statement.index_value_required");
        leases[1].ErrorMessage.Should().Be("Index value on the statement date is required.");
    }

    private static string UniqueName() =>
        "Dana " + string.Concat(Guid.NewGuid().ToString("N").Where(char.IsLetter).Take(8).Select(char.ToUpperInvariant).Select((c, i) => i == 0 ? c : char.ToLowerInvariant(c)));

    private static async Task<string> CreateIndexLease(HttpClient client, string homeowner, DateOnly leaseStart)
    {
        var response = await client.PostAsJsonAsync("/api/ground-leases",
            new CreateGroundLeaseRequest(
                Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), homeowner, "dana@example.com", null,
                leaseStart, 99, 60, 0m, 0m, 180_000m, 250_000m, "Standard covenant", "IndexBased",
                null, "CPI-U", 290.0m, null, null, 90, leaseStart, 0m));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CreateGroundLeaseResponse>())!.GroundLeaseId;
    }
}
