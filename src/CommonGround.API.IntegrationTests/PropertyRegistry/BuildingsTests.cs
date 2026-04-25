using System.Net;
using System.Net.Http.Json;
using CommonGround.API.Contracts.PropertyRegistry.Requests;
using CommonGround.API.Contracts.PropertyRegistry.Responses;
using CommonGround.SharedKernel.Infrastructure.API;
using FluentAssertions;

namespace CommonGround.API.IntegrationTests.PropertyRegistry;

[Collection(IntegrationCollection.Name)]
public class BuildingsTests(ApiFactory factory)
{
    // 401 tests — use fake IDs; auth fires before resource resolution

    [Fact]
    public async Task RegisterBuilding_Unauthenticated_Returns401()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/buildings",
            new RegisterBuildingRequest("parcel-00000000000000000000000000",
                "SingleFamily", 3, 2m, 1200m, 1990, 280000m));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetBuilding_Unauthenticated_Returns401()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/buildings/building-00000000000000000000000000");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetBuildings_Unauthenticated_Returns401()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/buildings");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RecordBuildingAssessment_Unauthenticated_Returns401()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/buildings/building-00000000000000000000000000/assessments",
            new RecordBuildingAssessmentRequest(310000m, new DateOnly(2025, 1, 1), null));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateBuildingStatus_Unauthenticated_Returns401()
    {
        var client = factory.CreateClient();

        var response = await client.PatchAsJsonAsync(
            "/api/buildings/building-00000000000000000000000000/status",
            new UpdateBuildingStatusRequest("Active"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RegisterBuilding_ValidRequest_ReturnsCreated()
    {
        var parcelId = await CreateLandParcelAsync();
        var client = factory.CreateClient().WithAuthToken();

        var response = await client.PostAsJsonAsync("/api/buildings",
            new RegisterBuildingRequest(parcelId, "SingleFamily", 3, 2m, 1200m, 1990, 280000m));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<RegisterBuildingResponse>();
        result!.BuildingId.Should().NotBeNullOrWhiteSpace();
        result.BuildingId.Should().StartWith("building-");
    }

    [Fact]
    public async Task GetBuilding_AfterRegistration_ReturnsBuilding()
    {
        var parcelId = await CreateLandParcelAsync();
        var client = factory.CreateClient().WithAuthToken();

        var createResponse = await client.PostAsJsonAsync("/api/buildings",
            new RegisterBuildingRequest(parcelId, "Duplex", 4, 2m, 1800m, 2005, 420000m));
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<RegisterBuildingResponse>();
        var buildingId = created!.BuildingId;

        var getResponse = await client.GetAsync($"/api/buildings/{buildingId}");

        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var building = await getResponse.Content.ReadFromJsonAsync<BuildingResponse>();
        building!.Id.Should().Be(buildingId);
        building.LandParcelId.Should().Be(parcelId);
    }

    [Fact]
    public async Task GetBuildingAssessments_Unauthenticated_Returns401()
    {
        var client = factory.CreateClient();
        var response = await client.GetAsync("/api/buildings/building-00000000000000000000000000/assessments");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetBuildingAssessments_AfterRecordingAssessments_ReturnsPaginatedListNewestFirst()
    {
        var parcelId = await CreateLandParcelAsync();
        var client = factory.CreateClient().WithAuthToken();

        // Register building
        var createResponse = await client.PostAsJsonAsync("/api/buildings",
            new RegisterBuildingRequest(parcelId, "SingleFamily", 3, 2m, 1200m, 1990, 300000m));
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<RegisterBuildingResponse>();
        var buildingId = created!.BuildingId;

        // Record two assessments
        var firstDate = new DateOnly(2024, 1, 15);
        var secondDate = new DateOnly(2025, 6, 10);

        await client.PostAsJsonAsync($"/api/buildings/{buildingId}/assessments",
            new RecordBuildingAssessmentRequest(310000m, firstDate, "Good condition"));
        await client.PostAsJsonAsync($"/api/buildings/{buildingId}/assessments",
            new RecordBuildingAssessmentRequest(325000m, secondDate, "Excellent condition"));

        // Get assessments
        var response = await client.GetAsync($"/api/buildings/{buildingId}/assessments?page.number=1&page.size=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginatedResponse<BuildingAssessmentItemResponse>>();
        result!.Items.Should().HaveCount(2);
        result.Items[0].AssessmentDate.Should().Be(secondDate);
        result.Items[1].AssessmentDate.Should().Be(firstDate);
    }

    private async Task<string> CreateLandParcelAsync()
    {
        var client = factory.CreateClient().WithAuthToken();
        var response = await client.PostAsJsonAsync("/api/land-parcels",
            new RegisterLandParcelRequest("APN-BLDG-001", "100 Building Ave", 0.5m,
                new DateOnly(2022, 1, 1), "Purchase", null));
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<RegisterLandParcelResponse>();
        return result!.ParcelId;
    }
}
