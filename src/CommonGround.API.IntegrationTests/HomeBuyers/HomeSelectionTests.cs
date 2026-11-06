using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CommonGround.API.Contracts.HomeBuyers.Requests;
using CommonGround.API.Contracts.HomeBuyers.Responses;
using CommonGround.API.Contracts.Homeownership.Requests;
using CommonGround.API.Contracts.Homeownership.Responses;
using CommonGround.API.Contracts.PropertyRegistry.Requests;
using CommonGround.API.Contracts.PropertyRegistry.Responses;
using CommonGround.HomeBuyers.Application.Commands;
using CommonGround.HomeBuyers.Application.EventHandlers;
using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.HomeSelections;
using CommonGround.PropertyRegistry.Contracts.Events;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Infrastructure.API;
using FluentAssertions;
using Marten;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace CommonGround.API.IntegrationTests.HomeBuyers;

/// <summary>
/// Covers home selections end to end: starting one from the waiting list, choosing a household by
/// priority or lottery, cancelling, the pass-overs recorded for the other candidates, and the
/// hand-off to a purchase workflow.
///
/// The test database is shared across the collection, so every selection snapshots the approved
/// applications other tests left behind as well. Assertions are therefore limited to the
/// applications each test creates, and lotteries are drawn between tied candidates so the pool
/// is known.
///
/// As in <see cref="Homeownership.PurchaseWorkflowCloseTests"/>, the test host runs Wolverine in
/// MediatorOnly mode, so domain events are not forwarded as integration events. HomeBuyers's
/// HomeFactsView is seeded by calling its HomeRegistered handler directly; the hand-off to
/// Homeownership and PropertyRegistry is covered by <see cref="Flows.HomePurchaseFlowTests"/>.
/// </summary>
[Collection(IntegrationCollection.Name)]
public class HomeSelectionTests(ApiFactory factory)
{
    private const string ActiveSelectionCode = "home_buyers.home_selection.start.home_has_active_selection";
    private const string NotOpenCode = "home_buyers.home_selection.modify.not_open";

    private static readonly DateOnly SelectedDate = new(2026, 7, 20);

    [Fact]
    public async Task StartHomeSelection_SnapshotsApprovedApplicationsInWaitingListOrder()
    {
        var client = factory.CreateClient().WithAuthToken();
        var homeId = await CreateHomeAsync(client);

        var later = await CreateApprovedApplicationAsync(client, new DateOnly(2026, 5, 1), annualIncome: 52000m);
        var earlier = await CreateApprovedApplicationAsync(client, new DateOnly(2026, 3, 1), requiresAccessibility: true);
        var notReady = await CreateApprovedApplicationAsync(client, new DateOnly(2026, 4, 1), ready: false);
        var submitted = await CreateSubmittedApplicationAsync(client);

        var response = await client.PostAsync($"/api/homes/{homeId}/home-selections", null);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var selectionId = (await response.Content.ReadFromJsonAsync<StartHomeSelectionResponse>())!.SelectionId;
        response.Headers.Location!.ToString().Should().Be($"/api/home-selections/{selectionId}");

        var selection = await GetSelectionAsync(client, selectionId);
        selection.Should().BeEquivalentTo(new
        {
            HomeId = homeId,
            HomeBedrooms = 3,
            HomeIsAccessible = false,
            Status = "Open",
            SelectedApplicationId = (string?)null,
        });

        var ids = selection.Candidates.Select(c => c.ApplicationId).ToList();
        ids.Should().NotContain(submitted.ApplicationId);
        ids.Should().ContainInOrder(earlier.ApplicationId, notReady.ApplicationId, later.ApplicationId);

        var candidates = selection.Candidates.ToDictionary(c => c.ApplicationId);
        candidates[earlier.ApplicationId].Should().BeEquivalentTo(new
        {
            earlier.HouseholdId,
            AnnualIncome = 65000m,
            HouseholdSize = 1,
            RequiresAccessibility = true,
            WaitingSince = new DateOnly(2026, 3, 1),
            ApplicationPhase = "Approved",
            CanBeSelected = true,
        });
        candidates[earlier.ApplicationId].PrimaryContact!.Email.Should().Be(earlier.Email);
        candidates[later.ApplicationId].AnnualIncome.Should().Be(52000m);
        candidates[notReady.ApplicationId].CanBeSelected.Should().BeFalse();

        var summaries = await GetSelectionsForHomeAsync(client, homeId);
        summaries.Should().ContainSingle().Which.Should().BeEquivalentTo(new
        {
            Id = selectionId,
            Status = "Open",
            CandidateCount = selection.Candidates.Count,
        });
    }

    [Fact]
    public async Task StartHomeSelection_ForHomeHomeBuyersDoesNotKnow_Returns404()
    {
        var client = factory.CreateClient().WithAuthToken();

        var response = await client.PostAsync($"/api/homes/{Guid.NewGuid()}/home-selections", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task StartHomeSelection_WhenHomeHasActiveSelection_Returns422()
    {
        var client = factory.CreateClient().WithAuthToken();
        var homeId = await CreateHomeAsync(client);
        await CreateApprovedApplicationAsync(client, new DateOnly(2026, 6, 1));
        await StartSelectionAsync(client, homeId);

        var response = await client.PostAsync($"/api/homes/{homeId}/home-selections", null);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await ReadProblemCodeAsync(response)).Should().Be(ActiveSelectionCode);
    }

    [Fact]
    public async Task StartHomeSelection_WhenTwoStartsRacePastTheCheck_TheLaterSaveIsReportedAs409()
    {
        var client = factory.CreateClient().WithAuthToken();
        var homeId = await CreateHomeAsync(client);
        await CreateApprovedApplicationAsync(client, new DateOnly(2026, 6, 1));

        using var scope = factory.Services.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
        await using var first = store.LightweightSession(ApiFactory.TestTrustId);
        await using var second = store.LightweightSession(ApiFactory.TestTrustId);

        // Both starts check for an active selection before either is saved.
        var firstCommand = new StartHomeSelection(HomeSelectionId.Generate(), homeId, UserId.Generate());
        var secondCommand = firstCommand with { SelectionId = HomeSelectionId.Generate() };
        var firstStart = await StartHomeSelectionHandler.LoadAsync(firstCommand, first, CancellationToken.None);
        var secondStart = await StartHomeSelectionHandler.LoadAsync(secondCommand, second, CancellationToken.None);
        StartHomeSelectionHandler.Handle(firstCommand, firstStart).Execute(first);
        StartHomeSelectionHandler.Handle(secondCommand, secondStart).Execute(second);
        await first.SaveChangesAsync();

        var conflict = await FluentActions.Awaiting(() => second.SaveChangesAsync())
            .Should().ThrowAsync<Exception>();

        (await HandleExceptionAsync(scope.ServiceProvider, conflict.Which))
            .Should().Be(StatusCodes.Status409Conflict);
        (await GetSelectionsForHomeAsync(client, homeId)).Should().ContainSingle();
    }

    [Fact]
    public async Task SelectByPriority_SelectsTheChosenHouseholdAndPassesOverOnlyCandidatesStillEligible()
    {
        var client = factory.CreateClient().WithAuthToken();
        var homeId = await CreateHomeAsync(client);
        var templateId = await CreateActiveGroundLeaseTemplateAsync(client);

        var chosen = await CreateApprovedApplicationAsync(client, new DateOnly(2026, 6, 1));
        var eligible = await CreateApprovedApplicationAsync(client, new DateOnly(2026, 6, 1));
        var notReady = await CreateApprovedApplicationAsync(client, new DateOnly(2026, 6, 1), ready: false);
        var withdrawn = await CreateApprovedApplicationAsync(client, new DateOnly(2026, 6, 1));

        var selectionId = await StartSelectionAsync(client, homeId);
        (await client.PostAsJsonAsync($"/api/homeowner-applications/{withdrawn.ApplicationId}/withdraw",
            new WithdrawApplicationRequest(new DateOnly(2026, 7, 1), null)))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        var response = await client.PostAsJsonAsync($"/api/home-selections/{selectionId}/select-by-priority",
            new SelectHouseholdByPriorityRequest(
                chosen.ApplicationId,
                ["LowerIncome", "WaitingListTenure"],
                "Longest on the waiting list at the lowest income",
                templateId,
                200000m,
                SelectedDate));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var selection = await GetSelectionAsync(client, selectionId);
        selection.Should().BeEquivalentTo(new
        {
            Status = "Completed",
            SelectedApplicationId = chosen.ApplicationId,
            SelectionMethod = "Priority",
            Priorities = new[] { "LowerIncome", "WaitingListTenure" },
            SelectionReason = "Longest on the waiting list at the lowest income",
            LotterySeed = (int?)null,
        });
        selection.ClosedAt.Should().NotBeNull();

        var chosenApplication = await GetApplicationAsync(client, chosen.ApplicationId);
        chosenApplication.Should().BeEquivalentTo(new
        {
            Phase = "Selected",
            SelectedHomeId = homeId,
            SelectedDate,
        });
        chosenApplication.PassOvers.Should().BeEmpty();

        var eligibleApplication = await GetApplicationAsync(client, eligible.ApplicationId);
        eligibleApplication.Phase.Should().Be("Approved");
        eligibleApplication.PassOvers.Should().ContainSingle().Which.Should().BeEquivalentTo(new
        {
            eligible.ApplicationId,
            SelectionId = selectionId,
            HomeId = homeId,
            SelectionMethod = "Priority",
        });

        (await GetApplicationAsync(client, notReady.ApplicationId)).PassOvers.Should().BeEmpty();
        (await GetApplicationAsync(client, withdrawn.ApplicationId)).PassOvers.Should().BeEmpty();

        var waitingList = (await GetWholeWaitingListAsync(client)).ToDictionary(e => e.ApplicationId);
        waitingList.Should().NotContainKey(chosen.ApplicationId);
        waitingList[eligible.ApplicationId].PassOverCount.Should().Be(1);
        waitingList[notReady.ApplicationId].PassOverCount.Should().Be(0);
    }

    [Fact]
    public async Task SelectByPriority_WithUnrecognizedPriority_Returns400()
    {
        var client = factory.CreateClient().WithAuthToken();
        var homeId = await CreateHomeAsync(client);
        var applicant = await CreateApprovedApplicationAsync(client, new DateOnly(2026, 6, 1));
        var selectionId = await StartSelectionAsync(client, homeId);

        var response = await client.PostAsJsonAsync($"/api/home-selections/{selectionId}/select-by-priority",
            new SelectHouseholdByPriorityRequest(
                applicant.ApplicationId, ["Seniority"], "Reason", "template-1", 200000m, SelectedDate));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await GetSelectionAsync(client, selectionId)).Status.Should().Be("Open");
    }

    [Fact]
    public async Task DrawLottery_BetweenTiedCandidates_DrawsReproduciblyFromThoseStillEligible()
    {
        var client = factory.CreateClient().WithAuthToken();
        var homeId = await CreateHomeAsync(client);
        var templateId = await CreateActiveGroundLeaseTemplateAsync(client);

        var tiedA = await CreateApprovedApplicationAsync(client, new DateOnly(2026, 6, 1));
        var tiedB = await CreateApprovedApplicationAsync(client, new DateOnly(2026, 6, 1));
        var tiedNotReady = await CreateApprovedApplicationAsync(client, new DateOnly(2026, 6, 1), ready: false);
        var untied = await CreateApprovedApplicationAsync(client, new DateOnly(2026, 6, 1));

        var selectionId = await StartSelectionAsync(client, homeId);

        var response = await client.PostAsJsonAsync($"/api/home-selections/{selectionId}/draw-lottery",
            new DrawSelectionLotteryRequest(
                [tiedA.ApplicationId, tiedB.ApplicationId, tiedNotReady.ApplicationId],
                templateId,
                200000m,
                SelectedDate));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var selection = await GetSelectionAsync(client, selectionId);
        selection.Status.Should().Be("Completed");
        selection.SelectionMethod.Should().Be("Lottery");
        selection.LotteryPoolApplicationIds.Should().BeEquivalentTo(tiedA.ApplicationId, tiedB.ApplicationId);
        selection.LotteryIneligibleApplicationIds.Should().Equal(tiedNotReady.ApplicationId);
        selection.LotterySeed.Should().NotBeNull();

        var expectedWinner = SelectionLottery.Draw(
            [HomeownerApplicationId.Create(tiedA.ApplicationId), HomeownerApplicationId.Create(tiedB.ApplicationId)],
            selection.LotterySeed!.Value);
        selection.SelectedApplicationId.Should().Be(expectedWinner.Value);

        var loser = expectedWinner.Value == tiedA.ApplicationId ? tiedB : tiedA;
        (await GetApplicationAsync(client, expectedWinner.Value)).Phase.Should().Be("Selected");
        (await GetApplicationAsync(client, loser.ApplicationId)).PassOvers
            .Should().ContainSingle().Which.SelectionMethod.Should().Be("Lottery");
        (await GetApplicationAsync(client, untied.ApplicationId)).PassOvers
            .Should().ContainSingle("every eligible candidate is passed over, tied or not");
        (await GetApplicationAsync(client, tiedNotReady.ApplicationId)).PassOvers.Should().BeEmpty();
    }

    [Fact]
    public async Task CancelHomeSelection_ClosesItWithoutChoosingAnyoneAndFreesTheHome()
    {
        var client = factory.CreateClient().WithAuthToken();
        var homeId = await CreateHomeAsync(client);
        var applicant = await CreateApprovedApplicationAsync(client, new DateOnly(2026, 6, 1));
        var cancelledId = await StartSelectionAsync(client, homeId);

        var response = await client.PostAsJsonAsync($"/api/home-selections/{cancelledId}/cancel",
            new CancelHomeSelectionRequest("Home withdrawn from sale"));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var cancelled = await GetSelectionAsync(client, cancelledId);
        cancelled.Should().BeEquivalentTo(new
        {
            Status = "Cancelled",
            CancellationReason = "Home withdrawn from sale",
            SelectedApplicationId = (string?)null,
            SelectionMethod = (string?)null,
        });
        cancelled.ClosedAt.Should().NotBeNull();

        var application = await GetApplicationAsync(client, applicant.ApplicationId);
        application.Phase.Should().Be("Approved");
        application.PassOvers.Should().BeEmpty();

        var cancelAgain = await client.PostAsJsonAsync($"/api/home-selections/{cancelledId}/cancel",
            new CancelHomeSelectionRequest("Again"));
        cancelAgain.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await ReadProblemCodeAsync(cancelAgain)).Should().Be(NotOpenCode);

        var restartedId = await StartSelectionAsync(client, homeId);

        (await GetSelectionsForHomeAsync(client, homeId)).Select(s => (s.Id, s.Status))
            .Should().Equal((restartedId, "Open"), (cancelledId, "Cancelled"));
    }

    [Fact]
    public async Task PassOvers_FollowTheHouseholdToItsLaterApplications()
    {
        var client = factory.CreateClient().WithAuthToken();
        var templateId = await CreateActiveGroundLeaseTemplateAsync(client);
        var passedOver = await CreateApprovedApplicationAsync(client, new DateOnly(2026, 6, 1));

        var firstSelectionId = await SelectSomeoneElseAsync(client, templateId);

        (await client.PostAsJsonAsync($"/api/homeowner-applications/{passedOver.ApplicationId}/withdraw",
            new WithdrawApplicationRequest(new DateOnly(2026, 7, 21), null)))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);
        var reapplicationId = await SubmitAsync(client, passedOver.HouseholdId);
        await ApproveAsync(client, reapplicationId, new DateOnly(2026, 8, 1));

        var secondSelectionId = await SelectSomeoneElseAsync(client, templateId);

        var reapplication = await GetApplicationAsync(client, reapplicationId);
        reapplication.PassOvers.Select(p => (p.ApplicationId, p.SelectionId)).Should().Equal(
            (passedOver.ApplicationId, firstSelectionId),
            (reapplicationId, secondSelectionId));

        var waitingList = (await GetWholeWaitingListAsync(client)).ToDictionary(e => e.ApplicationId);
        waitingList[reapplicationId].PassOverCount.Should().Be(2);
    }

    [Fact]
    public async Task GetHomeSelections_FiltersByStatusAcrossHomes()
    {
        var client = factory.CreateClient().WithAuthToken();

        var homeId = await CreateHomeAsync(client);
        await CreateApprovedApplicationAsync(client, new DateOnly(2026, 6, 1));
        var selectionId = await StartSelectionAsync(client, homeId);

        var open = await client.GetFromJsonAsync<PaginatedResponse<HomeSelectionListItemResponse>>(
            $"/api/home-selections{new GetHomeSelectionsQuerySpec(1, GetHomeSelectionsQuerySpec.MaxPageSize, "Open").ToQueryString()}");
        open!.Items.Should().OnlyContain(s => s.Status == "Open");
        open.Items.Should().ContainSingle(s => s.Id == selectionId).Which.HomeLabel.Should().Be("1 Test Lane");

        var cancelled = await client.GetFromJsonAsync<PaginatedResponse<HomeSelectionListItemResponse>>(
            "/api/home-selections?status=Cancelled");
        cancelled!.Items.Should().NotContain(s => s.Id == selectionId);
    }

    /// <summary>Starts a selection for a new home and chooses a new applicant, passing over everyone else eligible.</summary>
    private async Task<string> SelectSomeoneElseAsync(HttpClient client, string templateId)
    {
        var homeId = await CreateHomeAsync(client);
        var chosen = await CreateApprovedApplicationAsync(client, new DateOnly(2026, 6, 1));
        var selectionId = await StartSelectionAsync(client, homeId);
        await SelectByPriorityAsync(client, selectionId, chosen.ApplicationId, templateId);
        return selectionId;
    }

    private static async Task SelectByPriorityAsync(HttpClient client, string selectionId, string applicationId, string templateId)
    {
        (await client.PostAsJsonAsync($"/api/home-selections/{selectionId}/select-by-priority",
            new SelectHouseholdByPriorityRequest(
                applicationId, ["WaitingListTenure"], "Longest on the waiting list", templateId, 200000m, SelectedDate)))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    /// <summary>Runs <paramref name="exception"/> through the API's exception handlers, as the middleware does, and returns the status code written.</summary>
    private static async Task<int> HandleExceptionAsync(IServiceProvider services, Exception exception)
    {
        var httpContext = new DefaultHttpContext { RequestServices = services };
        httpContext.Response.Body = new MemoryStream();

        foreach (var handler in services.GetServices<IExceptionHandler>())
        {
            if (await handler.TryHandleAsync(httpContext, exception, CancellationToken.None))
                return httpContext.Response.StatusCode;
        }

        throw new InvalidOperationException($"No exception handler handled {exception.GetType().Name}.", exception);
    }

    private static async Task<string?> ReadProblemCodeAsync(HttpResponseMessage response)
    {
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        return problem.TryGetProperty("code", out var code) ? code.GetString() : null;
    }

    private static async Task<string> StartSelectionAsync(HttpClient client, string homeId)
    {
        var response = await client.PostAsync($"/api/homes/{homeId}/home-selections", null);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<StartHomeSelectionResponse>())!.SelectionId;
    }

    private static async Task<HomeSelectionResponse> GetSelectionAsync(HttpClient client, string selectionId) =>
        (await client.GetFromJsonAsync<HomeSelectionResponse>($"/api/home-selections/{selectionId}"))!;

    private static async Task<List<HomeSelectionSummaryResponse>> GetSelectionsForHomeAsync(HttpClient client, string homeId) =>
        (await client.GetFromJsonAsync<List<HomeSelectionSummaryResponse>>($"/api/homes/{homeId}/home-selections"))!;

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

    /// <summary>Registers a home and seeds HomeBuyers's view of it, which MediatorOnly mode would not forward.</summary>
    private async Task<string> CreateHomeAsync(HttpClient client, string? landParcelId = null)
    {
        landParcelId ??= await CreateLandParcelAsync(client);

        var response = await client.PostAsJsonAsync("/api/homes",
            new RegisterHomeRequest(
                landParcelId,
                "SingleFamily",
                3,
                2m,
                false,
                1200m,
                1990,
                "Test Appraiser",
                new DateOnly(2024, 1, 1),
                280000m));
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var homeId = (await response.Content.ReadFromJsonAsync<RegisterHomeResponse>())!.HomeId;

        using var scope = factory.Services.CreateScope();
        await using var session = scope.ServiceProvider.GetRequiredService<IDocumentStore>().LightweightSession(ApiFactory.TestTrustId);
        HomeRegisteredHandler.Handle(
            new HomeRegistered(homeId, landParcelId, Label: "1 Test Lane", Bedrooms: 3, IsAccessible: false))
            .Execute(session);
        await session.SaveChangesAsync();

        return homeId;
    }

    private static async Task<string> CreateLandParcelAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/land-parcels",
            new RegisterLandParcelRequest(
                $"APN-{Guid.NewGuid():N}",
                "789 Elm St",
                0.25m,
                new DateOnly(2024, 1, 1),
                "Purchase",
                null));
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<RegisterLandParcelResponse>())!.ParcelId;
    }

    private static async Task<string> CreateActiveGroundLeaseTemplateAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/ground-lease-templates",
            new CreateGroundLeaseTemplateRequest(
                $"Standard 99-Year {Guid.NewGuid():N}",
                99,
                60,
                "Standard covenant terms",
                "AppraisalBased",
                0.25m,
                null,
                null,
                null,
                null));
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<CreateGroundLeaseTemplateResponse>())!.GroundLeaseTemplateId;
    }

    private sealed record Applicant(string ApplicationId, string HouseholdId, string FullName, string Email);

    /// <summary>
    /// Registers a one-member household and submits and approves its application. A
    /// <paramref name="ready"/> household has finished counseling and orientation and holds a
    /// prequalification that never expires, so it can always be selected; otherwise it has none of them.
    /// </summary>
    private static async Task<Applicant> CreateApprovedApplicationAsync(
        HttpClient client,
        DateOnly approvedDate,
        bool ready = true,
        decimal annualIncome = 65000m,
        bool requiresAccessibility = false)
    {
        var applicant = await RegisterHouseholdAsync(client, annualIncome, requiresAccessibility);
        if (ready)
        {
            await RecordReadinessAsync(client, applicant.HouseholdId);
        }

        var applicationId = await SubmitAsync(client, applicant.HouseholdId);
        await ApproveAsync(client, applicationId, approvedDate);
        return applicant with { ApplicationId = applicationId };
    }

    private static async Task<Applicant> CreateSubmittedApplicationAsync(HttpClient client)
    {
        var applicant = await RegisterHouseholdAsync(client, 65000m, false);
        return applicant with { ApplicationId = await SubmitAsync(client, applicant.HouseholdId) };
    }

    private static async Task<Applicant> RegisterHouseholdAsync(HttpClient client, decimal annualIncome, bool requiresAccessibility)
    {
        var fullName = "Selection Applicant";
        var email = $"selection-{Guid.NewGuid():N}@example.com";

        var response = await client.PostAsJsonAsync("/api/households",
            new RegisterHouseholdRequest(
                [new HouseholdMemberRequest(fullName, email, "(202) 555-0100", true)],
                requiresAccessibility,
                annualIncome));
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var householdId = (await response.Content.ReadFromJsonAsync<RegisterHouseholdResponse>())!.HouseholdId;

        return new Applicant(string.Empty, householdId, fullName, email);
    }

    private static async Task RecordReadinessAsync(HttpClient client, string householdId)
    {
        (await client.PostAsJsonAsync($"/api/households/{householdId}/housing-counseling",
            new RecordHousingCounselingRequest("CERT-001", new DateOnly(2026, 1, 2), "Test Counseling Org")))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await client.PostAsJsonAsync($"/api/households/{householdId}/clt-orientation",
            new RecordCltOrientationRequest(new DateOnly(2026, 1, 3))))
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

    private static async Task ApproveAsync(HttpClient client, string applicationId, DateOnly approvedDate)
    {
        (await client.PostAsJsonAsync($"/api/homeowner-applications/{applicationId}/approve",
            new ApproveApplicationRequest(approvedDate, null)))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
