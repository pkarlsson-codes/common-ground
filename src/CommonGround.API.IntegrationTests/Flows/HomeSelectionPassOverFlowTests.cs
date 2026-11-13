using System.Net;
using System.Net.Http.Json;
using CommonGround.API.Contracts.HomeBuyers.Requests;
using CommonGround.API.Contracts.HomeBuyers.Responses;
using CommonGround.API.Contracts.Homeownership.Requests;
using CommonGround.API.Contracts.Homeownership.Responses;
using CommonGround.API.Contracts.PropertyRegistry.Requests;
using CommonGround.API.Contracts.PropertyRegistry.Responses;
using CommonGround.SharedKernel.Infrastructure.API;
using FluentAssertions;

namespace CommonGround.API.IntegrationTests.Flows;

/// <summary>
/// Covers the pass-overs recorded for a home selection's other candidates once it chooses a household.
/// Completing a selection writes only the selection and the chosen application; the pass-overs follow,
/// one message per candidate, so each step waits for them to finish.
///
/// The test database is shared across the collection, so every selection snapshots the approved
/// applications other tests left behind as well. Assertions are therefore limited to the applications
/// each test creates.
/// </summary>
[Collection(DurableIntegrationCollection.Name)]
public class HomeSelectionPassOverFlowTests(DurableApiFactory factory)
{
    private static readonly DateOnly ApprovedDate = new(2026, 6, 1);
    private static readonly DateOnly SelectedDate = new(2026, 7, 20);

    private readonly FlowTracker _tracker = new(factory);

    [Fact]
    public async Task SelectingByPriority_PassesOverOnlyTheOtherCandidatesStillEligible()
    {
        var client = factory.CreateClient().WithAuthToken();
        var homeId = await CreateHomeAsync(client);
        var templateId = await CreateActiveGroundLeaseTemplateAsync(client);

        var chosen = await CreateApprovedApplicationAsync(client);
        var eligible = await CreateApprovedApplicationAsync(client);
        var notReady = await CreateApprovedApplicationAsync(client, ready: false);
        var withdrawn = await CreateApprovedApplicationAsync(client);

        var selectionId = await StartSelectionAsync(client, homeId);
        await _tracker.TrackAsync(async () =>
            (await client.PostAsJsonAsync($"/api/homeowner-applications/{withdrawn}/withdraw",
                new WithdrawApplicationRequest(new DateOnly(2026, 7, 1), null)))
                .StatusCode.Should().Be(HttpStatusCode.NoContent));

        await SelectByPriorityAsync(client, selectionId, chosen, templateId);

        (await GetApplicationAsync(client, chosen)).PassOvers.Should().BeEmpty();

        var eligibleApplication = await GetApplicationAsync(client, eligible);
        eligibleApplication.Phase.Should().Be("Approved");
        eligibleApplication.PassOvers.Should().ContainSingle().Which.Should().BeEquivalentTo(new
        {
            ApplicationId = eligible,
            SelectionId = selectionId,
            HomeId = homeId,
            SelectionMethod = "Priority",
        });

        (await GetApplicationAsync(client, notReady)).PassOvers.Should().BeEmpty();
        (await GetApplicationAsync(client, withdrawn)).PassOvers.Should().BeEmpty();

        var waitingList = (await GetWholeWaitingListAsync(client)).ToDictionary(e => e.ApplicationId);
        waitingList[eligible].PassOverCount.Should().Be(1);
        waitingList[notReady].PassOverCount.Should().Be(0);
    }

    [Fact]
    public async Task DrawingALottery_PassesOverEveryOtherCandidateStillEligible()
    {
        var client = factory.CreateClient().WithAuthToken();
        var homeId = await CreateHomeAsync(client);
        var templateId = await CreateActiveGroundLeaseTemplateAsync(client);

        var tiedA = await CreateApprovedApplicationAsync(client);
        var tiedB = await CreateApprovedApplicationAsync(client);
        var tiedNotReady = await CreateApprovedApplicationAsync(client, ready: false);
        var untied = await CreateApprovedApplicationAsync(client);

        var selectionId = await StartSelectionAsync(client, homeId);
        await _tracker.TrackAsync(async () =>
            (await client.PostAsJsonAsync($"/api/home-selections/{selectionId}/draw-lottery",
                new DrawSelectionLotteryRequest([tiedA, tiedB, tiedNotReady], templateId, 200000m, SelectedDate)))
                .StatusCode.Should().Be(HttpStatusCode.NoContent));

        var winner = (await client.GetFromJsonAsync<HomeSelectionResponse>($"/api/home-selections/{selectionId}"))!
            .SelectedApplicationId;
        var loser = winner == tiedA ? tiedB : tiedA;

        (await GetApplicationAsync(client, winner!)).PassOvers.Should().BeEmpty();
        (await GetApplicationAsync(client, loser)).PassOvers
            .Should().ContainSingle().Which.SelectionMethod.Should().Be("Lottery");
        (await GetApplicationAsync(client, untied)).PassOvers
            .Should().ContainSingle("every eligible candidate is passed over, tied or not");
        (await GetApplicationAsync(client, tiedNotReady)).PassOvers.Should().BeEmpty();
    }

    [Fact]
    public async Task PassOvers_FollowTheHouseholdToItsLaterApplications()
    {
        var client = factory.CreateClient().WithAuthToken();
        var templateId = await CreateActiveGroundLeaseTemplateAsync(client);
        var householdId = await RegisterReadyHouseholdAsync(client);
        var passedOver = await SubmitAndApproveAsync(client, householdId);

        var firstSelectionId = await SelectSomeoneElseAsync(client, templateId);

        await _tracker.TrackAsync(async () =>
            (await client.PostAsJsonAsync($"/api/homeowner-applications/{passedOver}/withdraw",
                new WithdrawApplicationRequest(new DateOnly(2026, 7, 21), null)))
                .StatusCode.Should().Be(HttpStatusCode.NoContent));
        var reapplication = await SubmitAndApproveAsync(client, householdId, new DateOnly(2026, 8, 1));

        var secondSelectionId = await SelectSomeoneElseAsync(client, templateId);

        (await GetApplicationAsync(client, reapplication)).PassOvers.Select(p => (p.ApplicationId, p.SelectionId))
            .Should().Equal((passedOver, firstSelectionId), (reapplication, secondSelectionId));

        var waitingList = (await GetWholeWaitingListAsync(client)).ToDictionary(e => e.ApplicationId);
        waitingList[reapplication].PassOverCount.Should().Be(2);
    }

    /// <summary>Starts a selection for a new home and chooses a new applicant, passing over everyone else eligible.</summary>
    private async Task<string> SelectSomeoneElseAsync(HttpClient client, string templateId)
    {
        var homeId = await CreateHomeAsync(client);
        var chosen = await CreateApprovedApplicationAsync(client);
        var selectionId = await StartSelectionAsync(client, homeId);
        await SelectByPriorityAsync(client, selectionId, chosen, templateId);
        return selectionId;
    }

    private Task SelectByPriorityAsync(HttpClient client, string selectionId, string applicationId, string templateId) =>
        _tracker.TrackAsync(async () =>
            (await client.PostAsJsonAsync($"/api/home-selections/{selectionId}/select-by-priority",
                new SelectHouseholdByPriorityRequest(
                    applicationId, ["WaitingListTenure"], "Longest on the waiting list", templateId, 200000m, SelectedDate)))
                .StatusCode.Should().Be(HttpStatusCode.NoContent));

    private Task<string> StartSelectionAsync(HttpClient client, string homeId) =>
        _tracker.TrackAsync(async () =>
        {
            var response = await client.PostAsync($"/api/homes/{homeId}/home-selections", null);
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            return (await response.Content.ReadFromJsonAsync<StartHomeSelectionResponse>())!.SelectionId;
        });

    private static async Task<HomeownerApplicationResponse> GetApplicationAsync(HttpClient client, string applicationId) =>
        (await client.GetFromJsonAsync<HomeownerApplicationResponse>($"/api/homeowner-applications/{applicationId}"))!;

    private static async Task<List<WaitingListEntryResponse>> GetWholeWaitingListAsync(HttpClient client)
    {
        var entries = new List<WaitingListEntryResponse>();
        for (var pageNumber = 1; ; pageNumber++)
        {
            var page = await client.GetFromJsonAsync<PaginatedResponse<WaitingListEntryResponse>>(
                $"/api/waiting-list{new GetWaitingListQuerySpec(pageNumber, GetWaitingListQuerySpec.MaxPageSize).ToQueryString()}");
            entries.AddRange(page!.Items);
            if (pageNumber >= page.TotalPages)
                return entries;
        }
    }

    /// <summary>Registers a home and waits for HomeBuyers to learn of it.</summary>
    private Task<string> CreateHomeAsync(HttpClient client) =>
        _tracker.TrackAsync(async () =>
        {
            var parcel = await client.PostAsJsonAsync("/api/land-parcels",
                new RegisterLandParcelRequest($"APN-{Guid.NewGuid():N}", "789 Elm St", 0.25m, new DateOnly(2024, 1, 1), "Purchase", null));
            parcel.StatusCode.Should().Be(HttpStatusCode.Created);
            var landParcelId = (await parcel.Content.ReadFromJsonAsync<RegisterLandParcelResponse>())!.ParcelId;

            var home = await client.PostAsJsonAsync("/api/homes",
                new RegisterHomeRequest(
                    landParcelId, "SingleFamily", 3, 2m, false, 80, 1200m, 1990, "Test Appraiser", new DateOnly(2024, 1, 1), 280000m));
            home.StatusCode.Should().Be(HttpStatusCode.Created);
            return (await home.Content.ReadFromJsonAsync<RegisterHomeResponse>())!.HomeId;
        });

    private static async Task<string> CreateActiveGroundLeaseTemplateAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/ground-lease-templates",
            new CreateGroundLeaseTemplateRequest(
                $"Standard 99-Year {Guid.NewGuid():N}", 99, 60, 45m, 15m, "Standard covenant terms", "AppraisalBased", 0.25m, null, null, null, null, 90));
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<CreateGroundLeaseTemplateResponse>())!.GroundLeaseTemplateId;
    }

    /// <summary>
    /// Registers a one-member household and submits and approves its application. A <paramref name="ready"/>
    /// household has finished counseling and orientation and holds a prequalification that never expires,
    /// so it can always be selected; otherwise it has none of them.
    /// </summary>
    private async Task<string> CreateApprovedApplicationAsync(HttpClient client, bool ready = true)
    {
        var householdId = ready ? await RegisterReadyHouseholdAsync(client) : await RegisterHouseholdAsync(client);
        return await SubmitAndApproveAsync(client, householdId);
    }

    private static async Task<string> RegisterReadyHouseholdAsync(HttpClient client)
    {
        var householdId = await RegisterHouseholdAsync(client);

        (await client.PostAsJsonAsync($"/api/households/{householdId}/housing-counseling",
            new RecordHousingCounselingRequest("CERT-001", new DateOnly(2026, 1, 2), "Test Counseling Org")))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await client.PostAsJsonAsync($"/api/households/{householdId}/clt-orientation",
            new RecordCltOrientationRequest(new DateOnly(2026, 1, 3))))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await client.PostAsJsonAsync($"/api/households/{householdId}/mortgage-prequalification",
            new RecordMortgagePrequalificationRequest("Test Lender", 210000m, new DateOnly(2099, 1, 1))))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        return householdId;
    }

    private static async Task<string> RegisterHouseholdAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/households",
            new RegisterHouseholdRequest(
                [new HouseholdMemberRequest("Pass-over Applicant", $"pass-over-{Guid.NewGuid():N}@example.com", "(202) 555-0143", true)],
                false,
                65000m));
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<RegisterHouseholdResponse>())!.HouseholdId;
    }

    private Task<string> SubmitAndApproveAsync(HttpClient client, string householdId, DateOnly? approvedDate = null) =>
        _tracker.TrackAsync(async () =>
        {
            var submitted = await client.PostAsJsonAsync("/api/homeowner-applications", new SubmitApplicationRequest(householdId));
            submitted.StatusCode.Should().Be(HttpStatusCode.Created);
            var applicationId = (await submitted.Content.ReadFromJsonAsync<SubmitApplicationResponse>())!.ApplicationId;

            (await client.PostAsJsonAsync($"/api/homeowner-applications/{applicationId}/approve",
                new ApproveApplicationRequest(approvedDate ?? ApprovedDate, null)))
                .StatusCode.Should().Be(HttpStatusCode.NoContent);

            return applicationId;
        });
}
