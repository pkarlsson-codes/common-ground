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
public class GroundLeasesTests(ApiFactory factory)
{
    [Fact]
    public async Task GetGroundLeases_ListsNewestFirstWithFormulaKind()
    {
        var client = factory.CreateClient().WithAuthToken();
        var landParcelId = Guid.NewGuid().ToString();
        var older = await CreateLease(client, landParcelId, "AppraisalBased", appreciationShare: 0.25m);
        var newer = await CreateLease(client, landParcelId, "IndexBased", indexType: "CPI", baselineIndexValue: 100m);

        var result = await client.GetFromJsonAsync<PaginatedResponse<GroundLeaseListItemResponse>>(
            $"/api/ground-leases?landParcelId={landParcelId}");

        result!.Items.Select(l => (l.Id, l.FormulaKind)).Should().Equal(
            (newer, "IndexBased"),
            (older, "AppraisalBased"));
    }

    [Fact]
    public async Task CalculateResaleEstimate_IndexBasedWithoutCurrentIndexValue_Returns422()
    {
        var client = factory.CreateClient().WithAuthToken();
        var id = await CreateLease(client, Guid.NewGuid().ToString(), "IndexBased", indexType: "CPI", baselineIndexValue: 100m);

        var response = await client.PostAsJsonAsync($"/api/ground-leases/{id}/resale-estimate",
            new CalculateResaleEstimateRequest(200_000m, 300_000m, 200_000m, 5, null, null));

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem!.Detail.Should().Be("Current index value is required to estimate an index-based resale price.");
    }

    [Fact]
    public async Task GetGroundLease_LeaseCreatedDirectly_ShowsItsHomeownerAndTheDayItWasRecorded()
    {
        var client = factory.CreateClient().WithAuthToken();
        var id = await CreateLease(client, Guid.NewGuid().ToString(), "AppraisalBased", appreciationShare: 0.25m);
        var today = (await client.GetFromJsonAsync<TodayResponse>("/api/today"))!.Today;

        var lease = await client.GetFromJsonAsync<GroundLeaseResponse>($"/api/ground-leases/{id}");

        lease.Should().BeEquivalentTo(new
        {
            LeaseStartDate = new DateOnly(2018, 1, 1),
            RecordedOn = today,
            HomeownerFullName = "Dana Whitfield",
            HomeownerEmail = "dana@example.com",
            HomeownerPhone = "+15035550123",
        });
    }

    [Fact]
    public async Task CreateGroundLease_HomeAlreadyHasAnActiveLease_Returns422()
    {
        var client = factory.CreateClient().WithAuthToken();
        var landParcelId = Guid.NewGuid().ToString();
        var homeId = Guid.NewGuid().ToString();
        await CreateLease(client, landParcelId, "AppraisalBased", appreciationShare: 0.25m, homeId: homeId);

        var response = await PostLease(client, landParcelId, homeId, "AppraisalBased", 0.25m, null, null);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem!.Detail.Should().Be("This home already has an active ground lease.");
    }

    [Fact]
    public async Task CreateGroundLease_HomeHasAnOpenPurchase_Returns422()
    {
        var client = factory.CreateClient().WithAuthToken();
        var landParcelId = Guid.NewGuid().ToString();
        var homeId = Guid.NewGuid().ToString();
        var template = await client.PostAsJsonAsync("/api/ground-lease-templates",
            new CreateGroundLeaseTemplateRequest(
                $"Template {Guid.NewGuid():N}", 99, 60, "Standard covenant terms", "AppraisalBased", 0.25m, null, null, null, null));
        var templateId = (await template.Content.ReadFromJsonAsync<CreateGroundLeaseTemplateResponse>())!.GroundLeaseTemplateId;
        (await client.PostAsJsonAsync("/api/purchase-workflows",
            new InitiatePurchaseWorkflowRequest(templateId, landParcelId, homeId, "Buyer Name", "buyer@example.com", null, 200_000m)))
            .EnsureSuccessStatusCode();

        var response = await PostLease(client, landParcelId, homeId, "AppraisalBased", 0.25m, null, null);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem!.Detail.Should().Be("This home has a purchase in progress. Lease it by closing that purchase, or end the purchase first.");
    }

    private static async Task<string> CreateLease(
        HttpClient client,
        string landParcelId,
        string formulaKind,
        decimal? appreciationShare = null,
        string? indexType = null,
        decimal? baselineIndexValue = null,
        string? homeId = null)
    {
        var response = await PostLease(
            client, landParcelId, homeId ?? Guid.NewGuid().ToString(), formulaKind, appreciationShare, indexType, baselineIndexValue);
        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<CreateGroundLeaseResponse>();
        return created!.GroundLeaseId;
    }

    private static Task<HttpResponseMessage> PostLease(
        HttpClient client,
        string landParcelId,
        string homeId,
        string formulaKind,
        decimal? appreciationShare,
        string? indexType,
        decimal? baselineIndexValue) =>
        client.PostAsJsonAsync("/api/ground-leases",
            new CreateGroundLeaseRequest(
                landParcelId, homeId, "Dana Whitfield", "dana@example.com", "(503) 555-0123",
                new DateOnly(2018, 1, 1), 99, 60, 200_000m, 250_000m, "Standard covenant", formulaKind,
                appreciationShare, indexType, baselineIndexValue, null, null));
}
