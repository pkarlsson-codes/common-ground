using System.Net;
using System.Net.Http.Json;
using CommonGround.API.Contracts.PropertyRegistry.Requests;
using CommonGround.SharedKernel.Infrastructure.API;
using FluentAssertions;

namespace CommonGround.API.IntegrationTests.PropertyRegistry;

[Collection(IntegrationCollection.Name)]
public class LandParcelsTests(ApiFactory factory)
{
    // 401 tests — use fake IDs; auth fires before resource resolution

    [Fact]
    public async Task RegisterLandParcel_Unauthenticated_Returns401()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/land-parcels",
            new RegisterLandParcelRequest("APN-001", "123 Main St", 0.25m,
                new DateOnly(2024, 3, 15), "Purchase", null));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetLandParcel_Unauthenticated_Returns401()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/land-parcels/parcel-00000000000000000000000000");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetLandParcels_Unauthenticated_Returns401()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/land-parcels");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RegisterLandParcel_ValidRequest_ReturnsCreated()
    {
        var client = factory.CreateClient().WithAuthToken();

        var response = await client.PostAsJsonAsync("/api/land-parcels",
            new RegisterLandParcelRequest(
                "APN-TEST-001",
                "456 Oak Ave, Springfield",
                0.33m,
                new DateOnly(2023, 6, 1),
                "Donation",
                "Resale formula: appraisal-based, 25% share"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
            var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<string>>();
            envelope.Should().NotBeNull();
            envelope!.Data.Should().NotBeNullOrWhiteSpace();
            envelope.Data.Should().StartWith("parcel-");
    }

    [Fact]
    public async Task GetLandParcel_AfterRegistration_ReturnsParcel()
    {
        var client = factory.CreateClient().WithAuthToken();

        // Register
        var createResponse = await client.PostAsJsonAsync("/api/land-parcels",
            new RegisterLandParcelRequest("APN-TEST-002", "789 Elm St", 0.5m,
                new DateOnly(2022, 1, 10), "Purchase", null));
        createResponse.EnsureSuccessStatusCode();
        var envelope = await createResponse.Content.ReadFromJsonAsync<ApiResponse<string>>();
        envelope.Should().NotBeNull();
        envelope!.Data.Should().NotBeNullOrWhiteSpace();
        var parcelId = envelope.Data!;

        // Fetch
        var getResponse = await client.GetAsync($"/api/land-parcels/{parcelId}");

        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}