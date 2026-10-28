using System.Net;
using System.Net.Http.Json;
using CommonGround.API.Contracts.Stewardship.Requests;
using CommonGround.SharedKernel.Infrastructure.API;
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
    public async Task GetCase_AfterRecordingDelinquencies_ListsThemOldestFirst()
    {
        var (client, caseId) = await OpenCaseAsync();

        var response = await client.PostAsJsonAsync(
            $"/api/stewardship/foreclosure-prevention/{caseId}/servicer-delinquency",
            new RecordServicerDelinquencyRequest("Second Servicer", 5, new DateOnly(2026, 4, 1)));
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var view = await (await client.GetAsync($"/api/stewardship/foreclosure-prevention/{caseId}"))
            .Content.ReadFromJsonAsync<ForeclosurePreventionCaseView>();
        view!.ServicerDelinquencies.Select(d => (d.ServicerName, d.DelinquentMonths, d.RecordedDate))
            .Should().Equal(
                ("Test Servicer", 3, new DateOnly(2026, 3, 1)),
                ("Second Servicer", 5, new DateOnly(2026, 4, 1)));
    }

    [Fact]
    public async Task GetAllCases_ListsNewestFirst()
    {
        var client = factory.CreateClient().WithAuthToken();
        var older = await OpenCaseAndGetIdAsync(client, new DateOnly(2001, 1, 1));
        var newer = await OpenCaseAndGetIdAsync(client, new DateOnly(2001, 1, 2));

        var page = await (await client.GetAsync("/api/stewardship/foreclosure-prevention?pageNumber=1&pageSize=100"))
            .Content.ReadFromJsonAsync<PaginatedResponse<ForeclosurePreventionCaseView>>();

        var ids = page!.Items.Select(x => x.Id).ToList();
        ids.IndexOf(newer).Should().BeLessThan(ids.IndexOf(older));
    }

    [Fact]
    public async Task GetAllCases_AfterOpening_ReturnsPaginatedResponse()
    {
        var (client, caseId) = await OpenCaseAsync();

        var response = await client.GetAsync("/api/stewardship/foreclosure-prevention?pageNumber=1&pageSize=20");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var page = await response.Content.ReadFromJsonAsync<PaginatedResponse<ForeclosurePreventionCaseView>>();
        page!.PageNumber.Should().Be(1);
        page.PageSize.Should().Be(20);
        page.TotalCount.Should().BeGreaterThanOrEqualTo(1);
        page.TotalPages.Should().BeGreaterThanOrEqualTo(1);
        page.Items.Should().Contain(x => x.Id == caseId);
    }

    [Fact]
    public async Task GetAllCases_Page2WithSmallPageSize_ReturnsDifferentItems()
    {
        var client = factory.CreateClient().WithAuthToken();

        var firstId = await OpenCaseAndGetIdAsync(client);
        var secondId = await OpenCaseAndGetIdAsync(client);

        var page1 = await (await client.GetAsync("/api/stewardship/foreclosure-prevention?pageNumber=1&pageSize=1"))
            .Content.ReadFromJsonAsync<PaginatedResponse<ForeclosurePreventionCaseView>>();
        var page2 = await (await client.GetAsync("/api/stewardship/foreclosure-prevention?pageNumber=2&pageSize=1"))
            .Content.ReadFromJsonAsync<PaginatedResponse<ForeclosurePreventionCaseView>>();

        page1!.Items.Should().HaveCount(1);
        page2!.Items.Should().HaveCount(1);
        page1.Items[0].Id.Should().NotBe(page2.Items[0].Id);
        page2.PageNumber.Should().Be(2);
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
    public async Task ActivateCurePeriod_AfterForbearanceOutcome_Returns204()
    {
        var (client, caseId) = await OpenCaseAsync();

        await client.PostAsJsonAsync(
            $"/api/stewardship/foreclosure-prevention/{caseId}/forbearance-requests",
            new RequestForbearanceRequest("First National Bank", new DateOnly(2026, 3, 10)));
        var caseView = await (await client.GetAsync($"/api/stewardship/foreclosure-prevention/{caseId}"))
            .Content.ReadFromJsonAsync<ForeclosurePreventionCaseView>();
        await client.PostAsJsonAsync(
            $"/api/stewardship/foreclosure-prevention/{caseId}/forbearance-requests/{caseView!.ForbearanceRequests[0].Id}/outcome",
            new RecordForbearanceOutcomeRequest("Granted", new DateOnly(2026, 4, 1), null));

        // The case's stream is reloaded, so the recorded outcome must deserialize.
        var response = await client.PostAsJsonAsync(
            $"/api/stewardship/foreclosure-prevention/{caseId}/cure-period",
            new ActivateCurePeriodRequest("LRA-2026-002", new DateOnly(2026, 4, 2), 90));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task RecordForbearanceOutcome_UnknownOutcome_Returns400()
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
            new RecordForbearanceOutcomeRequest("Withdrawn", new DateOnly(2026, 4, 1), null));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
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
        var id = await OpenCaseAndGetIdAsync(client);
        return (client, id);
    }

    private async Task<string> OpenCaseAndGetIdAsync(HttpClient client, DateOnly? recordedDate = null)
    {
        var groundLeaseId = $"groundle-{Guid.NewGuid():N}";
        var openResponse = await client.PostAsJsonAsync(
            "/api/stewardship/foreclosure-prevention",
            new OpenForeclosurePreventionCaseRequest(
                groundLeaseId, "Test Servicer", 3, recordedDate ?? new DateOnly(2026, 3, 1)));
        openResponse.EnsureSuccessStatusCode();
        var body = await openResponse.Content.ReadFromJsonAsync<OpenCaseResponse>();
        return body!.Id;
    }

    private sealed record OpenCaseResponse(string Id);
}
