using System.Net;
using System.Net.Http.Json;
using CommonGround.API.Contracts.Homeownership.Requests;
using CommonGround.API.Contracts.Homeownership.Responses;
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

    private static async Task<string> CreateLease(
        HttpClient client,
        string landParcelId,
        string formulaKind,
        decimal? appreciationShare = null,
        string? indexType = null,
        decimal? baselineIndexValue = null)
    {
        var response = await client.PostAsJsonAsync("/api/ground-leases",
            new CreateGroundLeaseRequest(
                landParcelId, Guid.NewGuid().ToString(), "2024-01-01", 99, "Standard covenant", formulaKind,
                appreciationShare, indexType, baselineIndexValue, null, null));
        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<CreateGroundLeaseResponse>();
        return created!.GroundLeaseId;
    }
}
