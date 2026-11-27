using System.Net;
using System.Net.Http.Json;
using CommonGround.API.Contracts.HomeBuyers.Requests;
using CommonGround.API.Contracts.HomeBuyers.Responses;
using CommonGround.API.Contracts.Homeownership.Requests;
using CommonGround.API.Contracts.Homeownership.Responses;
using CommonGround.API.Contracts.PropertyRegistry.Requests;
using CommonGround.API.Contracts.PropertyRegistry.Responses;
using CommonGround.API.Contracts.Today.Responses;
using CommonGround.SharedKernel.Infrastructure.API;
using FluentAssertions;

namespace CommonGround.API.IntegrationTests.Flows;

/// <summary>
/// Covers a waiting household's changes reaching its application: staff update the household, the open application
/// takes the new figures, and an approved one whose income or size changed can't be chosen until staff confirm it
/// still qualifies. The application follows the household through a handler, so each household change waits for it.
///
/// The test database is shared across the collection, so assertions are limited to the applications each test creates.
/// </summary>
[Collection(DurableIntegrationCollection.Name)]
public class QualificationToConfirmFlowTests(DurableApiFactory factory)
{
    private static readonly DateOnly ApprovedDate = new(2026, 6, 1);
    private static readonly DateOnly SelectedDate = new(2026, 7, 20);

    private readonly FlowTracker _tracker = new(factory);

    [Fact]
    public async Task AWaitingHouseholdsNewIncome_MarksItsApplicationUntilStaffConfirmItStillQualifies()
    {
        var client = factory.CreateClient().WithAuthToken();
        var homeId = await CreateHomeAsync(client);
        var templateId = await CreateActiveGroundLeaseTemplateAsync(client);
        var householdId = await RegisterReadyHouseholdAsync(client);
        var marked = await SubmitAndApproveAsync(client, householdId);
        var other = await SubmitAndApproveAsync(client, await RegisterReadyHouseholdAsync(client));
        var today = (await client.GetFromJsonAsync<TodayResponse>("/api/today"))!.Today;

        await UpdateCircumstancesAsync(client, householdId, 58000m, requiresAccessibility: false);

        var application = await GetApplicationAsync(client, marked);
        application.Should().BeEquivalentTo(new
        {
            Phase = "Approved",
            ApprovedDate,
            AnnualIncome = 58000m,
            QualificationToConfirmSince = today,
            QualifiedAnnualIncome = 65000m,
            QualifiedHouseholdSize = 1,
        });
        (await GetWholeWaitingListAsync(client)).Single(e => e.ApplicationId == marked)
            .QualificationToConfirmSince.Should().Be(today);
        (await client.GetFromJsonAsync<TodayResponse>("/api/today"))!.Items
            .Should().ContainSingle(i => i.Kind == "qualification-to-confirm" && i.RecordId == marked)
            .Which.Detail.Should().Be("Confirm they still qualify: income $65,000 → $58,000");

        var comparison = (await GetOverviewAsync(client, homeId)).WaitingList.Single(e => e.ApplicationId == marked);
        comparison.AnnualIncome.Should().Be(58000m);
        comparison.CanBeChosen.Should().BeFalse();
        comparison.Reasons.Should().Equal("QualificationToConfirm");

        var refused = await SelectByPriorityAsync(client, homeId, marked, templateId);
        refused.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        (await SelectByPriorityAsync(client, homeId, other, templateId)).StatusCode.Should().Be(HttpStatusCode.Created);
        var recorded = (await GetOverviewAsync(client, homeId)).Decisions.Single().Applications.Single(a => a.ApplicationId == marked);
        recorded.Outcome.Should().Be("CouldNotBeChosen");
        recorded.Reasons.Should().Equal("QualificationToConfirm");
        (await GetApplicationAsync(client, marked)).PassOvers.Should().BeEmpty("a household that couldn't be chosen isn't passed over");

        (await client.PostAsJsonAsync($"/api/homeowner-applications/{marked}/confirm-qualification",
            new ConfirmQualificationRequest(today)))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        (await GetApplicationAsync(client, marked)).Should().BeEquivalentTo(new
        {
            Phase = "Approved",
            ApprovedDate,
            AnnualIncome = 58000m,
            QualificationToConfirmSince = (DateOnly?)null,
            QualificationConfirmedDate = today,
        });
        (await GetOverviewAsync(client, await CreateHomeAsync(client))).WaitingList
            .Single(e => e.ApplicationId == marked).CanBeChosen.Should().BeTrue();
    }

    [Fact]
    public async Task AChangeToTheAccessibilityNeedAlone_UpdatesTheApplicationWithNoMark()
    {
        var client = factory.CreateClient().WithAuthToken();
        var householdId = await RegisterReadyHouseholdAsync(client);
        var applicationId = await SubmitAndApproveAsync(client, householdId);

        await UpdateCircumstancesAsync(client, householdId, 65000m, requiresAccessibility: true);

        (await GetApplicationAsync(client, applicationId)).Should().BeEquivalentTo(new
        {
            AccessibilityRequired = true,
            QualificationToConfirmSince = (DateOnly?)null,
        });
    }

    [Fact]
    public async Task ANewMemberOfASubmittedHousehold_ReachesTheApplicationWithNoMark()
    {
        var client = factory.CreateClient().WithAuthToken();
        var householdId = await RegisterReadyHouseholdAsync(client);
        var submitted = await client.PostAsJsonAsync("/api/homeowner-applications", new SubmitApplicationRequest(householdId));
        var applicationId = (await submitted.Content.ReadFromJsonAsync<SubmitApplicationResponse>())!.ApplicationId;

        await _tracker.TrackAsync(async () =>
            (await client.PostAsJsonAsync($"/api/households/{householdId}/members",
                new AddHouseholdMemberRequest("Second Member", $"second-{Guid.NewGuid():N}@example.com", "(202) 555-0144")))
                .StatusCode.Should().Be(HttpStatusCode.Created));

        (await GetApplicationAsync(client, applicationId)).Should().BeEquivalentTo(new
        {
            Phase = "Submitted",
            HouseholdSize = 2,
            QualificationToConfirmSince = (DateOnly?)null,
        });
    }

    private Task UpdateCircumstancesAsync(HttpClient client, string householdId, decimal annualIncome, bool requiresAccessibility) =>
        _tracker.TrackAsync(async () =>
            (await client.PutAsJsonAsync($"/api/households/{householdId}/circumstances",
                new UpdateHouseholdCircumstancesRequest(requiresAccessibility, annualIncome)))
                .StatusCode.Should().Be(HttpStatusCode.NoContent));

    private Task<HttpResponseMessage> SelectByPriorityAsync(HttpClient client, string homeId, string applicationId, string templateId) =>
        _tracker.TrackAsync(() =>
            client.PostAsJsonAsync($"/api/homes/{homeId}/home-selections/select-by-priority",
                new SelectHouseholdByPriorityRequest(
                    applicationId, ["WaitingListTenure"], "Longest on the waiting list", templateId, 200000m, SelectedDate)));

    private static async Task<HomeSelectionOverviewResponse> GetOverviewAsync(HttpClient client, string homeId) =>
        (await client.GetFromJsonAsync<HomeSelectionOverviewResponse>($"/api/homes/{homeId}/selection"))!;

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
                new RegisterLandParcelRequest($"APN-{Guid.NewGuid():N}", "12 Birch St", 0.25m, new DateOnly(2024, 1, 1), "Purchase", null));
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

    private static async Task<string> RegisterReadyHouseholdAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/households",
            new RegisterHouseholdRequest(
                [new HouseholdMemberRequest("Changing Applicant", $"changing-{Guid.NewGuid():N}@example.com", "(202) 555-0143", true)],
                false,
                65000m));
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var householdId = (await response.Content.ReadFromJsonAsync<RegisterHouseholdResponse>())!.HouseholdId;

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

    private Task<string> SubmitAndApproveAsync(HttpClient client, string householdId) =>
        _tracker.TrackAsync(async () =>
        {
            var submitted = await client.PostAsJsonAsync("/api/homeowner-applications", new SubmitApplicationRequest(householdId));
            submitted.StatusCode.Should().Be(HttpStatusCode.Created);
            var applicationId = (await submitted.Content.ReadFromJsonAsync<SubmitApplicationResponse>())!.ApplicationId;

            (await client.PostAsJsonAsync($"/api/homeowner-applications/{applicationId}/approve",
                new ApproveApplicationRequest(ApprovedDate, null)))
                .StatusCode.Should().Be(HttpStatusCode.NoContent);

            return applicationId;
        });
}
