using System.Net;
using System.Net.Http.Json;
using CommonGround.API.Contracts.Stewardship.Requests;
using CommonGround.Stewardship.Infrastructure.Persistence;
using FluentAssertions;

namespace CommonGround.API.IntegrationTests.Stewardship;

[Collection(IntegrationCollection.Name)]
public class ForeclosurePreventionCaseTests(ApiFactory factory)
{
    [Fact]
    public async Task GetCase_Unauthenticated_Returns401()
    {
        var client = factory.CreateClient();
        var response = await client.GetAsync("/api/stewardship/foreclosure-prevention/foreclosureprevention-00000000000000000000000001");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task OpenCase_ValidRequest_Returns202WithId()
    {
        var client = factory.CreateClient().WithAuthToken();
        var groundLeaseId = $"groundle-{Guid.NewGuid():N}";

        var response = await client.PostAsJsonAsync(
            "/api/stewardship/foreclosure-prevention",
            new OpenForeclosurePreventionCaseRequest(
                groundLeaseId, "Test Servicer", 3, new DateOnly(2026, 3, 1)));

        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var body = await response.Content.ReadFromJsonAsync<OpenCaseResponse>();
        body!.Id.Should().StartWith("foreclosureprevention-");
    }

    [Fact]
    public async Task GetCase_AfterOpening_Returns200()
    {
        var (client, caseId) = await OpenCaseAsync();

        var response = await client.GetAsync($"/api/stewardship/foreclosure-prevention/{caseId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var view = await response.Content.ReadFromJsonAsync<ForeclosurePreventionCaseView>();
        view!.Id.Should().Be(caseId);
    }

    [Fact]
    public async Task MakeCounselingReferral_AdvancesToActive_Returns204()
    {
        var (client, caseId) = await OpenCaseAsync();

        var response = await client.PostAsJsonAsync(
            $"/api/stewardship/foreclosure-prevention/{caseId}/counseling-referrals",
            new MakeCounselingReferralRequest("Home Counseling Agency", new DateOnly(2026, 3, 5)));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task RecordCounselingOutcome_AfterReferral_Returns204()
    {
        var (client, caseId) = await OpenCaseAsync();

        await client.PostAsJsonAsync(
            $"/api/stewardship/foreclosure-prevention/{caseId}/counseling-referrals",
            new MakeCounselingReferralRequest("Home Counseling Agency", new DateOnly(2026, 3, 5)));

        var caseView = await (await client.GetAsync($"/api/stewardship/foreclosure-prevention/{caseId}"))
            .Content.ReadFromJsonAsync<ForeclosurePreventionCaseView>();
        var referralId = caseView!.CounselingReferrals[0].Id;

        var response = await client.PostAsJsonAsync(
            $"/api/stewardship/foreclosure-prevention/{caseId}/counseling-referrals/{referralId}/outcome",
            new RecordCounselingOutcomeRequest("Completed successfully", new DateOnly(2026, 3, 20)));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task RequestForbearance_ValidRequest_Returns204()
    {
        var (client, caseId) = await OpenCaseAsync();

        var response = await client.PostAsJsonAsync(
            $"/api/stewardship/foreclosure-prevention/{caseId}/forbearance-requests",
            new RequestForbearanceRequest("First National Bank", new DateOnly(2026, 3, 10)));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task RecordForbearanceOutcome_AfterRequest_Returns204()
    {
        var (client, caseId) = await OpenCaseAsync();

        await client.PostAsJsonAsync(
            $"/api/stewardship/foreclosure-prevention/{caseId}/forbearance-requests",
            new RequestForbearanceRequest("First National Bank", new DateOnly(2026, 3, 10)));

        var caseView = await (await client.GetAsync($"/api/stewardship/foreclosure-prevention/{caseId}"))
            .Content.ReadFromJsonAsync<ForeclosurePreventionCaseView>();
        var requestId = caseView!.ForbearanceRequests[0].Id;

        var response = await client.PostAsJsonAsync(
            $"/api/stewardship/foreclosure-prevention/{caseId}/forbearance-requests/{requestId}/outcome",
            new RecordForbearanceOutcomeRequest("Granted", new DateOnly(2026, 4, 1), null));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ActivateCurePeriod_ValidRequest_Returns204()
    {
        var (client, caseId) = await OpenCaseAsync();

        var response = await client.PostAsJsonAsync(
            $"/api/stewardship/foreclosure-prevention/{caseId}/cure-period",
            new ActivateCurePeriodRequest("LRA-2026-001", new DateOnly(2026, 3, 15), 90));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ResolveCase_AfterActiveCounselingReferral_Returns204()
    {
        var (client, caseId) = await OpenCaseAsync();

        await client.PostAsJsonAsync(
            $"/api/stewardship/foreclosure-prevention/{caseId}/counseling-referrals",
            new MakeCounselingReferralRequest("Agency", new DateOnly(2026, 3, 5)));

        var response = await client.PostAsJsonAsync(
            $"/api/stewardship/foreclosure-prevention/{caseId}/resolve",
            new ResolveForeclosurePreventionCaseRequest("InterventionSuccessful", new DateOnly(2026, 4, 30)));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ResolveCase_OnResolvedCase_Returns422()
    {
        var (client, caseId) = await OpenCaseAsync();

        await client.PostAsJsonAsync(
            $"/api/stewardship/foreclosure-prevention/{caseId}/counseling-referrals",
            new MakeCounselingReferralRequest("Agency", new DateOnly(2026, 3, 5)));

        await client.PostAsJsonAsync(
            $"/api/stewardship/foreclosure-prevention/{caseId}/resolve",
            new ResolveForeclosurePreventionCaseRequest("InterventionSuccessful", new DateOnly(2026, 4, 30)));

        var response = await client.PostAsJsonAsync(
            $"/api/stewardship/foreclosure-prevention/{caseId}/resolve",
            new ResolveForeclosurePreventionCaseRequest("InterventionSuccessful", new DateOnly(2026, 5, 1)));

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    private async Task<(HttpClient client, string caseId)> OpenCaseAsync()
    {
        var client = factory.CreateClient().WithAuthToken();
        var groundLeaseId = $"groundle-{Guid.NewGuid():N}";

        var openResponse = await client.PostAsJsonAsync(
            "/api/stewardship/foreclosure-prevention",
            new OpenForeclosurePreventionCaseRequest(
                groundLeaseId, "Test Servicer", 3, new DateOnly(2026, 3, 1)));

        openResponse.EnsureSuccessStatusCode();
        var body = await openResponse.Content.ReadFromJsonAsync<OpenCaseResponse>();
        return (client, body!.Id);
    }

    private sealed record OpenCaseResponse(string Id);
}
