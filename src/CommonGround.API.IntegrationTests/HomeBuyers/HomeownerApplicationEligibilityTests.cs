using System.Net;
using System.Net.Http.Json;
using CommonGround.API.Contracts.HomeBuyers.Requests;
using CommonGround.API.Contracts.HomeBuyers.Responses;
using FluentAssertions;

namespace CommonGround.API.IntegrationTests.HomeBuyers;

/// <summary>
/// Covers the query-backed application rules: one open application per household on submit,
/// and the readiness filter on applicant candidates for a home.
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

    [Fact]
    public async Task GetHomeCandidates_ApprovedApplicationWithIncompleteReadiness_IsExcluded()
    {
        var client = factory.CreateClient().WithAuthToken();

        var readyHouseholdId = await RegisterHouseholdAsync(client, "ready-candidate@example.com");
        await RecordReadinessAsync(client, readyHouseholdId);
        var readyApplicationId = await SubmitAndApproveAsync(client, readyHouseholdId);

        var notReadyHouseholdId = await RegisterHouseholdAsync(client, "not-ready-candidate@example.com");
        var notReadyApplicationId = await SubmitAndApproveAsync(client, notReadyHouseholdId);

        var candidates = await client.GetFromJsonAsync<List<HomeCandidateResponse>>(
            $"/api/homes/home-{Guid.NewGuid():N}/applicant-candidates");

        candidates!.Select(c => c.Id).Should().Contain(readyApplicationId).And.NotContain(notReadyApplicationId);
    }

    private static async Task<string> RegisterHouseholdAsync(HttpClient client, string email)
    {
        var response = await client.PostAsJsonAsync("/api/households",
            new RegisterHouseholdRequest(
                [new HouseholdMemberRequest("Eligibility Applicant", email, "555-0100", true)],
                false,
                65000m));
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<RegisterHouseholdResponse>())!.HouseholdId;
    }

    private static async Task RecordReadinessAsync(HttpClient client, string householdId)
    {
        (await client.PostAsJsonAsync($"/api/households/{householdId}/housing-counseling",
            new RecordHousingCounselingRequest("CERT-001", new DateOnly(2026, 6, 2), "Test Counseling Org")))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await client.PostAsJsonAsync($"/api/households/{householdId}/clt-orientation",
            new RecordCltOrientationRequest(new DateOnly(2026, 6, 3))))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await client.PostAsJsonAsync($"/api/households/{householdId}/mortgage-prequalification",
            new RecordMortgagePrequalificationRequest("Test Lender", 210000m, new DateOnly(2099, 1, 1))))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    private static async Task<string> SubmitAsync(HttpClient client, string householdId)
    {
        var response = await client.PostAsJsonAsync("/api/homeowner-applications", new SubmitApplicationRequest(householdId));
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<SubmitApplicationResponse>())!.ApplicationId;
    }

    private static async Task<string> SubmitAndApproveAsync(HttpClient client, string householdId)
    {
        var applicationId = await SubmitAsync(client, householdId);
        (await client.PostAsJsonAsync($"/api/homeowner-applications/{applicationId}/approve",
            new ApproveApplicationRequest(new DateOnly(2026, 6, 1), null)))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);
        return applicationId;
    }
}
