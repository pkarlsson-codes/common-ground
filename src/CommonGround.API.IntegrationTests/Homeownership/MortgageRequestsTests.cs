using System.Net;
using System.Net.Http.Json;
using CommonGround.API.Contracts.Homeownership.Requests;
using CommonGround.API.Contracts.Homeownership.Responses;
using CommonGround.API.Contracts.Today.Responses;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace CommonGround.API.IntegrationTests.Homeownership;

[Collection(IntegrationCollection.Name)]
public class MortgageRequestsTests(ApiFactory factory)
{
    // Base price $200,000, original appraisal $250,000, 25% of appreciation, 90% cap: an appraisal of $330,000 gives a
    // purchase option price of $220,000 and a cap of $198,000.
    private const decimal LoanAppraisal = 330_000m;

    [Fact]
    public async Task Refinance_ReceivedAppraisedApprovedAndClosed_PaysOffThePurchaseLoanAndAddsTheNewOne()
    {
        var client = factory.CreateClient().WithAuthToken();
        var today = await Today(client);
        var leaseId = await CreateLease(client, today.AddYears(-4));
        var purchaseLoanId = await RecordPermittedMortgage(client, leaseId, today.AddYears(-4));

        var requestId = await Receive(client, leaseId, Request(today.AddDays(-10), today.AddDays(5), appraisedValue: null));

        var today1 = await client.GetFromJsonAsync<TodayResponse>("/api/today");
        today1!.Items.Should().ContainSingle(i => i.RecordId == leaseId && i.Kind == "mortgage-request-pending")
            .Which.Actions.Should().Equal("record-mortgage-request-appraisal");

        (await client.PostAsJsonAsync($"/api/mortgage-requests/{requestId}/appraisal",
            new RecordMortgageRequestAppraisalRequest(LoanAppraisal, today.AddDays(-3), null, 2_500m)))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        var appraised = await client.GetFromJsonAsync<MortgageRequestResponse>($"/api/mortgage-requests/{requestId}");
        appraised.Should().BeEquivalentTo(new
        {
            Status = "Pending",
            PurchaseOptionPrice = 220_000m,
            MaxMortgageDebtPercent = 90,
            DebtCapAmount = 198_000m,
            DebtPercent = 81.8m,
            OverCap = (bool?)false,
            ClosingCosts = 2_500m,
            ShortNotice = true,
        });

        (await client.PostAsJsonAsync($"/api/mortgage-requests/{requestId}/approve",
            new ApproveMortgageRequestRequest(today, null))).StatusCode.Should().Be(HttpStatusCode.NoContent);

        var closed = await client.PostAsJsonAsync($"/api/mortgage-requests/{requestId}/loan-closed",
            new RecordMortgageLoanClosedRequest(today, [purchaseLoanId], "PO Box 9, Portland OR"));
        closed.StatusCode.Should().Be(HttpStatusCode.OK);
        var newMortgageId = (await closed.Content.ReadFromJsonAsync<RecordMortgageLoanClosedResponse>())!.PermittedMortgageId;

        var lease = await client.GetFromJsonAsync<GroundLeaseResponse>($"/api/ground-leases/{leaseId}");
        lease!.Mortgages.Permitted.Should().BeEquivalentTo(new object[]
        {
            new { MortgageId = newMortgageId, Lender = "Columbia Bank", NoticeAddress = "PO Box 9, Portland OR", OriginalPrincipal = 175_000m, PermittedOn = today, Source = "Request", PaidOffOn = (DateOnly?)null },
            new { MortgageId = purchaseLoanId, PaidOffOn = (DateOnly?)today },
        }, o => o.WithStrictOrdering());

        var requests = await client.GetFromJsonAsync<List<MortgageRequestResponse>>($"/api/ground-leases/{leaseId}/mortgage-requests");
        requests.Should().ContainSingle().Which.Should().BeEquivalentTo(new
        {
            Status = "Closed",
            DecidedOn = (DateOnly?)today,
            ClosedOn = (DateOnly?)today,
            PermittedMortgageId = newMortgageId,
            PaidOffMortgageIds = new[] { purchaseLoanId },
        });
    }

    [Fact]
    public async Task Approve_OverTheCapWithoutReason_Returns422WithMessage()
    {
        var client = factory.CreateClient().WithAuthToken();
        var today = await Today(client);
        var leaseId = await CreateLease(client, today.AddYears(-4));
        var requestId = await Receive(client, leaseId, Request(today.AddDays(-30), today.AddDays(10), totalDebt: 200_000m));

        var response = await client.PostAsJsonAsync($"/api/mortgage-requests/{requestId}/approve", new ApproveMortgageRequestRequest(today, null));

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await response.Content.ReadFromJsonAsync<ProblemDetails>())!.Detail
            .Should().Be("The total debt is above the lease's debt cap. Give a reason for approving it.");
    }

    [Fact]
    public async Task Receive_WhileAnotherRequestIsPending_Returns422WithMessage()
    {
        var client = factory.CreateClient().WithAuthToken();
        var today = await Today(client);
        var leaseId = await CreateLease(client, today.AddYears(-4));
        await Receive(client, leaseId, Request(today.AddDays(-30), today.AddDays(10)));

        var response = await client.PostAsJsonAsync($"/api/ground-leases/{leaseId}/mortgage-requests", Request(today, today.AddDays(30)));

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await response.Content.ReadFromJsonAsync<ProblemDetails>())!.Detail
            .Should().Be("This lease already has a mortgage request waiting for a decision. Decide or withdraw it first.");
    }

    [Fact]
    public async Task ApprovedLoan_PastItsExpectedClosing_ShowsOnTodayUntilWithdrawn()
    {
        var client = factory.CreateClient().WithAuthToken();
        var today = await Today(client);
        var leaseId = await CreateLease(client, today.AddYears(-4));
        var requestId = await Receive(client, leaseId, Request(today.AddDays(-30), today.AddDays(-1)));
        (await client.PostAsJsonAsync($"/api/mortgage-requests/{requestId}/approve",
            new ApproveMortgageRequestRequest(today.AddDays(-20), null))).EnsureSuccessStatusCode();

        var before = await client.GetFromJsonAsync<TodayResponse>("/api/today");
        before!.Items.Should().ContainSingle(i => i.RecordId == leaseId && i.Kind.StartsWith("mortgage-", StringComparison.Ordinal))
            .Which.Should().BeEquivalentTo(new { Kind = "mortgage-loan-closing-unrecorded", WaitingSince = today });

        (await client.PostAsJsonAsync($"/api/mortgage-requests/{requestId}/withdraw",
            new WithdrawMortgageRequestRequest(today, "Rates went up"))).StatusCode.Should().Be(HttpStatusCode.NoContent);

        var after = await client.GetFromJsonAsync<TodayResponse>("/api/today");
        after!.Items.Should().NotContain(i => i.RecordId == leaseId && i.Kind.StartsWith("mortgage-", StringComparison.Ordinal));
    }

    private static async Task<DateOnly> Today(HttpClient client) =>
        (await client.GetFromJsonAsync<TodayResponse>("/api/today"))!.Today;

    private static ReceiveMortgageRequestRequest Request(
        DateOnly receivedOn, DateOnly expectedClosingOn, decimal totalDebt = 180_000m, decimal? appraisedValue = LoanAppraisal) =>
        new(receivedOn, "Columbia Bank", "Refinance", 175_000m, totalDebt, 6.25m, "Fixed", 30, expectedClosingOn,
            null, appraisedValue, appraisedValue is null ? null : receivedOn, null, null);

    private static async Task<string> Receive(HttpClient client, string leaseId, ReceiveMortgageRequestRequest request)
    {
        var response = await client.PostAsJsonAsync($"/api/ground-leases/{leaseId}/mortgage-requests", request);
        response.StatusCode.Should().Be(HttpStatusCode.Created, await response.Content.ReadAsStringAsync());
        return (await response.Content.ReadFromJsonAsync<ReceiveMortgageRequestResponse>())!.MortgageRequestId;
    }

    private static async Task<string> RecordPermittedMortgage(HttpClient client, string leaseId, DateOnly permittedOn)
    {
        var response = await client.PostAsJsonAsync($"/api/ground-leases/{leaseId}/permitted-mortgages",
            new RecordPermittedMortgageRequest("First Bank", null, 180_000m, permittedOn));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<RecordPermittedMortgageResponse>())!.MortgageId;
    }

    private static async Task<string> CreateLease(HttpClient client, DateOnly leaseStart)
    {
        var response = await client.PostAsJsonAsync("/api/ground-leases",
            new CreateGroundLeaseRequest(
                Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), "Dana Whitfield", "dana@example.com", null,
                leaseStart, 99, 60, 45m, 15m, 200_000m, 250_000m, "Standard covenant", "AppraisalBased",
                0.25m, null, null, null, null, 90, leaseStart, 0m, 0m));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CreateGroundLeaseResponse>())!.GroundLeaseId;
    }
}
