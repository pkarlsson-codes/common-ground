using System.Net.Http.Json;
using CommonGround.API.Contracts.Homeownership.Requests;
using CommonGround.API.Contracts.Homeownership.Responses;
using CommonGround.SharedKernel.Infrastructure.API;
using FluentAssertions;

namespace CommonGround.API.IntegrationTests.Homeownership;

[Collection(IntegrationCollection.Name)]
public class GroundLeaseTemplatesTests(ApiFactory factory)
{
    [Fact]
    public async Task GetGroundLeaseTemplates_Search_IsCaseInsensitiveAndOrderedByName()
    {
        var client = factory.CreateClient().WithAuthToken();
        var tag = Guid.NewGuid().ToString("N");
        var second = await CreateTemplate(client, $"Template {tag} B");
        var first = await CreateTemplate(client, $"template {tag} a");

        var result = await client.GetFromJsonAsync<PaginatedResponse<GroundLeaseTemplateListItemResponse>>(
            $"/api/ground-lease-templates?search={tag.ToUpperInvariant()}");

        result!.Items.Select(t => t.Id).Should().Equal(first, second);
    }

    [Fact]
    public async Task GetGroundLeaseTemplates_ArchivedTemplate_ListedUnderInactiveUntilRestored()
    {
        var client = factory.CreateClient().WithAuthToken();
        var name = $"Archive {Guid.NewGuid():N}";
        var id = await CreateTemplate(client, name);

        (await client.PostAsync($"/api/ground-lease-templates/{id}/archive", null)).EnsureSuccessStatusCode();
        var active = await List(client, name, "Active");
        var inactive = await List(client, name, "Inactive");

        active.Should().BeEmpty();
        inactive.Should().ContainSingle(t => t.Id == id);

        (await client.PostAsync($"/api/ground-lease-templates/{id}/restore", null)).EnsureSuccessStatusCode();
        (await List(client, name, "Active")).Should().ContainSingle(t => t.Id == id);
    }

    private static async Task<IReadOnlyList<GroundLeaseTemplateListItemResponse>> List(
        HttpClient client, string search, string status)
    {
        var result = await client.GetFromJsonAsync<PaginatedResponse<GroundLeaseTemplateListItemResponse>>(
            $"/api/ground-lease-templates?search={Uri.EscapeDataString(search)}&status={status}");
        return [.. result!.Items];
    }

    private static async Task<string> CreateTemplate(HttpClient client, string name)
    {
        var response = await client.PostAsJsonAsync("/api/ground-lease-templates",
            new CreateGroundLeaseTemplateRequest(name, 99, 60, 45m, 15m, "Standard covenant", "AppraisalBased", 0.25m, null, null, null, null, 90));
        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<CreateGroundLeaseTemplateResponse>();
        return created!.GroundLeaseTemplateId;
    }
}
