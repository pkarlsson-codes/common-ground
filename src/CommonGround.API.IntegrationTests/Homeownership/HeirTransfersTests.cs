using System.Net;
using System.Net.Http.Json;
using CommonGround.API.Contracts.Homeownership.Requests;
using CommonGround.API.Contracts.Homeownership.Responses;
using CommonGround.API.Contracts.Today.Responses;
using CommonGround.HomeBuyers.Application.ReadModels;
using CommonGround.SharedKernel.Infrastructure.API;
using FluentAssertions;
using Marten;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace CommonGround.API.IntegrationTests.Homeownership;

[Collection(IntegrationCollection.Name)]
public class HeirTransfersTests(ApiFactory factory)
{
    [Fact]
    public async Task SpouseInherits_OpenedRecordedAndCompleted_ChangesTheLeaseHomeownerAndKeepsTheTerms()
    {
        var client = factory.CreateClient().WithAuthToken();
        var today = await Today(client);
        var (leaseId, _, landParcelId) = await CreateLease(client, today.AddYears(-2));
        var before = await client.GetFromJsonAsync<GroundLeaseResponse>($"/api/ground-leases/{leaseId}");

        var transferId = await Open(client, leaseId, today.AddDays(-30), today.AddDays(-10));
        (await RecordSpouse(client, transferId)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        var complete = await client.PostAsJsonAsync($"/api/heir-transfers/{transferId}/complete",
            new CompleteHeirTransferRequest(today.AddDays(-2), "document-1", "document-2", null));
        complete.StatusCode.Should().Be(HttpStatusCode.NoContent, await complete.Content.ReadAsStringAsync());

        var transfer = await client.GetFromJsonAsync<HeirTransferResponse>($"/api/heir-transfers/{transferId}");
        transfer.Should().BeEquivalentTo(new
        {
            Status = "Completed",
            NoticeWasLate = false,
            DeceasedHomeownerFullName = "Dana Whitfield",
            HeirFullName = "Sam Whitfield",
            Relationship = "Spouse",
            LetterOfAgreementDocumentId = "document-1",
            AttorneyAcknowledgementDocumentId = "document-2",
        });
        var after = await client.GetFromJsonAsync<GroundLeaseResponse>($"/api/ground-leases/{leaseId}");
        after.Should().BeEquivalentTo(new { HomeownerFullName = "Sam Whitfield", HomeownerEmail = "sam@example.com", BasePrice = before!.BasePrice });
        after!.PreviousHomeowners.Should().ContainSingle().Which.Should().BeEquivalentTo(new
        {
            FullName = "Dana Whitfield",
            TransferredOn = today.AddDays(-2),
            HeirTransferId = transferId,
        });
        var list = await client.GetFromJsonAsync<PaginatedResponse<GroundLeaseListItemResponse>>($"/api/ground-leases?landParcelId={landParcelId}");
        list!.Items.Single(l => l.Id == leaseId).HomeownerFullName.Should().Be("Sam Whitfield");
    }

    [Fact]
    public async Task Open_WhenTheLeaseAlreadyHasAnOpenTransfer_Returns422WithMessage()
    {
        var client = factory.CreateClient().WithAuthToken();
        var today = await Today(client);
        var (leaseId, _, landParcelId) = await CreateLease(client, today.AddYears(-2));
        await Open(client, leaseId, today.AddDays(-30), today.AddDays(-10));

        var response = await client.PostAsJsonAsync($"/api/ground-leases/{leaseId}/heir-transfers",
            new OpenHeirTransferRequest(today.AddDays(-30), today.AddDays(-10), "Pat Whitfield", null, null));

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await response.Content.ReadFromJsonAsync<ProblemDetails>())!.Detail.Should().Be("This lease already has an open transfer to an heir. Complete or close it first.");
    }

    [Fact]
    public async Task Open_NoticeAfterNinetyDays_IsRecordedAsLate()
    {
        var client = factory.CreateClient().WithAuthToken();
        var today = await Today(client);
        var (leaseId, _, landParcelId) = await CreateLease(client, today.AddYears(-2));

        var transferId = await Open(client, leaseId, today.AddDays(-100), today);

        var transfer = await client.GetFromJsonAsync<HeirTransferResponse>($"/api/heir-transfers/{transferId}");
        transfer.Should().BeEquivalentTo(new { Status = "Open", NoticeWasLate = true, NoticeDueOn = today.AddDays(-10) });
    }

    [Fact]
    public async Task OtherHeirOverTheHomesLimit_IsRefusedAndTheTransferClosesAsNotQualified()
    {
        var client = factory.CreateClient().WithAuthToken();
        var today = await Today(client);
        var (leaseId, homeId, _) = await CreateLease(client, today.AddYears(-2));
        await StoreHomeWithLimit(homeId, 80);
        var transferId = await Open(client, leaseId, today.AddDays(-30), today.AddDays(-10));

        var limit = await client.GetFromJsonAsync<HeirIncomeLimitResponse>($"/api/heir-transfers/{transferId}/income-limit");
        var refused = await RecordOther(client, transferId, income: 85_000m, householdSize: 2);
        var close = await client.PostAsJsonAsync($"/api/heir-transfers/{transferId}/close", new CloseHeirTransferRequest("HeirNotQualified", "Over the limit"));

        limit.Should().BeEquivalentTo(new { PercentOfAmi = (int?)80, Limits = TestIncomeLimitsInitialData.EightyPercentLimits });
        refused.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await refused.Content.ReadFromJsonAsync<ProblemDetails>())!.Detail.Should().StartWith("The heir's income is over the home's income limit");
        close.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var transfer = await client.GetFromJsonAsync<HeirTransferResponse>($"/api/heir-transfers/{transferId}");
        transfer.Should().BeEquivalentTo(new { Status = "Closed", ClosedReason = "HeirNotQualified", ClosedNotes = "Over the limit", HeirFullName = (string?)null });
        var lease = await client.GetFromJsonAsync<GroundLeaseResponse>($"/api/ground-leases/{leaseId}");
        lease!.HomeownerFullName.Should().Be("Dana Whitfield");
        lease.PreviousHomeowners.Should().BeEmpty();
    }

    [Fact]
    public async Task OtherHeirUnderTheHomesLimit_IsRecordedWithTheLimitTheyWereCheckedAgainst()
    {
        var client = factory.CreateClient().WithAuthToken();
        var today = await Today(client);
        var (leaseId, homeId, _) = await CreateLease(client, today.AddYears(-2));
        await StoreHomeWithLimit(homeId, 80);
        var transferId = await Open(client, leaseId, today.AddDays(-30), today.AddDays(-10));

        var recorded = await RecordOther(client, transferId, income: 80_000m, householdSize: 2);

        recorded.StatusCode.Should().Be(HttpStatusCode.NoContent, await recorded.Content.ReadAsStringAsync());
        var transfer = await client.GetFromJsonAsync<HeirTransferResponse>($"/api/heir-transfers/{transferId}");
        transfer.Should().BeEquivalentTo(new
        {
            Status = "Heir recorded",
            Relationship = "Other",
            HeirVerifiedAnnualIncome = (decimal?)80_000m,
            IncomeLimit = (decimal?)80_000m,
            IncomeLimitPercentOfAmi = (int?)80,
            IncomeDocumentIds = new[] { "income-1" },
        });
    }

    [Fact]
    public async Task Complete_WhileTheLeaseHasAnOpenResale_Returns422AndChangesNothing()
    {
        var client = factory.CreateClient().WithAuthToken();
        var today = await Today(client);
        var (leaseId, _, landParcelId) = await CreateLease(client, today.AddYears(-2));
        var transferId = await Open(client, leaseId, today.AddDays(-30), today.AddDays(-10));
        await RecordSpouse(client, transferId);
        (await client.PostAsJsonAsync($"/api/ground-leases/{leaseId}/intent-to-sell", new RecordIntentToSellRequest(today, "Phone", null)))
            .StatusCode.Should().Be(HttpStatusCode.Created);

        var response = await client.PostAsJsonAsync($"/api/heir-transfers/{transferId}/complete",
            new CompleteHeirTransferRequest(today, "document-1", "document-2", null));

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await response.Content.ReadFromJsonAsync<ProblemDetails>())!.Detail.Should().Be("The home has a resale in progress. Close the transfer or finish the resale first.");
        (await client.GetFromJsonAsync<HeirTransferResponse>($"/api/heir-transfers/{transferId}"))!.Status.Should().Be("Heir recorded");
        (await client.GetFromJsonAsync<GroundLeaseResponse>($"/api/ground-leases/{leaseId}"))!.HomeownerFullName.Should().Be("Dana Whitfield");
    }

    [Fact]
    public async Task Complete_WithoutALetter_Returns422WithMessage()
    {
        var client = factory.CreateClient().WithAuthToken();
        var today = await Today(client);
        var (leaseId, _, landParcelId) = await CreateLease(client, today.AddYears(-2));
        var transferId = await Open(client, leaseId, today.AddDays(-30), today.AddDays(-10));
        await RecordSpouse(client, transferId);

        var response = await client.PostAsJsonAsync($"/api/heir-transfers/{transferId}/complete",
            new CompleteHeirTransferRequest(today, "document-1", " ", null));

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await response.Content.ReadFromJsonAsync<ProblemDetails>())!.Detail.Should()
            .Be("The signed Letter of Agreement and the signed Attorney's Acknowledgment are both required.");
    }

    private async Task StoreHomeWithLimit(string homeId, int percentOfAmi)
    {
        using var scope = factory.Services.CreateScope();
        await using var session = scope.ServiceProvider.GetRequiredService<IDocumentStore>().LightweightSession(ApiFactory.TestTrustId);
        session.Store(new HomeFactsView { Id = homeId, Label = "Heir Lane", IncomeLimitPercentOfAmi = percentOfAmi });
        await session.SaveChangesAsync();
    }

    private static async Task<DateOnly> Today(HttpClient client) =>
        (await client.GetFromJsonAsync<TodayResponse>("/api/today"))!.Today;

    private static async Task<string> Open(HttpClient client, string leaseId, DateOnly diedOn, DateOnly noticeOn)
    {
        var response = await client.PostAsJsonAsync($"/api/ground-leases/{leaseId}/heir-transfers",
            new OpenHeirTransferRequest(diedOn, noticeOn, "Pat Whitfield", "415-555-0101", null));
        response.StatusCode.Should().Be(HttpStatusCode.Created, await response.Content.ReadAsStringAsync());
        return (await response.Content.ReadFromJsonAsync<OpenHeirTransferResponse>())!.HeirTransferId;
    }

    private static Task<HttpResponseMessage> RecordSpouse(HttpClient client, string transferId) =>
        client.PostAsJsonAsync($"/api/heir-transfers/{transferId}/heir",
            new RecordHeirRequest("Sam Whitfield", "sam@example.com", null, "Spouse", null, null, null, null));

    private static Task<HttpResponseMessage> RecordOther(HttpClient client, string transferId, decimal income, int householdSize) =>
        client.PostAsJsonAsync($"/api/heir-transfers/{transferId}/heir",
            new RecordHeirRequest("Nia Okoye", "nia@example.com", null, "Other", null, income, householdSize, ["income-1"]));

    private static async Task<(string LeaseId, string HomeId, string LandParcelId)> CreateLease(HttpClient client, DateOnly leaseStart)
    {
        var homeId = Guid.NewGuid().ToString();
        var landParcelId = Guid.NewGuid().ToString();
        var response = await client.PostAsJsonAsync("/api/ground-leases",
            new CreateGroundLeaseRequest(
                landParcelId, homeId, "Dana Whitfield", "dana@example.com", null,
                leaseStart, 99, 60, 45m, 15m, 200_000m, 250_000m, "Standard covenant", "AppraisalBased",
                0.25m, null, null, null, null, 90, leaseStart, 0m, 0m));
        response.EnsureSuccessStatusCode();
        return ((await response.Content.ReadFromJsonAsync<CreateGroundLeaseResponse>())!.GroundLeaseId, homeId, landParcelId);
    }
}
