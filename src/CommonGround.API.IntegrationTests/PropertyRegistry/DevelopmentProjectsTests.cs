using System.Net;
using System.Net.Http.Json;
using CommonGround.API.Contracts.PropertyRegistry.Requests;
using CommonGround.API.Contracts.PropertyRegistry.Responses;
using CommonGround.PropertyRegistry.Domain.DevelopmentProjects;
using CommonGround.PropertyRegistry.Domain.Homes;
using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.SharedKernel.Infrastructure.API;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace CommonGround.API.IntegrationTests.PropertyRegistry;

[Collection(IntegrationCollection.Name)]
public class DevelopmentProjectsTests(ApiFactory factory)
{
    [Fact]
    public async Task GetDevelopmentProject_ReturnsProjectWithHomes()
    {
        var client = factory.CreateClient().WithAuthToken();
        var parcelId = LandParcelId.Generate().Value;
        var homeId = HomeId.Generate().Value;
        var id = await InitiateAsync(client, parcelId, "Roof Rehab", "Rehabilitation", [homeId]);

        var project = await client.GetFromJsonAsync<DevelopmentProjectResponse>($"/api/development-projects/{id}");

        project.Should().BeEquivalentTo(new DevelopmentProjectResponse(
            id, parcelId, "Roof Rehab", "Scope of work", "Initiated", "Rehabilitation", [homeId]));
    }

    [Fact]
    public async Task GetDevelopmentProject_Unknown_Returns404()
    {
        var client = factory.CreateClient().WithAuthToken();

        var response = await client.GetAsync($"/api/development-projects/{DevelopmentProjectId.Generate().Value}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetDevelopmentProjects_ListsNewestFirstAndSearchesName()
    {
        var client = factory.CreateClient().WithAuthToken();
        var parcelId = LandParcelId.Generate().Value;
        var older = await InitiateAsync(client, parcelId, "Phase One Infill", "NewConstruction");
        var newer = await InitiateAsync(client, parcelId, "Phase Two Infill", "NewConstruction");
        await InitiateAsync(client, parcelId, "Unrelated", "NewConstruction");

        var result = await client.GetFromJsonAsync<PaginatedResponse<DevelopmentProjectListItemResponse>>(
            $"/api/development-projects?landParcelId={parcelId}&search=infill");

        result!.Items.Select(p => p.Id).Should().Equal(newer, older);
    }

    [Fact]
    public async Task InitiateDevelopmentProject_BlankName_Returns422()
    {
        var client = factory.CreateClient().WithAuthToken();

        var response = await client.PostAsJsonAsync("/api/development-projects",
            new InitiateDevelopmentProjectRequest(LandParcelId.Generate().Value, " ", "NewConstruction", "Scope of work", null));

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem!.Detail.Should().Be("Project name is required.");
    }

    private static async Task<string> InitiateAsync(
        HttpClient client, string parcelId, string name, string type, List<string>? homeIds = null)
    {
        var response = await client.PostAsJsonAsync("/api/development-projects",
            new InitiateDevelopmentProjectRequest(parcelId, name, type, "Scope of work", homeIds));
        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<InitiateDevelopmentProjectResponse>();
        return created!.ProjectId;
    }
}
