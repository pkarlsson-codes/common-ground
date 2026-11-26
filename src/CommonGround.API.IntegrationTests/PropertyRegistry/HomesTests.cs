using System.Net;
using System.Net.Http.Json;
using CommonGround.API.Contracts.PropertyRegistry.Requests;
using CommonGround.API.Contracts.PropertyRegistry.Responses;
using CommonGround.HomeBuyers.Contracts.Events;
using CommonGround.PropertyRegistry.Application.EventHandlers;
using CommonGround.SharedKernel.Infrastructure.API;
using FluentAssertions;
using Marten;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

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
                false,
                80,
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
                false,
                80,
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
                false,
                80,
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
                true,
                80,
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
        home.IsAccessible.Should().BeTrue();
        home.IncomeLimitPercentOfAmi.Should().Be(80);
        home.AssessedValue.Should().Be(420000m);
        home.LastAssessedDate.Should().Be(new DateOnly(2024, 1, 1));
    }

    [Fact]
    public async Task GetHomeAssessments_Unauthenticated_Returns401()
    {
        var client = factory.CreateClient();
        var response = await client.GetAsync("/api/homes/unit-00000000000000000000000000/assessments");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetHomeAssessments_AfterRecordingAssessments_ReturnsThemWithTheInitialAssessmentNewestFirst()
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
                false,
                80,
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
                null,
                secondDate,
                "Excellent condition"));

        // Get assessments
        var response = await client.GetAsync($"/api/homes/{homeId}/assessments?page.number=1&page.size=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginatedResponse<HomeAssessmentItemResponse>>();
        result!.Items.Select(a => (a.AssessmentDate, a.AssessedValue, a.AppraiserName)).Should().Equal(
            (secondDate, 325000m, (string?)null),
            (firstDate, 310000m, "Test Appraiser"),
            (new DateOnly(2024, 1, 1), 300000m, "Test Appraiser"));
        result.Items.Should().AllSatisfy(a => a.RecordedBy.Should().NotBeNullOrEmpty());
    }

    [Fact]
    public async Task GetHomes_FilteredByOccupancyAndSaleStage_ReturnsHomesMatchingBoth()
    {
        var parcelId = await CreateLandParcelAsync();
        var client = factory.CreateClient().WithAuthToken();
        var availableHomeId = await RegisterHomeAsync(client, parcelId);
        var underContractHomeId = await RegisterHomeAsync(client, parcelId);
        using (var scope = factory.Services.CreateScope())
        {
            // ApplicationSelected has a handler in Homeownership too, so it can't be invoked through the bus.
            await using var session = scope.ServiceProvider.GetRequiredService<IDocumentStore>().LightweightSession(ApiFactory.TestTrustId);
            var (op, _) = ApplicationSelectedHandler.Handle(
                new ApplicationSelected(
                    "application-filter", "household-filter", "Filter Buyer", "filter@example.com", null, "homeselection-filter",
                    underContractHomeId, parcelId, "template-filter", 200_000m, new DateOnly(2026, 10, 1), "user-filter",
                    50_000m, 2, null, null, null),
                availability: null);
            op.Execute(session);
            await session.SaveChangesAsync();
        }

        var available = await client.GetFromJsonAsync<PaginatedResponse<HomeListItemResponse>>(
            $"/api/homes?landParcelIds={parcelId}&occupancy=Vacant&saleStages=Offered");
        var underContract = await client.GetFromJsonAsync<PaginatedResponse<HomeListItemResponse>>(
            $"/api/homes?landParcelIds={parcelId}&saleStages=UnderContract&saleStages=HeldByTrust");

        var notOffered = await client.GetFromJsonAsync<PaginatedResponse<HomeListItemResponse>>(
            $"/api/homes?landParcelIds={parcelId}&occupancy=Occupied");

        available!.Items.Select(h => (h.Id, h.Occupancy, h.SaleStage))
            .Should().Equal((availableHomeId, "Vacant", "Offered"));
        underContract!.Items.Select(h => (h.Id, h.Occupancy, h.SaleStage))
            .Should().Equal((underContractHomeId, "Vacant", "UnderContract"));
        notOffered!.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task SetHomeIncomeLimit_Unauthenticated_Returns401()
    {
        var client = factory.CreateClient();

        var response = await client.PutAsJsonAsync(
            "/api/homes/unit-00000000000000000000000000/income-limit", new SetHomeIncomeLimitRequest(60));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RegisterHome_WithoutTheIncomeLimitField_ReturnsBadRequest()
    {
        var parcelId = await CreateLandParcelAsync();
        var client = factory.CreateClient().WithAuthToken();

        var response = await client.PostAsJsonAsync("/api/homes", new
        {
            landParcelId = parcelId,
            homeType = "SingleFamily",
            bedrooms = 3,
            bathrooms = 2m,
            isAccessible = false,
            squareFootage = 1200m,
            yearBuilt = 1990,
            appraiserName = "Test Appraiser",
            initialAssessedDate = "2024-01-01",
            initialAssessedValue = 280000m,
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RegisterHome_WithNoIncomeLimit_HomeHasNone()
    {
        var parcelId = await CreateLandParcelAsync();
        var client = factory.CreateClient().WithAuthToken();

        var response = await client.PostAsJsonAsync("/api/homes",
            new RegisterHomeRequest(parcelId, "SingleFamily", 3, 2m, false, null, 1200m, 1990, "Test Appraiser", new DateOnly(2024, 1, 1), 280000m));
        response.EnsureSuccessStatusCode();
        var homeId = (await response.Content.ReadFromJsonAsync<RegisterHomeResponse>())!.HomeId;

        (await client.GetFromJsonAsync<HomeResponse>($"/api/homes/{homeId}"))!.IncomeLimitPercentOfAmi.Should().BeNull();
    }

    [Fact]
    public async Task SetHomeIncomeLimit_ThenNone_HomeShowsEachInTurn()
    {
        var parcelId = await CreateLandParcelAsync();
        var client = factory.CreateClient().WithAuthToken();
        var homeId = await RegisterHomeAsync(client, parcelId);

        (await client.PutAsJsonAsync($"/api/homes/{homeId}/income-limit", new SetHomeIncomeLimitRequest(60)))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await client.GetFromJsonAsync<HomeResponse>($"/api/homes/{homeId}"))!.IncomeLimitPercentOfAmi.Should().Be(60);

        (await client.PutAsJsonAsync($"/api/homes/{homeId}/income-limit", new SetHomeIncomeLimitRequest(null)))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await client.GetFromJsonAsync<HomeResponse>($"/api/homes/{homeId}"))!.IncomeLimitPercentOfAmi.Should().BeNull();
    }

    [Fact]
    public async Task SetHomeIncomeLimit_WithoutThePercentField_ReturnsBadRequestAndKeepsTheLimit()
    {
        var parcelId = await CreateLandParcelAsync();
        var client = factory.CreateClient().WithAuthToken();
        var homeId = await RegisterHomeAsync(client, parcelId);

        (await client.PutAsJsonAsync($"/api/homes/{homeId}/income-limit", new { }))
            .StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await client.GetFromJsonAsync<HomeResponse>($"/api/homes/{homeId}"))!.IncomeLimitPercentOfAmi.Should().Be(80);
    }

    [Fact]
    public async Task SetHomeIncomeLimit_OutOfRange_Returns422WithTheRange()
    {
        var parcelId = await CreateLandParcelAsync();
        var client = factory.CreateClient().WithAuthToken();
        var homeId = await RegisterHomeAsync(client, parcelId);

        var response = await client.PutAsJsonAsync($"/api/homes/{homeId}/income-limit", new SetHomeIncomeLimitRequest(250));

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        var problem = (await response.Content.ReadFromJsonAsync<ProblemDetails>())!;
        problem.Extensions["code"]!.ToString().Should().Be("property_registry.home.income_limit.percent_out_of_range");
        problem.Detail.Should().Be("Income limit must be between 1% and 200% of AMI.");
    }

    private static async Task<string> RegisterHomeAsync(HttpClient client, string parcelId)
    {
        var response = await client.PostAsJsonAsync("/api/homes",
            new RegisterHomeRequest(parcelId, "SingleFamily", 3, 2m, false, 80, 1200m, 1990, "Test Appraiser", new DateOnly(2024, 1, 1), 280000m));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<RegisterHomeResponse>())!.HomeId;
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
