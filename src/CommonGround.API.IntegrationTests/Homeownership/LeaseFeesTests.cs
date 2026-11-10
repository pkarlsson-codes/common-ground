using System.Net;
using System.Net.Http.Json;
using CommonGround.API.Contracts.Homeownership.Requests;
using CommonGround.API.Contracts.Homeownership.Responses;
using CommonGround.API.Contracts.Today.Responses;
using CommonGround.SharedKernel.Infrastructure.API;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace CommonGround.API.IntegrationTests.Homeownership;

[Collection(IntegrationCollection.Name)]
public class LeaseFeesTests(ApiFactory factory)
{
    [Fact]
    public async Task RecordAndVoidPayment_ChangesTheBalanceOnTheLeaseAndInTheBalancesList()
    {
        var client = factory.CreateClient().WithAuthToken();
        var today = (await client.GetFromJsonAsync<TodayResponse>("/api/today"))!.Today;
        var thisMonth = new DateOnly(today.Year, today.Month, 1);
        var homeowner = UniqueName();
        var leaseId = await CreateLease(client, homeowner, leaseStart: thisMonth.AddMonths(-2));

        var recorded = await client.PostAsJsonAsync($"/api/ground-leases/{leaseId}/lease-fee-payments",
            new RecordLeaseFeePaymentRequest(today, 60m, "Check", "Check #1001"));
        recorded.StatusCode.Should().Be(HttpStatusCode.Created);
        var paymentId = (await recorded.Content.ReadFromJsonAsync<RecordLeaseFeePaymentResponse>())!.PaymentId;

        var afterPayment = await client.GetFromJsonAsync<GroundLeaseResponse>($"/api/ground-leases/{leaseId}");
        afterPayment!.LeaseFees.Should().BeEquivalentTo(new
        {
            MonthlyFee = 60m,
            Statement = new { TotalCharged = 180m, Balance = 120m, OldestUnpaidDueDate = (DateOnly?)thisMonth.AddMonths(-1) },
        });

        var listed = await client.GetFromJsonAsync<PaginatedResponse<LeaseFeeBalanceResponse>>(
            $"/api/lease-fee-balances?search={Uri.EscapeDataString(homeowner)}");
        listed!.Items.Should().ContainSingle().Which.Balance.Should().Be(120m);

        (await client.PostAsJsonAsync($"/api/ground-leases/{leaseId}/lease-fee-payments/{paymentId}/void",
            new VoidLeaseFeePaymentRequest("Check bounced"))).EnsureSuccessStatusCode();

        var afterVoid = await client.GetFromJsonAsync<GroundLeaseResponse>($"/api/ground-leases/{leaseId}");
        afterVoid!.LeaseFees.Should().BeEquivalentTo(new
        {
            Statement = new { Balance = 180m },
            Payments = new[] { new { PaymentId = paymentId, VoidReason = "Check bounced" } },
        });
    }

    [Fact]
    public async Task RecordPayment_InTheFuture_Returns422()
    {
        var client = factory.CreateClient().WithAuthToken();
        var today = (await client.GetFromJsonAsync<TodayResponse>("/api/today"))!.Today;
        var leaseId = await CreateLease(client, UniqueName(), leaseStart: today.AddYears(-1));

        var response = await client.PostAsJsonAsync($"/api/ground-leases/{leaseId}/lease-fee-payments",
            new RecordLeaseFeePaymentRequest(today.AddDays(1), 60m, "Cash", null));

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem!.Detail.Should().Be("Received date can't be in the future.");
    }

    [Fact]
    public async Task GetLeaseFeeBalances_PaidUpLease_IsListedOnlyWithAllLeases()
    {
        var client = factory.CreateClient().WithAuthToken();
        var today = (await client.GetFromJsonAsync<TodayResponse>("/api/today"))!.Today;
        var homeowner = UniqueName();
        var leaseId = await CreateLease(client, homeowner, leaseStart: new DateOnly(today.Year, today.Month, 1));
        (await client.PostAsJsonAsync($"/api/ground-leases/{leaseId}/lease-fee-payments",
            new RecordLeaseFeePaymentRequest(today, 60m, "BankTransfer", null))).EnsureSuccessStatusCode();
        var search = Uri.EscapeDataString(homeowner);

        var due = await client.GetFromJsonAsync<PaginatedResponse<LeaseFeeBalanceResponse>>($"/api/lease-fee-balances?search={search}");
        var all = await client.GetFromJsonAsync<PaginatedResponse<LeaseFeeBalanceResponse>>($"/api/lease-fee-balances?includeAll=true&search={search}");

        due!.Items.Should().BeEmpty();
        all!.Items.Should().ContainSingle().Which.Balance.Should().Be(0m);
    }

    private static string UniqueName() =>
        "Dana " + string.Concat(Guid.NewGuid().ToString("N").Where(char.IsLetter).Take(8).Select(char.ToUpperInvariant).Select((c, i) => i == 0 ? c : char.ToLowerInvariant(c)));

    private static async Task<string> CreateLease(HttpClient client, string homeowner, DateOnly leaseStart)
    {
        var response = await client.PostAsJsonAsync("/api/ground-leases",
            new CreateGroundLeaseRequest(
                Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), homeowner, "dana@example.com", null,
                leaseStart, 99, 60, 45m, 15m, 200_000m, 250_000m, "Standard covenant", "AppraisalBased",
                0.25m, null, null, null, null));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CreateGroundLeaseResponse>())!.GroundLeaseId;
    }
}
