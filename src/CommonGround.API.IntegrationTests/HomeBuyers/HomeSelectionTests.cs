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
using CommonGround.HomeBuyers.Application.ReadModels;
using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.HomeSelections;
using CommonGround.PropertyRegistry.Contracts.Events;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Services;
using CommonGround.SharedKernel.Infrastructure.API;
using FluentAssertions;
using Marten;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace CommonGround.API.IntegrationTests.HomeBuyers;

/// <summary>
/// Covers home selections end to end: comparing the waiting list as it is against a home, and choosing a household
/// by priority or lottery in one step, recorded with the whole waiting list.
///
/// The test database is shared across the collection, so every comparison includes the approved applications other
/// tests left behind as well. Assertions are therefore limited to the applications each test creates, and lotteries
/// are drawn between tied households so the pool is known.
///
/// As in <see cref="Homeownership.PurchaseWorkflowCloseTests"/>, the test host runs Wolverine in MediatorOnly mode, so
/// domain events are not forwarded as integration events. HomeBuyers's HomeFactsView is seeded by calling its
/// HomeRegistered handler directly, and PropertyRegistry never hears of a choice here, so a home stays chosen. The
/// pass-overs that follow a choice are covered by <see cref="Flows.HomeSelectionPassOverFlowTests"/>, and the hand-off
/// to Homeownership and PropertyRegistry by <see cref="Flows.HomePurchaseFlowTests"/>.
/// </summary>
[Collection(IntegrationCollection.Name)]
public class HomeSelectionTests(ApiFactory factory)
{
    private const string HomeNotAvailableCode = "home_buyers.home_selection.choose.home_not_available";

    private static readonly DateOnly SelectedDate = new(2026, 7, 20);

    [Fact]
    public async Task GetSelection_ComparesTheWaitingListAsItIsAgainstTheHome_InWaitingListOrder()
    {
        var client = factory.CreateClient().WithAuthToken();
        var homeId = await CreateHomeAsync(client);

        var later = await CreateApprovedApplicationAsync(client, new DateOnly(2026, 5, 1), annualIncome: 52000m);
        var earlier = await CreateApprovedApplicationAsync(client, new DateOnly(2026, 3, 1), requiresAccessibility: true);
        var notReady = await CreateApprovedApplicationAsync(client, new DateOnly(2026, 4, 1), ready: false);
        var submitted = await CreateSubmittedApplicationAsync(client);

        var overview = await GetOverviewAsync(client, homeId);

        overview.Should().BeEquivalentTo(new
        {
            HomeId = homeId,
            HomeLabel = "1 Test Lane",
            HomeBedrooms = 3,
            HomeBathrooms = 2m,
            HomeSquareFootage = 1200m,
            HomeIsAccessible = false,
            SaleStage = "Offered",
            IsOffered = true,
            HasIncomeLimitsInEffect = true,
        });
        overview.Decisions.Should().BeEmpty();

        var ids = overview.WaitingList.Select(c => c.ApplicationId).ToList();
        ids.Should().NotContain(submitted.ApplicationId);
        ids.Should().ContainInOrder(earlier.ApplicationId, notReady.ApplicationId, later.ApplicationId);

        var waiting = overview.WaitingList.ToDictionary(c => c.ApplicationId);
        waiting[earlier.ApplicationId].Should().BeEquivalentTo(new
        {
            earlier.HouseholdId,
            AnnualIncome = 65000m,
            HouseholdSize = 1,
            RequiresAccessibility = true,
            WaitingSince = new DateOnly(2026, 3, 1),
            CanBeChosen = true,
            Reasons = Array.Empty<string>(),
            PrequalifiedAmount = (decimal?)210000m,
        });
        waiting[earlier.ApplicationId].PrimaryContact!.Email.Should().Be(earlier.Email);
        waiting[later.ApplicationId].AnnualIncome.Should().Be(52000m);
        waiting[notReady.ApplicationId].CanBeChosen.Should().BeFalse();
        waiting[notReady.ApplicationId].Reasons.Should().Equal(
            "HousingCounselingIncomplete", "CltOrientationIncomplete", "NoPrequalification");
        waiting[notReady.ApplicationId].PrequalifiedAmount.Should().BeNull();
    }

    [Fact]
    public async Task GetSelection_ForAHomeAvailableForAMonth_IncludesAHouseholdApprovedYesterday()
    {
        var client = factory.CreateClient().WithAuthToken();
        var today = await TodayAsync();
        var homeId = await CreateHomeAsync(client);
        await RecordSaleStageAsync(homeId, "Offered", since: today.AddDays(-30), sequence: 1, latestSelectionId: null);

        var approvedYesterday = await CreateApprovedApplicationAsync(client, today.AddDays(-1));

        var overview = await GetOverviewAsync(client, homeId);

        overview.IsOffered.Should().BeTrue();
        overview.WaitingList.Should().ContainSingle(c => c.ApplicationId == approvedYesterday.ApplicationId)
            .Which.Should().BeEquivalentTo(new { WaitingSince = today.AddDays(-1), CanBeChosen = true });
    }

    [Fact]
    public async Task GetSelection_ForHomeWithIncomeLimit_JudgesEachHouseholdAgainstIt()
    {
        var client = factory.CreateClient().WithAuthToken();
        var homeId = await CreateHomeAsync(client, incomeLimitPercentOfAmi: 80);
        var within = await CreateApprovedApplicationAsync(client, new DateOnly(2026, 6, 1), annualIncome: 70_000m);
        var over = await CreateApprovedApplicationAsync(client, new DateOnly(2026, 6, 1), annualIncome: 70_001m);

        var overview = await GetOverviewAsync(client, homeId);

        overview.IncomeLimitPercentOfAmi.Should().Be(80);
        overview.IncomeLimitsEffectiveFrom.Should().Be(TestIncomeLimitsInitialData.EffectiveFrom);
        var waiting = overview.WaitingList.ToDictionary(c => c.ApplicationId);
        var onePersonLimit = TestIncomeLimitsInitialData.EightyPercentLimits[0];
        waiting[within.ApplicationId].Should().BeEquivalentTo(new { IncomeLimit = onePersonLimit, CanBeChosen = true });
        waiting[over.ApplicationId].Should().BeEquivalentTo(new { IncomeLimit = onePersonLimit, CanBeChosen = false, Reasons = new[] { "OverIncomeLimit" } });
    }

    [Fact]
    public async Task GetSelection_ForHomeWithNoIncomeLimit_JudgesNobodysIncome()
    {
        var client = factory.CreateClient().WithAuthToken();
        var homeId = await CreateHomeAsync(client, incomeLimitPercentOfAmi: null);
        var applicant = await CreateApprovedApplicationAsync(client, new DateOnly(2026, 6, 1), annualIncome: 500_000m);

        var overview = await GetOverviewAsync(client, homeId);

        overview.IncomeLimitPercentOfAmi.Should().BeNull();
        overview.IncomeLimitsEffectiveFrom.Should().Be(TestIncomeLimitsInitialData.EffectiveFrom);
        overview.WaitingList.Single(c => c.ApplicationId == applicant.ApplicationId)
            .Should().BeEquivalentTo(new { IncomeLimit = (decimal?)null, CanBeChosen = true });
    }

    [Fact]
    public async Task SelectByPriority_ForHomeWithALevelNoScheduleInEffectHas_Returns422()
    {
        var client = factory.CreateClient().WithAuthToken();
        var homeId = await CreateHomeAsync(client, incomeLimitPercentOfAmi: 60);
        var templateId = await CreateActiveGroundLeaseTemplateAsync(client);
        var applicant = await CreateApprovedApplicationAsync(client, new DateOnly(2026, 6, 1));

        (await GetOverviewAsync(client, homeId)).HasIncomeLimitsInEffect.Should().BeFalse();

        var response = await SelectByPriorityAsync(client, homeId, applicant.ApplicationId, templateId);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await ReadProblemCodeAsync(response)).Should().Be("home_buyers.home_selection.choose.no_income_limits_in_effect");
        (await GetOverviewAsync(client, homeId)).Decisions.Should().BeEmpty();
    }

    [Fact]
    public async Task SelectByPriority_ForHouseholdOverIncomeLimit_Returns422()
    {
        var client = factory.CreateClient().WithAuthToken();
        var homeId = await CreateHomeAsync(client, incomeLimitPercentOfAmi: 80);
        var templateId = await CreateActiveGroundLeaseTemplateAsync(client);
        var over = await CreateApprovedApplicationAsync(client, new DateOnly(2026, 6, 1), annualIncome: 90_000m);

        var response = await SelectByPriorityAsync(client, homeId, over.ApplicationId, templateId);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await ReadProblemCodeAsync(response)).Should().Be("home_buyers.home_selection.choose.cannot_be_chosen");
        (await GetOverviewAsync(client, homeId)).Decisions.Should().BeEmpty();
    }

    [Fact]
    public async Task SelectByPriority_DatedAfterToday_Returns422()
    {
        var client = factory.CreateClient().WithAuthToken();
        var homeId = await CreateHomeAsync(client);
        var templateId = await CreateActiveGroundLeaseTemplateAsync(client);
        var applicant = await CreateApprovedApplicationAsync(client, new DateOnly(2026, 6, 1));

        var response = await SelectByPriorityAsync(client, homeId, applicant.ApplicationId, templateId, (await TodayAsync()).AddDays(1));

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await ReadProblemCodeAsync(response)).Should().Be("home_buyers.home_selection.choose.selected_date_in_future");
    }

    [Fact]
    public async Task SelectByPriority_DatedBeforeTheHomeBecameAvailable_Returns422()
    {
        var client = factory.CreateClient().WithAuthToken();
        var homeId = await CreateHomeAsync(client);
        await RecordSaleStageAsync(homeId, "Offered", since: SelectedDate.AddDays(1), sequence: 1, latestSelectionId: null);
        var templateId = await CreateActiveGroundLeaseTemplateAsync(client);
        var applicant = await CreateApprovedApplicationAsync(client, new DateOnly(2026, 6, 1));

        var response = await SelectByPriorityAsync(client, homeId, applicant.ApplicationId, templateId);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await ReadProblemCodeAsync(response)).Should().Be("home_buyers.home_selection.choose.selected_date_before_available");
    }

    [Fact]
    public async Task SelectByPriority_ForHomeHomeBuyersDoesNotKnow_Returns404()
    {
        var client = factory.CreateClient().WithAuthToken();

        (await client.GetAsync($"/api/homes/{Guid.NewGuid()}/selection")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await SelectByPriorityAsync(client, Guid.NewGuid().ToString(), HomeownerApplicationId.Generate().Value, "template-1"))
            .StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SelectByPriority_RecordsTheWholeWaitingListAndSelectsTheChosenHousehold()
    {
        var client = factory.CreateClient().WithAuthToken();
        var homeId = await CreateHomeAsync(client);
        var templateId = await CreateActiveGroundLeaseTemplateAsync(client);

        var chosen = await CreateApprovedApplicationAsync(client, new DateOnly(2026, 6, 1));
        var passedOver = await CreateApprovedApplicationAsync(client, new DateOnly(2026, 6, 1));
        var notReady = await CreateApprovedApplicationAsync(client, new DateOnly(2026, 6, 1), ready: false);

        var response = await client.PostAsJsonAsync($"/api/homes/{homeId}/home-selections/select-by-priority",
            new SelectHouseholdByPriorityRequest(
                chosen.ApplicationId,
                ["LowerIncome", "WaitingListTenure"],
                "Longest on the waiting list at the lowest income",
                templateId,
                200000m,
                SelectedDate));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var selectionId = (await response.Content.ReadFromJsonAsync<HomeSelectionCreatedResponse>())!.SelectionId;
        response.Headers.Location!.ToString().Should().Be($"/api/homes/{homeId}/selection");

        var overview = await GetOverviewAsync(client, homeId);
        overview.Should().BeEquivalentTo(new { SaleStage = "UnderContract", IsOffered = false });
        var decision = overview.Decisions.Should().ContainSingle().Subject;
        decision.Should().BeEquivalentTo(new
        {
            Id = selectionId,
            SelectedApplicationId = chosen.ApplicationId,
            SelectedHouseholdId = chosen.HouseholdId,
            SelectedDate,
            SelectionMethod = "Priority",
            GroundLeaseTemplateId = templateId,
            PurchasePrice = 200000m,
            Priorities = new[] { "LowerIncome", "WaitingListTenure" },
            SelectionReason = "Longest on the waiting list at the lowest income",
            LotterySeed = (int?)null,
        });
        var outcomes = decision.Applications.ToDictionary(a => a.ApplicationId);
        outcomes[chosen.ApplicationId].Outcome.Should().Be("Chosen");
        outcomes[passedOver.ApplicationId].Outcome.Should().Be("PassedOver");
        outcomes[notReady.ApplicationId].Should().BeEquivalentTo(new
        {
            Outcome = "CouldNotBeChosen",
            Reasons = new[] { "HousingCounselingIncomplete", "CltOrientationIncomplete", "NoPrequalification" },
        });
        outcomes[chosen.ApplicationId].PrimaryContact!.Email.Should().Be(chosen.Email);

        var chosenApplication = await GetApplicationAsync(client, chosen.ApplicationId);
        chosenApplication.Should().BeEquivalentTo(new
        {
            Phase = "Selected",
            SelectedHomeId = homeId,
            SelectedDate,
        });
        chosenApplication.PassOvers.Should().BeEmpty();

        (await GetWholeWaitingListAsync(client)).Should().NotContain(e => e.ApplicationId == chosen.ApplicationId);
        overview.WaitingList.Should().NotContain(e => e.ApplicationId == chosen.ApplicationId);
    }

    [Fact]
    public async Task SelectByPriority_ForAHomeAHouseholdWasAlreadyChosenFor_Returns422()
    {
        var client = factory.CreateClient().WithAuthToken();
        var homeId = await CreateHomeAsync(client);
        var templateId = await CreateActiveGroundLeaseTemplateAsync(client);
        var first = await CreateApprovedApplicationAsync(client, new DateOnly(2026, 6, 1));
        var second = await CreateApprovedApplicationAsync(client, new DateOnly(2026, 6, 1));
        (await SelectByPriorityAsync(client, homeId, first.ApplicationId, templateId)).StatusCode.Should().Be(HttpStatusCode.Created);

        var response = await SelectByPriorityAsync(client, homeId, second.ApplicationId, templateId);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await ReadProblemCodeAsync(response)).Should().Be(HomeNotAvailableCode);
        (await GetOverviewAsync(client, homeId)).Decisions.Should().ContainSingle();
        (await GetApplicationAsync(client, second.ApplicationId)).Phase.Should().Be("Approved");
    }

    [Fact]
    public async Task SelectByPriority_ForAHomeThatIsNotOffered_Returns422()
    {
        var client = factory.CreateClient().WithAuthToken();
        var homeId = await CreateHomeAsync(client);
        await RecordSaleStageAsync(homeId, "HeldByTrust", since: new DateOnly(2026, 6, 1), sequence: 1, latestSelectionId: null);
        var templateId = await CreateActiveGroundLeaseTemplateAsync(client);
        var applicant = await CreateApprovedApplicationAsync(client, new DateOnly(2026, 6, 1));

        var response = await SelectByPriorityAsync(client, homeId, applicant.ApplicationId, templateId);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await ReadProblemCodeAsync(response)).Should().Be(HomeNotAvailableCode);
    }

    [Fact]
    public async Task SelectByPriority_WhenTwoChoicesForTheSameHomeRace_TheLaterSaveIsReportedAs409()
    {
        var client = factory.CreateClient().WithAuthToken();
        var homeId = await CreateHomeAsync(client);
        var templateId = await CreateActiveGroundLeaseTemplateAsync(client);
        var firstChoice = await CreateApprovedApplicationAsync(client, new DateOnly(2026, 6, 1));
        var secondChoice = await CreateApprovedApplicationAsync(client, new DateOnly(2026, 6, 1));

        using var scope = factory.Services.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
        var trustClock = scope.ServiceProvider.GetRequiredService<ITrustClock>();
        await using var first = store.LightweightSession(ApiFactory.TestTrustId);
        await using var second = store.LightweightSession(ApiFactory.TestTrustId);

        // Both choices find the home offered before either is saved.
        var firstCommand = PriorityCommand(homeId, firstChoice.ApplicationId, templateId);
        var secondCommand = PriorityCommand(homeId, secondChoice.ApplicationId, templateId);
        var firstLoad = await SelectHouseholdByPriorityHandler.LoadAsync(firstCommand, first, trustClock, CancellationToken.None);
        var secondLoad = await SelectHouseholdByPriorityHandler.LoadAsync(secondCommand, second, trustClock, CancellationToken.None);
        await SelectHouseholdByPriorityHandler.Handle(firstCommand, firstLoad, first, CancellationToken.None);
        await SelectHouseholdByPriorityHandler.Handle(secondCommand, secondLoad, second, CancellationToken.None);
        await first.SaveChangesAsync();

        var conflict = await FluentActions.Awaiting(() => second.SaveChangesAsync())
            .Should().ThrowAsync<Exception>();

        (await HandleExceptionAsync(scope.ServiceProvider, conflict.Which))
            .Should().Be(StatusCodes.Status409Conflict);
        (await GetOverviewAsync(client, homeId)).Decisions.Should().ContainSingle()
            .Which.SelectedApplicationId.Should().Be(firstChoice.ApplicationId);
        (await GetApplicationAsync(client, secondChoice.ApplicationId)).Phase.Should().Be("Approved");
    }

    [Fact]
    public async Task SelectByPriority_WhenChoicesForHomesSharingAWaitingListAreRecordedAtOnce_BothSucceed()
    {
        var client = factory.CreateClient().WithAuthToken();
        var firstHomeId = await CreateHomeAsync(client);
        var secondHomeId = await CreateHomeAsync(client);
        var templateId = await CreateActiveGroundLeaseTemplateAsync(client);

        var firstChoice = await CreateApprovedApplicationAsync(client, new DateOnly(2026, 6, 1));
        var secondChoice = await CreateApprovedApplicationAsync(client, new DateOnly(2026, 6, 1));

        using var scope = factory.Services.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
        var trustClock = scope.ServiceProvider.GetRequiredService<ITrustClock>();
        await using var first = store.LightweightSession(ApiFactory.TestTrustId);
        await using var second = store.LightweightSession(ApiFactory.TestTrustId);

        // Both choices compare a waiting list holding both households before either is saved.
        var firstCommand = PriorityCommand(firstHomeId, firstChoice.ApplicationId, templateId);
        var secondCommand = PriorityCommand(secondHomeId, secondChoice.ApplicationId, templateId);
        var firstLoad = await SelectHouseholdByPriorityHandler.LoadAsync(firstCommand, first, trustClock, CancellationToken.None);
        var secondLoad = await SelectHouseholdByPriorityHandler.LoadAsync(secondCommand, second, trustClock, CancellationToken.None);
        await SelectHouseholdByPriorityHandler.Handle(firstCommand, firstLoad, first, CancellationToken.None);
        await SelectHouseholdByPriorityHandler.Handle(secondCommand, secondLoad, second, CancellationToken.None);
        await first.SaveChangesAsync();
        await second.SaveChangesAsync();

        (await GetOverviewAsync(client, firstHomeId)).Decisions.Single().SelectedApplicationId.Should().Be(firstChoice.ApplicationId);
        (await GetOverviewAsync(client, secondHomeId)).Decisions.Single().SelectedApplicationId.Should().Be(secondChoice.ApplicationId);
        (await GetApplicationAsync(client, firstChoice.ApplicationId)).SelectedHomeId.Should().Be(firstHomeId);
        (await GetApplicationAsync(client, secondChoice.ApplicationId)).SelectedHomeId.Should().Be(secondHomeId);
    }

    [Fact]
    public async Task SubmitApplication_ForHouseholdSelectedForAHome_Returns422()
    {
        var client = factory.CreateClient().WithAuthToken();
        var homeId = await CreateHomeAsync(client);
        var templateId = await CreateActiveGroundLeaseTemplateAsync(client);
        var chosen = await CreateApprovedApplicationAsync(client, new DateOnly(2026, 6, 1));
        (await SelectByPriorityAsync(client, homeId, chosen.ApplicationId, templateId)).StatusCode.Should().Be(HttpStatusCode.Created);

        var response = await client.PostAsJsonAsync("/api/homeowner-applications", new SubmitApplicationRequest(chosen.HouseholdId));

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await ReadProblemCodeAsync(response)).Should().Be("home_buyers.homeowner_application.submit.household_has_open_application");
    }

    [Fact]
    public async Task SelectByPriority_WithUnrecognizedPriority_Returns400()
    {
        var client = factory.CreateClient().WithAuthToken();
        var homeId = await CreateHomeAsync(client);
        var applicant = await CreateApprovedApplicationAsync(client, new DateOnly(2026, 6, 1));

        var response = await client.PostAsJsonAsync($"/api/homes/{homeId}/home-selections/select-by-priority",
            new SelectHouseholdByPriorityRequest(
                applicant.ApplicationId, ["Seniority"], "Reason", "template-1", 200000m, SelectedDate));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await GetOverviewAsync(client, homeId)).Decisions.Should().BeEmpty();
    }

    [Fact]
    public async Task DrawLottery_BetweenTiedHouseholds_DrawsReproduciblyFromThoseThatCanBeChosen()
    {
        var client = factory.CreateClient().WithAuthToken();
        var homeId = await CreateHomeAsync(client);
        var templateId = await CreateActiveGroundLeaseTemplateAsync(client);

        var tiedA = await CreateApprovedApplicationAsync(client, new DateOnly(2026, 6, 1));
        var tiedB = await CreateApprovedApplicationAsync(client, new DateOnly(2026, 6, 1));
        var tiedNotReady = await CreateApprovedApplicationAsync(client, new DateOnly(2026, 6, 1), ready: false);
        var untied = await CreateApprovedApplicationAsync(client, new DateOnly(2026, 6, 1));

        var response = await client.PostAsJsonAsync($"/api/homes/{homeId}/home-selections/draw-lottery",
            new DrawSelectionLotteryRequest(
                [tiedA.ApplicationId, tiedB.ApplicationId, tiedNotReady.ApplicationId],
                templateId,
                200000m,
                SelectedDate));

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var decision = (await GetOverviewAsync(client, homeId)).Decisions.Should().ContainSingle().Subject;
        decision.SelectionMethod.Should().Be("Lottery");
        decision.LotteryTiedApplicationIds.Should().BeEquivalentTo(tiedA.ApplicationId, tiedB.ApplicationId, tiedNotReady.ApplicationId);
        decision.LotteryPoolApplicationIds.Should().BeEquivalentTo(tiedA.ApplicationId, tiedB.ApplicationId);
        decision.LotterySeed.Should().NotBeNull();

        var expectedWinner = SelectionLottery.Draw(
            [HomeownerApplicationId.Create(tiedA.ApplicationId), HomeownerApplicationId.Create(tiedB.ApplicationId)],
            decision.LotterySeed!.Value);
        decision.SelectedApplicationId.Should().Be(expectedWinner.Value);
        (await GetApplicationAsync(client, expectedWinner.Value)).Phase.Should().Be("Selected");

        var outcomes = decision.Applications.ToDictionary(a => a.ApplicationId);
        outcomes[tiedNotReady.ApplicationId].Outcome.Should().Be("CouldNotBeChosen");
        outcomes[untied.ApplicationId].Outcome.Should().Be("PassedOver");
    }

    [Fact]
    public async Task GetHomeSelections_ListsTheChoicesAcrossHomesNewestFirst()
    {
        var client = factory.CreateClient().WithAuthToken();
        var templateId = await CreateActiveGroundLeaseTemplateAsync(client);
        var firstHomeId = await CreateHomeAsync(client);
        var secondHomeId = await CreateHomeAsync(client);
        var firstChoice = await CreateApprovedApplicationAsync(client, new DateOnly(2026, 6, 1));
        var secondChoice = await CreateApprovedApplicationAsync(client, new DateOnly(2026, 6, 1));
        var earlier = await ReadCreatedIdAsync(await SelectByPriorityAsync(client, firstHomeId, firstChoice.ApplicationId, templateId));
        var later = await ReadCreatedIdAsync(await SelectByPriorityAsync(client, secondHomeId, secondChoice.ApplicationId, templateId));

        var page = await client.GetFromJsonAsync<PaginatedResponse<HomeSelectionListItemResponse>>(
            $"/api/home-selections{new GetHomeSelectionsQuerySpec(1, GetHomeSelectionsQuerySpec.MaxPageSize).ToQueryString()}");

        var ids = page!.Items.Select(s => s.Id).ToList();
        ids.Should().ContainInOrder(later, earlier);
        page.Items.Single(s => s.Id == earlier).Should().BeEquivalentTo(new
        {
            HomeId = firstHomeId,
            HomeLabel = "1 Test Lane",
            SelectedDate,
            SelectionMethod = "Priority",
            SelectedApplicationId = firstChoice.ApplicationId,
        });
        page.Items.Single(s => s.Id == earlier).ChosenContact!.Email.Should().Be(firstChoice.Email);
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

    private static SelectHouseholdByPriority PriorityCommand(string homeId, string applicationId, string templateId) => new(
        HomeSelectionId.Generate(),
        homeId,
        HomeownerApplicationId.Create(applicationId),
        [SelectionPriority.LowerIncome],
        "Lowest income",
        templateId,
        200000m,
        SelectedDate,
        UserId.Generate());

    private static Task<HttpResponseMessage> SelectByPriorityAsync(
        HttpClient client, string homeId, string applicationId, string templateId, DateOnly? selectedDate = null) =>
        client.PostAsJsonAsync($"/api/homes/{homeId}/home-selections/select-by-priority",
            new SelectHouseholdByPriorityRequest(applicationId, ["WaitingListTenure"], "Longest waiting", templateId, 200000m, selectedDate ?? SelectedDate));

    private static async Task<string> ReadCreatedIdAsync(HttpResponseMessage response)
    {
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<HomeSelectionCreatedResponse>())!.SelectionId;
    }

    private static async Task<HomeSelectionOverviewResponse> GetOverviewAsync(HttpClient client, string homeId) =>
        (await client.GetFromJsonAsync<HomeSelectionOverviewResponse>($"/api/homes/{homeId}/selection"))!;

    private async Task<DateOnly> TodayAsync()
    {
        using var scope = factory.Services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<ITrustClock>().TodayAsync(ApiFactory.TestTrustId);
    }

    /// <summary>Records a sale stage on HomeBuyers's view of the home, as PropertyRegistry's news would in a real host.</summary>
    private async Task RecordSaleStageAsync(string homeId, string saleStage, DateOnly? since, int sequence, string? latestSelectionId)
    {
        using var scope = factory.Services.CreateScope();
        await using var session = scope.ServiceProvider.GetRequiredService<IDocumentStore>().LightweightSession(ApiFactory.TestTrustId);
        var facts = await session.LoadAsync<HomeFactsView>(homeId);
        HomeSaleStageChangedHandler.Handle(new HomeSaleStageChanged(homeId, saleStage, since, sequence, latestSelectionId), facts)
            .Execute(session);
        await session.SaveChangesAsync();
    }

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
    private async Task<string> CreateHomeAsync(HttpClient client, string? landParcelId = null, int? incomeLimitPercentOfAmi = 80)
    {
        landParcelId ??= await CreateLandParcelAsync(client);

        var response = await client.PostAsJsonAsync("/api/homes",
            new RegisterHomeRequest(
                landParcelId,
                "SingleFamily",
                3,
                2m,
                false,
                incomeLimitPercentOfAmi,
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
            new HomeRegistered(homeId, landParcelId, Label: "1 Test Lane", Bedrooms: 3, Bathrooms: 2m, SquareFootage: 1200m, IsAccessible: false, incomeLimitPercentOfAmi))
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
                45m,
                15m,
                "Standard covenant terms",
                "AppraisalBased",
                0.25m,
                null,
                null,
                null,
                null,
                90));
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
