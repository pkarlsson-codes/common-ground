using System.Net;
using System.Net.Http.Json;
using CommonGround.API.Contracts.HomeBuyers.Requests;
using CommonGround.API.Contracts.HomeBuyers.Responses;
using FluentAssertions;

namespace CommonGround.API.IntegrationTests.HomeBuyers;

/// <summary>
/// Covers the query-backed application rule: one open application per household on submit.
/// </summary>
[Collection(IntegrationCollection.Name)]
public class HomeownerApplicationEligibilityTests(ApiFactory factory)
{
    [Fact]
    public async Task SubmitApplication_HouseholdHasOpenApplication_IsRejectedUntilItIsWithdrawn()
    {
        var client = factory.CreateClient().WithAuthToken();
        var householdId = await RegisterHouseholdAsync(client, "open-application@example.com");
        var firstApplicationId = await SubmitAsync(client, householdId);

        var duplicate = await client.PostAsJsonAsync("/api/homeowner-applications", new SubmitApplicationRequest(householdId));
        duplicate.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        (await client.PostAsJsonAsync($"/api/homeowner-applications/{firstApplicationId}/withdraw",
            new WithdrawApplicationRequest(new DateOnly(2026, 6, 5), null)))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        var resubmit = await client.PostAsJsonAsync("/api/homeowner-applications", new SubmitApplicationRequest(householdId));
        resubmit.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    private static async Task<string> RegisterHouseholdAsync(HttpClient client, string email)
    {
        var response = await client.PostAsJsonAsync("/api/households",
            new RegisterHouseholdRequest(
                [new HouseholdMemberRequest("Eligibility Applicant", email, "(202) 555-0100", true)],
                false,
                65000m));
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<RegisterHouseholdResponse>())!.HouseholdId;
    }

    private static async Task<string> SubmitAsync(HttpClient client, string householdId)
    {
        var response = await client.PostAsJsonAsync("/api/homeowner-applications", new SubmitApplicationRequest(householdId));
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<SubmitApplicationResponse>())!.ApplicationId;
    }
}
