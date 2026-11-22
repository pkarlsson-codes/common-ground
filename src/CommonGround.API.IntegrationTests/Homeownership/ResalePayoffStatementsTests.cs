using System.Net;
using System.Net.Http.Json;
using CommonGround.API.Contracts.Homeownership.Requests;
using CommonGround.API.Contracts.Homeownership.Responses;
using CommonGround.API.Contracts.Today.Responses;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace CommonGround.API.IntegrationTests.Homeownership;

[Collection(IntegrationCollection.Name)]
public class ResalePayoffStatementsTests(ApiFactory factory)
{
    [Fact]
    public async Task PrintingAStatementAndClosingWithTheBalance_LeavesTheEndedLeaseAtZero()
    {
        var client = factory.CreateClient().WithAuthToken();
        var today = (await client.GetFromJsonAsync<TodayResponse>("/api/today"))!.Today;
        var thisMonth = new DateOnly(today.Year, today.Month, 1);
        var leaseId = await CreateLease(client, thisMonth.AddMonths(-2));
        var resaleId = await OpenResaleAndExerciseOption(client, leaseId, today);

        var statement = await client.GetFromJsonAsync<PayoffStatementResponse>($"/api/resales/{resaleId}/payoff-statement?goodThrough={today:yyyy-MM-dd}");
        statement.Should().Be(new PayoffStatementResponse(today, 180m, 0m, 0m, 180m, thisMonth.AddMonths(-2)));

        (await client.PostAsJsonAsync($"/api/resales/{resaleId}/payoff-statements", new RecordPayoffStatementRequest(today)))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await client.GetFromJsonAsync<ResaleResponse>($"/api/resales/{resaleId}"))!.PayoffStatements
            .Should().ContainSingle().Which.Should().BeEquivalentTo(new { GoodThrough = today, TotalOwed = 180m });

        (await client.PostAsJsonAsync($"/api/resales/{resaleId}/buyback-closing",
            new RecordBuybackClosedRequest(today, 205_000m, null, true, null, 180m))).StatusCode.Should().Be(HttpStatusCode.NoContent);

        (await client.GetFromJsonAsync<ResaleResponse>($"/api/resales/{resaleId}"))!
            .Should().BeEquivalentTo(new { Status = "Closed", FeesPaidFromProceeds = (decimal?)180m });
        var lease = await client.GetFromJsonAsync<GroundLeaseResponse>($"/api/ground-leases/{leaseId}");
        lease!.LeaseFees.Should().BeEquivalentTo(new
        {
            Statement = new { Balance = 0m },
            Payments = new[] { new { Amount = 180m, Method = "SaleProceeds", Reference = $"Resale {resaleId}" } },
        });
    }

    [Fact]
    public async Task ClosingWithoutFeesPaid_LeavesTheLeaseBalance()
    {
        var client = factory.CreateClient().WithAuthToken();
        var today = (await client.GetFromJsonAsync<TodayResponse>("/api/today"))!.Today;
        var thisMonth = new DateOnly(today.Year, today.Month, 1);
        var leaseId = await CreateLease(client, thisMonth.AddMonths(-2));
        var resaleId = await OpenResaleAndExerciseOption(client, leaseId, today);

        (await client.PostAsJsonAsync($"/api/resales/{resaleId}/buyback-closing",
            new RecordBuybackClosedRequest(today, 205_000m, null, true, null))).StatusCode.Should().Be(HttpStatusCode.NoContent);

        var lease = await client.GetFromJsonAsync<GroundLeaseResponse>($"/api/ground-leases/{leaseId}");
        lease!.LeaseFees.Statement.Balance.Should().Be(180m);
        lease.LeaseFees.Payments.Should().BeEmpty();
    }

    [Fact]
    public async Task OpenResale_ShowsOnTodayUntilTheBuybackClosingIsRecorded()
    {
        var client = factory.CreateClient().WithAuthToken();
        var today = (await client.GetFromJsonAsync<TodayResponse>("/api/today"))!.Today;
        var leaseId = await CreateLease(client, today.AddMonths(-2));
        var resaleId = await OpenResale(client, leaseId, today);

        var undecided = await client.GetFromJsonAsync<TodayResponse>("/api/today");
        undecided!.Items.Should().ContainSingle(i => i.RecordId == resaleId && i.Kind == "resale-decision-due")
            .Which.Should().BeEquivalentTo(new
            {
                RecordType = "resale",
                DueDate = (DateOnly?)today.AddDays(60),
                Actions = new[] { "record-resale-appraisal", "exercise-purchase-option" },
            });

        (await client.PostAsJsonAsync($"/api/resales/{resaleId}/appraisal",
            new RecordResaleAppraisalRequest(270_000m, today, "Hollis", null, null))).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await client.PostAsJsonAsync($"/api/resales/{resaleId}/exercise-option",
            new ExercisePurchaseOptionRequest(today, null))).StatusCode.Should().Be(HttpStatusCode.NoContent);

        var exercised = await client.GetFromJsonAsync<TodayResponse>("/api/today");
        exercised!.Items.Should().NotContain(i => i.RecordId == resaleId && i.Kind == "resale-decision-due");
        exercised.Items.Should().ContainSingle(i => i.RecordId == resaleId && i.Kind == "resale-closing-waiting")
            .Which.Actions.Should().Equal("record-buyback-closing");

        (await client.PostAsJsonAsync($"/api/resales/{resaleId}/buyback-closing",
            new RecordBuybackClosedRequest(today, 205_000m, null, true, null))).StatusCode.Should().Be(HttpStatusCode.NoContent);

        (await client.GetFromJsonAsync<TodayResponse>("/api/today"))!.Items.Should().NotContain(i => i.RecordId == resaleId);
    }

    [Theory]
    [InlineData(-1, "Good Through can't be")]
    [InlineData(61, "Good Through can't be later than")]
    public async Task GoodThroughOutsideTheRange_Returns422WithMessage(int daysFromToday, string messageStart)
    {
        var client = factory.CreateClient().WithAuthToken();
        var today = (await client.GetFromJsonAsync<TodayResponse>("/api/today"))!.Today;
        var leaseId = await CreateLease(client, today.AddMonths(-2));
        var resaleId = await OpenResale(client, leaseId, today);

        var response = await client.GetAsync($"/api/resales/{resaleId}/payoff-statement?goodThrough={today.AddDays(daysFromToday):yyyy-MM-dd}");

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await response.Content.ReadFromJsonAsync<ProblemDetails>())!.Detail.Should().StartWith(messageStart);
    }

    private static async Task<string> OpenResale(HttpClient client, string leaseId, DateOnly today)
    {
        var response = await client.PostAsJsonAsync($"/api/ground-leases/{leaseId}/intent-to-sell",
            new RecordIntentToSellRequest(today, "Phone", null));
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<RecordIntentToSellResponse>())!.ResaleId;
    }

    private static async Task<string> OpenResaleAndExerciseOption(HttpClient client, string leaseId, DateOnly today)
    {
        var resaleId = await OpenResale(client, leaseId, today);
        (await client.PostAsJsonAsync($"/api/resales/{resaleId}/appraisal",
            new RecordResaleAppraisalRequest(270_000m, today, "Hollis", null, null))).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await client.PostAsJsonAsync($"/api/resales/{resaleId}/exercise-option",
            new ExercisePurchaseOptionRequest(today, null))).StatusCode.Should().Be(HttpStatusCode.NoContent);
        return resaleId;
    }

    private static async Task<string> CreateLease(HttpClient client, DateOnly leaseStart)
    {
        var response = await client.PostAsJsonAsync("/api/ground-leases",
            new CreateGroundLeaseRequest(
                Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), $"Homeowner {Guid.NewGuid():N}", "dana@example.com", null,
                leaseStart, 99, 60, 45m, 15m, 200_000m, 250_000m, "Standard covenant", "AppraisalBased",
                0.25m, null, null, null, null, 90, leaseStart, 0m));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CreateGroundLeaseResponse>())!.GroundLeaseId;
    }
}
