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
using CommonGround.Stewardship.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Wolverine.Marten;
using Wolverine.Tracking;

namespace CommonGround.API.IntegrationTests.Flows;

/// <summary>
/// Follows a home from registration to a closed or failed purchase across PropertyRegistry,
/// HomeBuyers and Homeownership, with nothing delivered by hand: each step is an API call, and
/// the integration events it leads to reach every bounded context's handler on its own queue.
/// Each step waits for Marten to forward its events and for every handler they reach to finish.
/// </summary>
[Collection(DurableIntegrationCollection.Name)]
public class HomePurchaseFlowTests(DurableApiFactory factory)
{
    private static readonly DateOnly ApprovedDate = new(2026, 6, 1);
    private static readonly DateOnly SelectedDate = new(2026, 7, 20);
    private static readonly DateOnly ClosingDate = new(2026, 9, 1);

    private static readonly string FailingHandlerQueue = nameof(FailingApplicationSelectedHandler).ToLowerInvariant();

    private readonly List<string> _deadLettered = [];

    [Fact]
    public async Task RegisteringAHome_LetsHomeBuyersSelectForItAndHomeownershipNameIt()
    {
        var client = factory.CreateClient().WithAuthToken();
        var landParcelId = await CreateLandParcelAsync(client);

        var homeId = await TrackAsync(() => RegisterHomeAsync(client, landParcelId));
        await CreateApprovedApplicationAsync(client);

        var selectionId = await StartSelectionAsync(client, homeId);
        (await client.GetFromJsonAsync<HomeSelectionResponse>($"/api/home-selections/{selectionId}"))!
            .Should().BeEquivalentTo(new { HomeId = homeId, HomeBedrooms = 3, HomeIsAccessible = false });
    }

    [Fact]
    public async Task SelectingAHousehold_StartsItsPurchaseAndPutsTheHomeUnderContract()
    {
        var client = factory.CreateClient().WithAuthToken();
        var purchase = await SelectHouseholdAsync(client);

        (await client.GetFromJsonAsync<HomeResponse>($"/api/homes/{purchase.HomeId}"))!
            .Should().BeEquivalentTo(new { Status = "UnderContract", UnderContractDate = SelectedDate });

        var workflow = await client.GetFromJsonAsync<PurchaseWorkflowResponse>($"/api/purchase-workflows/{purchase.WorkflowId}");
        workflow.Should().BeEquivalentTo(new
        {
            purchase.Applicant.ApplicationId,
            purchase.Applicant.HouseholdId,
            purchase.HomeId,
            HomeLabel = "789 Elm St",
        });

        (await client.GetFromJsonAsync<HouseholdResponse>($"/api/households/{purchase.Applicant.HouseholdId}"))!
            .Journey.Stage.Should().Be("Purchasing");
        (await client.GetFromJsonAsync<PaginatedResponse<PurchaseWorkflowListItemResponse>>($"/api/purchase-workflows?householdId={purchase.Applicant.HouseholdId}"))!
            .Items.Should().ContainSingle().Which.Id.Should().Be(purchase.WorkflowId);
        (await client.GetFromJsonAsync<PaginatedResponse<PurchaseWorkflowListItemResponse>>($"/api/purchase-workflows?groundLeaseTemplateId={purchase.TemplateId}"))!
            .Items.Should().ContainSingle().Which.Id.Should().Be(purchase.WorkflowId);
    }

    [Fact]
    public async Task SelectingAHousehold_MovesTheHomeFromSelectionsToPurchasesOnToday()
    {
        var client = factory.CreateClient().WithAuthToken();
        var landParcelId = await CreateLandParcelAsync(client);
        var homeId = await TrackAsync(() => RegisterHomeAsync(client, landParcelId));
        var templateId = await CreateActiveGroundLeaseTemplateAsync(client);
        var applicant = await CreateApprovedApplicationAsync(client);

        (await GetTodayAsync(client)).Items.Should().ContainSingle(i => i.RecordId == homeId)
            .Which.Should().BeEquivalentTo(new { Area = "selections", Kind = "home-available", Label = "789 Elm St" });

        var selectionId = await TrackAsync(() => StartSelectionAsync(client, homeId));
        var selecting = await GetTodayAsync(client);
        selecting.Items.Should().NotContain(i => i.RecordId == homeId);
        selecting.Items.Should().ContainSingle(i => i.RecordId == selectionId).Which.Kind.Should().Be("selection-open");

        await TrackAsync(async () =>
            (await client.PostAsJsonAsync($"/api/home-selections/{selectionId}/select-by-priority",
                new SelectHouseholdByPriorityRequest(
                    applicant.ApplicationId, ["WaitingListTenure"], "Longest on the waiting list", templateId, 200000m, SelectedDate)))
                .StatusCode.Should().Be(HttpStatusCode.NoContent));
        var workflowId = (await client.GetFromJsonAsync<PaginatedResponse<PurchaseWorkflowListItemResponse>>(
            $"/api/purchase-workflows?homeId={homeId}"))!.Items.Single().Id;

        var purchasing = await GetTodayAsync(client);
        purchasing.Items.Should().NotContain(i => i.RecordId == selectionId || i.RecordId == homeId);
        purchasing.Items.Should().ContainSingle(i => i.RecordId == workflowId).Which.Should().BeEquivalentTo(new
        {
            Area = "purchases",
            Kind = "purchase-step-waiting",
            Label = "789 Elm St",
        });
    }

    [Fact]
    public async Task SelectingAHousehold_WhenAnotherContextFailsToReact_StillStartsThePurchaseAndPutsTheHomeUnderContract()
    {
        var client = factory.CreateClient().WithAuthToken();
        var purchase = await SelectHouseholdAsync(client, failingTheSelection: true);
        _deadLettered.Should().ContainSingle("the failing reaction ran, and only once, with no retries");

        (await client.GetFromJsonAsync<HomeResponse>($"/api/homes/{purchase.HomeId}"))!
            .Status.Should().Be("UnderContract");
        (await client.GetFromJsonAsync<PurchaseWorkflowResponse>($"/api/purchase-workflows/{purchase.WorkflowId}"))!
            .ApplicationId.Should().Be(purchase.Applicant.ApplicationId);
    }

    [Fact]
    public async Task FailingThePurchase_ReturnsTheHouseholdToTheWaitingListAndTheHomeToSelection()
    {
        var client = factory.CreateClient().WithAuthToken();
        var purchase = await SelectHouseholdAsync(client);
        await CreateApprovedApplicationAsync(client);
        var failedDate = SelectedDate.AddDays(20);

        await TrackAsync(async () =>
            (await client.PostAsJsonAsync($"/api/purchase-workflows/{purchase.WorkflowId}/fail",
                new FailPurchaseRequest("Financing fell through", failedDate)))
                .StatusCode.Should().Be(HttpStatusCode.NoContent));

        (await client.GetFromJsonAsync<HomeownerApplicationResponse>($"/api/homeowner-applications/{purchase.Applicant.ApplicationId}"))!
            .Should().BeEquivalentTo(new { Phase = "Approved", ApprovedDate = ApprovedDate });
        (await client.GetFromJsonAsync<HouseholdResponse>($"/api/households/{purchase.Applicant.HouseholdId}"))!
            .Journey.Should().BeEquivalentTo(new { Stage = "Waiting", ApprovedDate = ApprovedDate, SelectedDate = (DateOnly?)null });

        var selections = await client.GetFromJsonAsync<List<HomeSelectionSummaryResponse>>($"/api/homes/{purchase.HomeId}/home-selections");
        selections!.Select(s => s.Status).Should().Equal("Open", "FellThrough");
        (await client.GetFromJsonAsync<HomeSelectionResponse>($"/api/home-selections/{selections![0].Id}"))!
            .Candidates.Should().NotContain(c => c.ApplicationId == purchase.Applicant.ApplicationId,
                "the household isn't a candidate for the home its purchase just fell through on");
        (await client.GetFromJsonAsync<HomeResponse>($"/api/homes/{purchase.HomeId}"))!
            .Should().BeEquivalentTo(new { Status = "InSelection", UnderContractDate = (DateOnly?)null, AvailableAgainDate = failedDate });
    }

    [Fact]
    public async Task ClosingThePurchase_LeasesTheHomeToTheHousehold()
    {
        var client = factory.CreateClient().WithAuthToken();
        var purchase = await SelectHouseholdAsync(client);
        await SatisfyAllTracksAsync(client, purchase.WorkflowId);
        (await client.PostAsJsonAsync($"/api/purchase-workflows/{purchase.WorkflowId}/closing/schedule",
            new ScheduleClosingRequest(ClosingDate)))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        await TrackAsync(async () =>
            (await client.PostAsJsonAsync($"/api/purchase-workflows/{purchase.WorkflowId}/closing/record",
                new RecordPurchaseClosedRequest(ClosingDate, null)))
                .StatusCode.Should().Be(HttpStatusCode.NoContent));

        var leases = await client.GetFromJsonAsync<PaginatedResponse<GroundLeaseListItemResponse>>(
            $"/api/ground-leases?landParcelId={purchase.LandParcelId}");
        leases!.Items.Should().ContainSingle();
        (await client.GetFromJsonAsync<HomeResponse>($"/api/homes/{purchase.HomeId}"))!
            .Should().BeEquivalentTo(new { Status = "Leased", LeasedDate = ClosingDate });
        (await client.GetFromJsonAsync<HouseholdResponse>($"/api/households/{purchase.Applicant.HouseholdId}"))!
            .Journey.Should().BeEquivalentTo(new { Stage = "Homeowner", PurchasedDate = ClosingDate, SelectedHomeId = purchase.HomeId });
    }

    [Fact]
    public async Task CreatingALeaseDirectly_OpensAStewardshipCaseAndLeasesTheHome()
    {
        var client = factory.CreateClient().WithAuthToken();
        var landParcelId = await CreateLandParcelAsync(client);
        var homeId = await TrackAsync(() => RegisterHomeAsync(client, landParcelId));
        var leaseStart = new DateOnly(2019, 3, 1);

        var leaseId = await TrackAsync(async () =>
        {
            var response = await CreateLeaseAsync(client, landParcelId, homeId, leaseStart);
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            return (await response.Content.ReadFromJsonAsync<CreateGroundLeaseResponse>())!.GroundLeaseId;
        });

        var today = await GetTodayAsync(client);
        var cases = await client.GetFromJsonAsync<PaginatedResponse<StewardshipCaseView>>($"/api/stewardship/cases?groundLeaseId={leaseId}");
        cases!.Items.Should().ContainSingle().Which.Should().BeEquivalentTo(new { Status = "Active", OpenedDate = today.Today },
            "the case opens on the day the lease was recorded, not on its start date");
        (await client.GetFromJsonAsync<HomeResponse>($"/api/homes/{homeId}"))!
            .Should().BeEquivalentTo(new { Status = "Leased", LeasedDate = leaseStart });
        today.Items.Should().NotContain(i => i.RecordId == homeId, "a leased home isn't waiting for a selection");
    }

    [Fact]
    public async Task CreatingALeaseDirectly_OnAHomeInSelection_IsRefused()
    {
        var client = factory.CreateClient().WithAuthToken();
        var landParcelId = await CreateLandParcelAsync(client);
        var homeId = await TrackAsync(() => RegisterHomeAsync(client, landParcelId));
        await CreateApprovedApplicationAsync(client);
        await TrackAsync(() => StartSelectionAsync(client, homeId));

        var response = await CreateLeaseAsync(client, landParcelId, homeId, new DateOnly(2019, 3, 1));

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await response.Content.ReadFromJsonAsync<ProblemDetails>())!.Detail
            .Should().Be("This home is in a home selection. Cancel the selection before leasing it by hand.");
    }

    [Fact]
    public async Task CreatingALeaseDirectly_OnAHomeUnderContract_IsRefused()
    {
        var client = factory.CreateClient().WithAuthToken();
        var purchase = await SelectHouseholdAsync(client);

        var response = await CreateLeaseAsync(client, purchase.LandParcelId, purchase.HomeId, new DateOnly(2019, 3, 1));

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await response.Content.ReadFromJsonAsync<ProblemDetails>())!.Detail
            .Should().Be("This home has a purchase in progress. Lease it by closing that purchase, or end the purchase first.");
    }

    [Fact]
    public async Task CreatingALeaseDirectly_AfterTheHomesSelectionIsCancelled_IsAllowed()
    {
        var client = factory.CreateClient().WithAuthToken();
        var landParcelId = await CreateLandParcelAsync(client);
        var homeId = await TrackAsync(() => RegisterHomeAsync(client, landParcelId));
        await CreateApprovedApplicationAsync(client);
        var selectionId = await TrackAsync(() => StartSelectionAsync(client, homeId));
        await TrackAsync(async () =>
            (await client.PostAsJsonAsync($"/api/home-selections/{selectionId}/cancel", new CancelHomeSelectionRequest("Recording an existing lease")))
                .StatusCode.Should().Be(HttpStatusCode.NoContent));

        var response = await CreateLeaseAsync(client, landParcelId, homeId, new DateOnly(2019, 3, 1));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task RecordingAnIntentToSell_PutsTheHomeInResaleUntilItIsWithdrawn()
    {
        var client = factory.CreateClient().WithAuthToken();
        var landParcelId = await CreateLandParcelAsync(client);
        var homeId = await TrackAsync(() => RegisterHomeAsync(client, landParcelId));
        var leaseId = await TrackAsync(async () =>
            (await (await CreateLeaseAsync(client, landParcelId, homeId, new DateOnly(2019, 3, 1)))
                .Content.ReadFromJsonAsync<CreateGroundLeaseResponse>())!.GroundLeaseId);
        var today = (await GetTodayAsync(client)).Today;

        var resaleId = await TrackAsync(async () =>
        {
            var response = await client.PostAsJsonAsync($"/api/ground-leases/{leaseId}/intent-to-sell",
                new RecordIntentToSellRequest(today, "Phone", "Moving for work"));
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            return (await response.Content.ReadFromJsonAsync<RecordIntentToSellResponse>())!.ResaleId;
        });

        (await client.GetFromJsonAsync<HomeResponse>($"/api/homes/{homeId}"))!
            .Should().BeEquivalentTo(new { Status = "Resale", ResaleNoticeDate = today });
        (await client.GetFromJsonAsync<PaginatedResponse<ResaleResponse>>($"/api/resales?status=Open&groundLeaseId={leaseId}"))!
            .Items.Should().ContainSingle().Which.Should().BeEquivalentTo(new
            {
                Id = resaleId,
                HomeId = homeId,
                HomeownerFullName = "Dana Whitfield",
                OptionWindowEndsOn = today.AddDays(60),
            });

        var second = await client.PostAsJsonAsync($"/api/ground-leases/{leaseId}/intent-to-sell",
            new RecordIntentToSellRequest(today, "Email", null));
        second.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await second.Content.ReadFromJsonAsync<ProblemDetails>())!.Detail
            .Should().Be("This lease already has a resale in progress.");

        await TrackAsync(async () =>
            (await client.PostAsJsonAsync($"/api/resales/{resaleId}/withdraw", new WithdrawIntentToSellRequest(today, "Staying put")))
                .StatusCode.Should().Be(HttpStatusCode.NoContent));

        (await client.GetFromJsonAsync<HomeResponse>($"/api/homes/{homeId}"))!
            .Should().BeEquivalentTo(new { Status = "Leased", ResaleNoticeDate = (DateOnly?)null });
        (await client.GetFromJsonAsync<PaginatedResponse<ResaleResponse>>($"/api/resales?status=Open&groundLeaseId={leaseId}"))!
            .Items.Should().BeEmpty();
        (await client.PostAsJsonAsync($"/api/ground-leases/{leaseId}/intent-to-sell", new RecordIntentToSellRequest(today, "Letter", null)))
            .StatusCode.Should().Be(HttpStatusCode.Created, "a withdrawn resale frees the lease for another notice");
    }

    /// <summary>
    /// Runs <paramref name="action"/>, then waits for the events it leads to to reach every handler. A
    /// catch-up forwards only the events stored so far, and handlers append more (a fallen-through
    /// purchase starts a new selection), so it catches up again until a round forwards nothing. Fails
    /// if a handler dead-letters a message, other than <see cref="FailingApplicationSelectedHandler"/>,
    /// whose failures are kept in <see cref="_deadLettered"/>.
    /// </summary>
    private async Task TrackAsync(Func<Task> action)
    {
        var session = await TrackRoundAsync(action);
        for (var round = 0; session.AllRecordsInOrder().Any(r => r.MessageEventType == MessageEventType.Sent); round++)
        {
            round.Should().BeLessThan(10, "the events a step leads to should settle");
            session = await TrackRoundAsync(() => Task.CompletedTask);
        }
    }

    private async Task<ITrackedSession> TrackRoundAsync(Func<Task> action)
    {
        var session = await factory.Services.TrackActivity(TimeSpan.FromSeconds(30))
            .PauseThenCatchUpOnMartenDaemonActivity()
            .ExecuteAndWaitAsync(_ => action());

        var deadLettered = session.AllRecordsInOrder()
            .Where(r => r.MessageEventType == MessageEventType.MovedToErrorQueue)
            .Select(r => r.Envelope!.Destination!.ToString())
            .ToList();
        var failedOnPurpose = deadLettered.Where(d => d.Contains(FailingHandlerQueue, StringComparison.Ordinal)).ToList();
        deadLettered.Except(failedOnPurpose).Should().BeEmpty("no handler should fail");
        _deadLettered.AddRange(failedOnPurpose);

        return session;
    }

    private async Task<T> TrackAsync<T>(Func<Task<T>> action)
    {
        T result = default!;
        await TrackAsync(async () => { result = await action(); });
        return result;
    }

    private sealed record Applicant(string ApplicationId, string HouseholdId);

    private sealed record Purchase(string LandParcelId, string HomeId, string TemplateId, Applicant Applicant, string WorkflowId);

    /// <summary>
    /// Registers a home, selects a ready household for it, and returns the purchase that starts.
    /// <paramref name="failingTheSelection"/> makes <see cref="FailingApplicationSelectedHandler"/> fail on it.
    /// </summary>
    private async Task<Purchase> SelectHouseholdAsync(HttpClient client, bool failingTheSelection = false)
    {
        var landParcelId = await CreateLandParcelAsync(client);
        var homeId = await TrackAsync(() => RegisterHomeAsync(client, landParcelId));
        if (failingTheSelection)
            FailingApplicationSelectedHandler.FailForHomes[homeId] = true;
        var templateId = await CreateActiveGroundLeaseTemplateAsync(client);
        var applicant = await CreateApprovedApplicationAsync(client);
        var selectionId = await StartSelectionAsync(client, homeId);

        await TrackAsync(async () =>
            (await client.PostAsJsonAsync($"/api/home-selections/{selectionId}/select-by-priority",
                new SelectHouseholdByPriorityRequest(
                    applicant.ApplicationId, ["WaitingListTenure"], "Longest on the waiting list", templateId, 200000m, SelectedDate)))
                .StatusCode.Should().Be(HttpStatusCode.NoContent));

        var workflows = await client.GetFromJsonAsync<PaginatedResponse<PurchaseWorkflowListItemResponse>>(
            $"/api/purchase-workflows?homeId={homeId}");
        var workflowId = workflows!.Items.Should().ContainSingle("selecting a household starts its purchase").Subject.Id;

        return new Purchase(landParcelId, homeId, templateId, applicant, workflowId);
    }

    private static Task<HttpResponseMessage> CreateLeaseAsync(HttpClient client, string landParcelId, string homeId, DateOnly leaseStart) =>
        client.PostAsJsonAsync("/api/ground-leases",
            new CreateGroundLeaseRequest(
                landParcelId, homeId, "Dana Whitfield", "dana@example.com", null, leaseStart, 99, 60, "Signed on paper",
                "AppraisalBased", 0.25m, null, null, null, null));

    private static async Task<TodayResponse> GetTodayAsync(HttpClient client) =>
        (await client.GetFromJsonAsync<TodayResponse>("/api/today"))!;

    private static async Task<string> CreateLandParcelAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/land-parcels",
            new RegisterLandParcelRequest($"APN-{Guid.NewGuid():N}", "789 Elm St", 0.25m, new DateOnly(2024, 1, 1), "Purchase", null));
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<RegisterLandParcelResponse>())!.ParcelId;
    }

    private static async Task<string> RegisterHomeAsync(HttpClient client, string landParcelId)
    {
        var response = await client.PostAsJsonAsync("/api/homes",
            new RegisterHomeRequest(
                landParcelId, "SingleFamily", 3, 2m, false, 1200m, 1990, "Test Appraiser", new DateOnly(2024, 1, 1), 280000m));
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<RegisterHomeResponse>())!.HomeId;
    }

    private static async Task<string> CreateActiveGroundLeaseTemplateAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/ground-lease-templates",
            new CreateGroundLeaseTemplateRequest(
                $"Standard 99-Year {Guid.NewGuid():N}", 99, 60, "Standard covenant terms", "AppraisalBased", 0.25m, null, null, null, null));
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<CreateGroundLeaseTemplateResponse>())!.GroundLeaseTemplateId;
    }

    private static async Task<string> StartSelectionAsync(HttpClient client, string homeId)
    {
        var response = await client.PostAsync($"/api/homes/{homeId}/home-selections", null);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<StartHomeSelectionResponse>())!.SelectionId;
    }

    /// <summary>Registers a one-member household ready to buy, and submits and approves its application.</summary>
    private static async Task<Applicant> CreateApprovedApplicationAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/households",
            new RegisterHouseholdRequest(
                [new HouseholdMemberRequest("Flow Applicant", $"flow-{Guid.NewGuid():N}@example.com", "(202) 555-0143", true)],
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

        var submitted = await client.PostAsJsonAsync("/api/homeowner-applications", new SubmitApplicationRequest(householdId));
        submitted.StatusCode.Should().Be(HttpStatusCode.Created);
        var applicationId = (await submitted.Content.ReadFromJsonAsync<SubmitApplicationResponse>())!.ApplicationId;

        (await client.PostAsJsonAsync($"/api/homeowner-applications/{applicationId}/approve",
            new ApproveApplicationRequest(ApprovedDate, null)))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        return new Applicant(applicationId, householdId);
    }

    private static async Task SatisfyAllTracksAsync(HttpClient client, string workflowId)
    {
        var responses = new List<HttpResponseMessage>
        {
            await client.PostAsJsonAsync($"/api/purchase-workflows/{workflowId}/eligibility",
                new RecordEligibilityVerifiedRequest(new DateOnly(2026, 8, 1), null)),
            await client.PostAsJsonAsync($"/api/purchase-workflows/{workflowId}/financing",
                new RecordFinancingCommittedRequest("Test Lender", 140000m, new DateOnly(2026, 8, 2))),
            await client.PostAsJsonAsync($"/api/purchase-workflows/{workflowId}/inspection",
                new RecordInspectionCompletedRequest(new DateOnly(2026, 8, 3), "Jane Inspector", null)),
            await client.PostAsJsonAsync($"/api/purchase-workflows/{workflowId}/title",
                new RecordTitleClearedRequest(new DateOnly(2026, 8, 4), "Test Title Co")),
            await client.PostAsJsonAsync($"/api/purchase-workflows/{workflowId}/lease-execution",
                new RecordLeaseExecutedRequest(new DateOnly(2026, 8, 5))),
        };

        responses.Should().AllSatisfy(r => r.StatusCode.Should().Be(HttpStatusCode.NoContent));
    }
}
