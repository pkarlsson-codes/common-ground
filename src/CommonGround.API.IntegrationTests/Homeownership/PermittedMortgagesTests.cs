using System.Net;
using System.Net.Http.Json;
using CommonGround.API.Contracts.Homeownership.Requests;
using CommonGround.API.Contracts.Homeownership.Responses;
using CommonGround.API.Contracts.Today.Responses;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace CommonGround.API.IntegrationTests.Homeownership;

[Collection(IntegrationCollection.Name)]
public class PermittedMortgagesTests(ApiFactory factory)
{
    [Fact]
    public async Task RecordEditAndPayOff_ShowOnTheLeaseWithCurrentMortgagesFirst()
    {
        var client = factory.CreateClient().WithAuthToken();
        var today = (await client.GetFromJsonAsync<TodayResponse>("/api/today"))!.Today;
        var leaseStart = today.AddYears(-4);
        var leaseId = await CreateLease(client, leaseStart, maxMortgageDebtPercent: 85);

        var first = await Record(client, leaseId, new RecordPermittedMortgageRequest("Columbia Bank", null, 180_000m, leaseStart));
        var second = await Record(client, leaseId,
            new RecordPermittedMortgageRequest("Repair Fund", "PO Box 12\nSalem OR 97301", 15_000m, today.AddYears(-1)));

        (await client.PutAsJsonAsync($"/api/ground-leases/{leaseId}/permitted-mortgages/{first}",
            new EditPermittedMortgageRequest("Mr. Cooper", "8950 Cypress Waters Blvd"))).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await client.PostAsJsonAsync($"/api/ground-leases/{leaseId}/permitted-mortgages/{second}/paid-off",
            new MarkPermittedMortgagePaidOffRequest(today))).StatusCode.Should().Be(HttpStatusCode.NoContent);

        var lease = await client.GetFromJsonAsync<GroundLeaseResponse>($"/api/ground-leases/{leaseId}");
        lease!.Mortgages.Should().BeEquivalentTo(new
        {
            MaxMortgageDebtPercent = 85,
            Permitted = new object[]
            {
                new
                {
                    MortgageId = first, Lender = "Mr. Cooper", NoticeAddress = "8950 Cypress Waters Blvd", OriginalPrincipal = 180_000m,
                    PermittedOn = leaseStart, Source = "Recorded", PaidOffOn = (DateOnly?)null,
                },
                new
                {
                    MortgageId = second, Lender = "Repair Fund", NoticeAddress = "PO Box 12\nSalem OR 97301", OriginalPrincipal = 15_000m,
                    PermittedOn = today.AddYears(-1), Source = "Recorded", PaidOffOn = (DateOnly?)today,
                },
            },
        }, o => o.WithStrictOrdering());
    }

    [Fact]
    public async Task Record_PermittedBeforeTheLeaseStart_Returns422WithMessage()
    {
        var client = factory.CreateClient().WithAuthToken();
        var today = (await client.GetFromJsonAsync<TodayResponse>("/api/today"))!.Today;
        var leaseId = await CreateLease(client, today.AddYears(-1));

        var response = await client.PostAsJsonAsync($"/api/ground-leases/{leaseId}/permitted-mortgages",
            new RecordPermittedMortgageRequest("Bank", null, 100_000m, today.AddYears(-2)));

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await response.Content.ReadFromJsonAsync<ProblemDetails>())!.Detail
            .Should().Be("Permitted on can't be before the lease start date.");
    }

    [Fact]
    public async Task CreateLease_DebtCapOutOfRange_Returns422WithMessage()
    {
        var client = factory.CreateClient().WithAuthToken();

        var response = await PostLease(client, new DateOnly(2020, 1, 1), maxMortgageDebtPercent: 0);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await response.Content.ReadFromJsonAsync<ProblemDetails>())!.Detail
            .Should().Be("Debt cap must be a whole percent from 1 to 200.");
    }

    [Fact]
    public async Task CreateTemplate_WithDebtCap_ShowsItOnTheTemplate()
    {
        var client = factory.CreateClient().WithAuthToken();

        var created = await client.PostAsJsonAsync("/api/ground-lease-templates",
            new CreateGroundLeaseTemplateRequest(
                $"Template {Guid.NewGuid():N}", 99, 60, 45m, 15m, "Covenant", "AppraisalBased", 0.25m, null, null, null, null, 95));
        created.EnsureSuccessStatusCode();
        var id = (await created.Content.ReadFromJsonAsync<CreateGroundLeaseTemplateResponse>())!.GroundLeaseTemplateId;

        var template = await client.GetFromJsonAsync<GroundLeaseTemplateResponse>($"/api/ground-lease-templates/{id}");
        template!.MaxMortgageDebtPercent.Should().Be(95);
    }

    private static async Task<string> Record(HttpClient client, string leaseId, RecordPermittedMortgageRequest request)
    {
        var response = await client.PostAsJsonAsync($"/api/ground-leases/{leaseId}/permitted-mortgages", request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<RecordPermittedMortgageResponse>())!.MortgageId;
    }

    private static async Task<string> CreateLease(HttpClient client, DateOnly leaseStart, int maxMortgageDebtPercent = 90)
    {
        var response = await PostLease(client, leaseStart, maxMortgageDebtPercent);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CreateGroundLeaseResponse>())!.GroundLeaseId;
    }

    private static Task<HttpResponseMessage> PostLease(HttpClient client, DateOnly leaseStart, int maxMortgageDebtPercent) =>
        client.PostAsJsonAsync("/api/ground-leases",
            new CreateGroundLeaseRequest(
                Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), "Dana Whitfield", "dana@example.com", null,
                leaseStart, 99, 60, 45m, 15m, 200_000m, 250_000m, "Standard covenant", "AppraisalBased",
                0.25m, null, null, null, null, maxMortgageDebtPercent, 0m));
}
