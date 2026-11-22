using System.Net;
using System.Net.Http.Json;
using CommonGround.API.Contracts.Homeownership.Requests;
using CommonGround.API.Contracts.Homeownership.Responses;
using CommonGround.API.Contracts.Today.Responses;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace CommonGround.API.IntegrationTests.Homeownership;

[Collection(IntegrationCollection.Name)]
public class ImprovementRequestsTests(ApiFactory factory)
{
    [Fact]
    public async Task RoofRequest_ReceivedApprovedAndCompleted_LeavesTheDecisionTodayAndCountsTheConfirmedCredit()
    {
        var client = factory.CreateClient().WithAuthToken();
        var today = await Today(client);
        var leaseId = await CreateLease(client, today.AddYears(-2));

        var requestId = await Receive(client, leaseId, today.AddDays(-20));

        var waiting = await client.GetFromJsonAsync<TodayResponse>("/api/today");
        waiting!.Items.Should().ContainSingle(i => i.RecordId == leaseId && i.Kind == "improvement-request-waiting")
            .Which.Should().BeEquivalentTo(new { DueDate = (DateOnly?)today.AddDays(-6), Actions = new[] { "decide-improvement-request" } });

        (await client.PostAsJsonAsync($"/api/improvement-requests/{requestId}/approve",
            new ApproveImprovementRequestRequest(today.AddDays(-5), 15_000m, "Roof only"))).StatusCode.Should().Be(HttpStatusCode.NoContent);

        var approved = await client.GetFromJsonAsync<TodayResponse>("/api/today");
        approved!.Items.Should().NotContain(i => i.RecordId == leaseId && i.Kind == "improvement-request-waiting");

        (await client.PostAsJsonAsync($"/api/improvement-requests/{requestId}/completed",
            new RecordImprovementCompletedRequest(today, 18_000m, ["document-1", "document-2"], 15_000m, null)))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        var requests = await client.GetFromJsonAsync<List<ImprovementRequestResponse>>($"/api/ground-leases/{leaseId}/improvement-requests");
        requests.Should().ContainSingle().Which.Should().BeEquivalentTo(new
        {
            Status = "Completed",
            EstimatedCost = 20_000m,
            PreApprovedCredit = (decimal?)15_000m,
            CreditOnLeaseBefore = (decimal?)0m,
            LeaseImprovementsCap = (decimal?)null,
            ActualCost = (decimal?)18_000m,
            ConfirmedCredit = (decimal?)15_000m,
            ReceiptDocumentIds = new[] { "document-1", "document-2" },
        });

        var lease = await client.GetFromJsonAsync<GroundLeaseResponse>($"/api/ground-leases/{leaseId}");
        lease!.ApprovedImprovements.Should().Be(15_000m);
    }

    [Fact]
    public async Task SecondRequest_ApprovedAfterAFirstCompleted_KeepsTheCreditAlreadyOnTheLease()
    {
        var client = factory.CreateClient().WithAuthToken();
        var today = await Today(client);
        var leaseId = await CreateLease(client, today.AddYears(-2));
        var first = await Receive(client, leaseId, today.AddDays(-30));
        await client.PostAsJsonAsync($"/api/improvement-requests/{first}/approve", new ApproveImprovementRequestRequest(today.AddDays(-29), 5_000m, null));
        await client.PostAsJsonAsync($"/api/improvement-requests/{first}/completed",
            new RecordImprovementCompletedRequest(today.AddDays(-10), 6_000m, ["document-1"], 4_000m, null));
        var second = await Receive(client, leaseId, today.AddDays(-3));

        (await client.PostAsJsonAsync($"/api/improvement-requests/{second}/approve",
            new ApproveImprovementRequestRequest(today, 0m, null))).StatusCode.Should().Be(HttpStatusCode.NoContent);

        var view = await client.GetFromJsonAsync<ImprovementRequestResponse>($"/api/improvement-requests/{second}");
        view!.Should().BeEquivalentTo(new { Status = "Approved", CreditOnLeaseBefore = (decimal?)4_000m, PreApprovedCredit = (decimal?)0m });
    }

    [Fact]
    public async Task Deny_WithoutReason_Returns422WithMessage()
    {
        var client = factory.CreateClient().WithAuthToken();
        var today = await Today(client);
        var leaseId = await CreateLease(client, today.AddYears(-2));
        var requestId = await Receive(client, leaseId, today);

        var response = await client.PostAsJsonAsync($"/api/improvement-requests/{requestId}/deny", new DenyImprovementRequestRequest(today, " "));

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await response.Content.ReadFromJsonAsync<ProblemDetails>())!.Detail.Should().Be("A reason for denying is required.");
    }

    [Fact]
    public async Task Withdraw_WaitingRequest_LeavesToday()
    {
        var client = factory.CreateClient().WithAuthToken();
        var today = await Today(client);
        var leaseId = await CreateLease(client, today.AddYears(-2));
        var requestId = await Receive(client, leaseId, today);

        (await client.PostAsJsonAsync($"/api/improvement-requests/{requestId}/withdraw",
            new WithdrawImprovementRequestRequest(today, "Too costly"))).StatusCode.Should().Be(HttpStatusCode.NoContent);

        var after = await client.GetFromJsonAsync<TodayResponse>("/api/today");
        after!.Items.Should().NotContain(i => i.RecordId == leaseId && i.Kind == "improvement-request-waiting");
    }

    private static async Task<DateOnly> Today(HttpClient client) =>
        (await client.GetFromJsonAsync<TodayResponse>("/api/today"))!.Today;

    private static async Task<string> Receive(HttpClient client, string leaseId, DateOnly receivedOn)
    {
        var response = await client.PostAsJsonAsync($"/api/ground-leases/{leaseId}/improvement-requests",
            new ReceiveImprovementRequestRequest(receivedOn, "Replace the roof", 20_000m));
        response.StatusCode.Should().Be(HttpStatusCode.Created, await response.Content.ReadAsStringAsync());
        return (await response.Content.ReadFromJsonAsync<ReceiveImprovementRequestResponse>())!.ImprovementRequestId;
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
