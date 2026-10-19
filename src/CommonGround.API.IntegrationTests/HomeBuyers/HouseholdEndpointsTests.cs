using System.Net;
using System.Net.Http.Json;
using CommonGround.API.Contracts.HomeBuyers.Requests;
using CommonGround.API.Contracts.HomeBuyers.Responses;
using CommonGround.SharedKernel.Infrastructure.API;
using FluentAssertions;

namespace CommonGround.API.IntegrationTests.HomeBuyers;

/// <summary>
/// Covers the household list, member and circumstances endpoints, and the application history on household detail.
/// </summary>
[Collection(IntegrationCollection.Name)]
public class HouseholdEndpointsTests(ApiFactory factory)
{
    [Fact]
    public async Task GetHouseholds_SearchByPrimaryMemberEmail_ReturnsMatchingHousehold()
    {
        var client = factory.CreateClient().WithAuthToken();
        var email = $"search-{Guid.NewGuid():N}@example.com";
        var householdId = await RegisterHouseholdAsync(client, email);

        var page = await client.GetFromJsonAsync<PaginatedResponse<HouseholdListItemResponse>>(
            $"/api/households?search={Uri.EscapeDataString(email.ToUpperInvariant())}");

        page!.Items.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new { Id = householdId, PrimaryContact = new { Email = email }, HouseholdSize = 1 });
    }

    [Fact]
    public async Task GetHouseholds_SearchByOtherMemberName_ReturnsHouseholdUsingTheMembersCurrentName()
    {
        var client = factory.CreateClient().WithAuthToken();
        var householdId = await RegisterHouseholdAsync(client, $"other-member-{Guid.NewGuid():N}@example.com");
        var added = await client.PostAsJsonAsync($"/api/households/{householdId}/members",
            new AddHouseholdMemberRequest("Partner", "partner@example.com", "555-0101"));
        var memberId = (await added.Content.ReadFromJsonAsync<AddHouseholdMemberResponse>())!.MemberId;
        var newName = $"Renamed-{Guid.NewGuid():N}";
        await client.PutAsJsonAsync($"/api/households/{householdId}/members/{memberId}",
            new UpdateHouseholdMemberRequest(newName, "partner@example.com", "555-0101"));

        var page = await client.GetFromJsonAsync<PaginatedResponse<HouseholdListItemResponse>>(
            $"/api/households?search={Uri.EscapeDataString(newName.ToLowerInvariant())}");

        page!.Items.Should().ContainSingle().Which.Id.Should().Be(householdId);
    }

    [Fact]
    public async Task HouseholdMembers_AddUpdateRemove_AreReflectedOnHouseholdDetail()
    {
        var client = factory.CreateClient().WithAuthToken();
        var householdId = await RegisterHouseholdAsync(client, $"members-{Guid.NewGuid():N}@example.com");

        var added = await client.PostAsJsonAsync($"/api/households/{householdId}/members",
            new AddHouseholdMemberRequest("Partner", "partner@example.com", "555-0101"));
        added.StatusCode.Should().Be(HttpStatusCode.Created);
        var memberId = (await added.Content.ReadFromJsonAsync<AddHouseholdMemberResponse>())!.MemberId;

        (await client.PutAsJsonAsync($"/api/households/{householdId}/members/{memberId}",
            new UpdateHouseholdMemberRequest("Partner Renamed", "partner@example.com", "555-0199")))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        var household = await GetHouseholdAsync(client, householdId);
        household.HouseholdSize.Should().Be(2);
        household.Members.Should().ContainSingle(m => m.Id == memberId)
            .Which.Should().BeEquivalentTo(new { Name = "Partner Renamed", Phone = "555-0199", IsPrimary = false });

        (await client.DeleteAsync($"/api/households/{householdId}/members/{memberId}"))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        (await GetHouseholdAsync(client, householdId)).Members.Should().ContainSingle(m => m.IsPrimary);
    }

    [Fact]
    public async Task RemoveHouseholdMember_PrimaryMember_IsRejected()
    {
        var client = factory.CreateClient().WithAuthToken();
        var householdId = await RegisterHouseholdAsync(client, $"remove-primary-{Guid.NewGuid():N}@example.com");
        var primaryId = (await GetHouseholdAsync(client, householdId)).Members.Single().Id;

        var response = await client.DeleteAsync($"/api/households/{householdId}/members/{primaryId}");

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task UpdateHouseholdCircumstances_NewValues_AreReflectedOnHouseholdDetail()
    {
        var client = factory.CreateClient().WithAuthToken();
        var householdId = await RegisterHouseholdAsync(client, $"circumstances-{Guid.NewGuid():N}@example.com");

        (await client.PutAsJsonAsync($"/api/households/{householdId}/circumstances",
            new UpdateHouseholdCircumstancesRequest(true, 72000m)))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        (await GetHouseholdAsync(client, householdId))
            .Should().BeEquivalentTo(new { AccessibilityRequired = true, AnnualIncome = 72000m });
    }

    [Fact]
    public async Task GetHousehold_AfterSubmittingApplication_ListsItInApplicationHistory()
    {
        var client = factory.CreateClient().WithAuthToken();
        var householdId = await RegisterHouseholdAsync(client, $"history-{Guid.NewGuid():N}@example.com");
        var submitted = await client.PostAsJsonAsync("/api/homeowner-applications", new SubmitApplicationRequest(householdId));
        var applicationId = (await submitted.Content.ReadFromJsonAsync<SubmitApplicationResponse>())!.ApplicationId;

        var household = await GetHouseholdAsync(client, householdId);

        household.Applications.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new { Id = applicationId, Phase = "Submitted" });
    }

    private static async Task<string> RegisterHouseholdAsync(HttpClient client, string email)
    {
        var response = await client.PostAsJsonAsync("/api/households",
            new RegisterHouseholdRequest(
                [new HouseholdMemberRequest("Household Applicant", email, "555-0100", true)],
                false,
                65000m));
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<RegisterHouseholdResponse>())!.HouseholdId;
    }

    private static async Task<HouseholdResponse> GetHouseholdAsync(HttpClient client, string householdId) =>
        (await client.GetFromJsonAsync<HouseholdResponse>($"/api/households/{householdId}"))!;
}
