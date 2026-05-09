using System.Net;
using System.Net.Http.Json;
using CommonGround.API.Contracts.PropertyRegistry.Requests;
using CommonGround.API.Contracts.PropertyRegistry.Responses;
using CommonGround.SharedKernel.Infrastructure.API;
using FluentAssertions;

namespace CommonGround.API.IntegrationTests.PropertyRegistry;

[Collection(IntegrationCollection.Name)]
public class HomesTests(ApiFactory factory)
{
    // 401 tests — use fake IDs; auth fires before resource resolution

    [Fact]
    public async Task RegisterHome_Unauthenticated_Returns401()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/homes",
            new RegisterHomeRequest(
                "parcel-00000000000000000000000000",
                "SingleFamily",
                3,
                2m,
                1200m,
                1990,
                "Test Appraiser",
                new DateOnly(2024, 1, 1),
                280000m));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetHome_Unauthenticated_Returns401()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/homes/unit-00000000000000000000000000");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetHomes_Unauthenticated_Returns401()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/homes");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RecordHomeAssessment_Unauthenticated_Returns401()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/homes/unit-00000000000000000000000000/assessments",
            new RecordHomeAssessmentRequest(
                310000m,
                "Test Appraiser",
                new DateOnly(2025, 1, 1),
                null));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RegisterHome_ValidRequest_ReturnsCreated()
    {
        var parcelId = await CreateLandParcelAsync();
        var client = factory.CreateClient().WithAuthToken();

        var response = await client.PostAsJsonAsync("/api/homes",
            new RegisterHomeRequest(
                parcelId,
                "SingleFamily",
                3,
                2m,
                1200m,
                1990,
                "Test Appraiser",
                new DateOnly(2024, 1, 1),
                280000m));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<RegisterHomeResponse>();
        result!.HomeId.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task RegisterHome_InvalidHomeType_ReturnsBadRequest()
    {
        var parcelId = await CreateLandParcelAsync();
        var client = factory.CreateClient().WithAuthToken();

        var response = await client.PostAsJsonAsync("/api/homes",
            new RegisterHomeRequest(
                parcelId,
                string.Empty,
                3,
                2m,
                1200m,
                1990,
                "Test Appraiser",
                new DateOnly(2024, 1, 1),
                280000m));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetHome_AfterRegistration_ReturnsHome()
    {
        var parcelId = await CreateLandParcelAsync();
        var client = factory.CreateClient().WithAuthToken();

        var createResponse = await client.PostAsJsonAsync("/api/homes",
            new RegisterHomeRequest(
                parcelId,
                "Duplex",
                4,
                2m,
                1800m,
                2005,
                "Test Appraiser",
                new DateOnly(2024, 1, 1),
                420000m));
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<RegisterHomeResponse>();
        var homeId = created!.HomeId;

        var getResponse = await client.GetAsync($"/api/homes/{homeId}");

        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var home = await getResponse.Content.ReadFromJsonAsync<HomeResponse>();
        home!.Id.Should().Be(homeId);
        home.LandParcelId.Should().Be(parcelId);
    }

    [Fact]
    public async Task GetHomeAssessments_Unauthenticated_Returns401()
    {
        var client = factory.CreateClient();
        var response = await client.GetAsync("/api/homes/unit-00000000000000000000000000/assessments");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetHomeAssessments_AfterRecordingAssessments_ReturnsPaginatedListNewestFirst()
    {
        var parcelId = await CreateLandParcelAsync();
        var client = factory.CreateClient().WithAuthToken();

        // Register home
        var createResponse = await client.PostAsJsonAsync("/api/homes",
            new RegisterHomeRequest(
                parcelId,
                "SingleFamily",
                3,
                2m,
                1200m,
                1990,
                "Test Appraiser",
                new DateOnly(2024, 1, 1),
                300000m));
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<RegisterHomeResponse>();
        var homeId = created!.HomeId;

        // Record two assessments
        var firstDate = new DateOnly(2024, 1, 15);
        var secondDate = new DateOnly(2025, 6, 10);

        await client.PostAsJsonAsync($"/api/homes/{homeId}/assessments",
            new RecordHomeAssessmentRequest(
                310000m,
                "Test Appraiser",
                firstDate,
                "Good condition"));
        await client.PostAsJsonAsync($"/api/homes/{homeId}/assessments",
            new RecordHomeAssessmentRequest(
                325000m,
                "Test Appraiser",
                secondDate,
                "Excellent condition"));

        // Get assessments
        var response = await client.GetAsync($"/api/homes/{homeId}/assessments?page.number=1&page.size=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginatedResponse<HomeAssessmentItemResponse>>();
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
