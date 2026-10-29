using System.Net;
using System.Net.Http.Json;
using CommonGround.API.Contracts.HomeBuyers.Requests;
using CommonGround.API.Contracts.HomeBuyers.Responses;
using CommonGround.HomeBuyers.Application.Commands;
using CommonGround.HomeBuyers.Domain.Households;
using CommonGround.SharedKernel.Infrastructure.API;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;

namespace CommonGround.API.IntegrationTests.HomeBuyers;

/// <summary>
/// Covers the waiting list query: which applications are on it, their order, and the readiness
/// and re-verification state derived from the household as of today.
/// </summary>
[Collection(IntegrationCollection.Name)]
public class WaitingListTests(ApiFactory factory)
{
    [Fact]
    public async Task GetWaitingList_ListsApprovedApplicationsLongestWaitingFirst()
    {
        var client = factory.CreateClient().WithAuthToken();

        var laterId = await SubmitAndApproveAsync(client, await RegisterHouseholdAsync(client, "waiting-later@example.com"), "2026-05-01");
        var earlierId = await SubmitAndApproveAsync(client, await RegisterHouseholdAsync(client, "waiting-earlier@example.com"), "2026-03-01");
        var submittedId = await SubmitAsync(client, await RegisterHouseholdAsync(client, "waiting-submitted@example.com"));
        var withdrawnId = await SubmitAndApproveAsync(client, await RegisterHouseholdAsync(client, "waiting-withdrawn@example.com"), "2026-03-01");
        (await client.PostAsJsonAsync($"/api/homeowner-applications/{withdrawnId}/withdraw",
            new WithdrawApplicationRequest("2026-06-05", null)))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        var waitingList = await GetWholeWaitingListAsync(client);

        var ids = waitingList.Select(e => e.ApplicationId).ToList();
        ids.Should().NotContain([submittedId, withdrawnId]);
        ids.Should().ContainInOrder(earlierId, laterId);

        var earlier = waitingList.Single(e => e.ApplicationId == earlierId);
        earlier.ApprovedDate.Should().Be("2026-03-01");
        earlier.PrimaryContact!.Email.Should().Be("waiting-earlier@example.com");
    }

    [Fact]
    public async Task GetWaitingList_DerivesReadinessAndReverificationFromTheHousehold()
    {
        var client = factory.CreateClient().WithAuthToken();

        var readyHouseholdId = await RegisterHouseholdAsync(client, "waiting-ready@example.com");
        await RecordReadinessAsync(client, readyHouseholdId, "2099-01-01");
        var readyId = await SubmitAndApproveAsync(client, readyHouseholdId, "2026-06-01");

        var neverPrequalifiedId = await SubmitAndApproveAsync(
            client, await RegisterHouseholdAsync(client, "waiting-never-prequalified@example.com"), "2026-06-01");

        var pastExpiryHouseholdId = await RegisterHouseholdAsync(client, "waiting-past-expiry@example.com");
        await RecordReadinessAsync(client, pastExpiryHouseholdId, "2026-01-01");
        var pastExpiryId = await SubmitAndApproveAsync(client, pastExpiryHouseholdId, "2026-06-01");

        var lapsedHouseholdId = await RegisterHouseholdAsync(client, "waiting-lapsed@example.com");
        await RecordReadinessAsync(client, lapsedHouseholdId, "2026-02-01");
        await ExpirePrequalificationAsync(lapsedHouseholdId, new DateOnly(2026, 2, 1));
        var lapsedId = await SubmitAndApproveAsync(client, lapsedHouseholdId, "2026-06-01");

        var waitingList = (await GetWholeWaitingListAsync(client)).ToDictionary(e => e.ApplicationId);

        waitingList[readyId].Should().BeEquivalentTo(new
        {
            IsHousingCounselingCompleted = true,
            IsCltOrientationCompleted = true,
            MortgagePrequalificationExpiryDate = "2099-01-01",
            IsReadyForSelection = true,
            NeedsReverification = false,
        });
        waitingList[neverPrequalifiedId].Should().BeEquivalentTo(new
        {
            MortgagePrequalificationExpiryDate = (string?)null,
            IsReadyForSelection = false,
            NeedsReverification = false,
        });
        waitingList[pastExpiryId].Should().BeEquivalentTo(new
        {
            MortgagePrequalificationExpiryDate = "2026-01-01",
            IsReadyForSelection = false,
            NeedsReverification = true,
        });
        waitingList[lapsedId].Should().BeEquivalentTo(new
        {
            MortgagePrequalificationExpiryDate = (string?)null,
            IsReadyForSelection = false,
            NeedsReverification = true,
        });
    }

    [Fact]
    public async Task GetWaitingList_WithSearch_MatchesAnyHouseholdMember()
    {
        var client = factory.CreateClient().WithAuthToken();
        var marker = Guid.NewGuid().ToString("N")[..8];

        var householdResponse = await client.PostAsJsonAsync("/api/households",
            new RegisterHouseholdRequest(
                [
                    new HouseholdMemberRequest("Searched Primary", $"searched-primary-{marker}@example.com", "555-0100", true),
                    new HouseholdMemberRequest($"Second {marker}", "searched-second@example.com", "555-0101", false),
                ],
                false,
                65000m));
        var householdId = (await householdResponse.Content.ReadFromJsonAsync<RegisterHouseholdResponse>())!.HouseholdId;
        var matchingId = await SubmitAndApproveAsync(client, householdId, "2026-06-01");
        await SubmitAndApproveAsync(client, await RegisterHouseholdAsync(client, "waiting-unsearched@example.com"), "2026-06-01");

        var page = await client.GetFromJsonAsync<PaginatedResponse<WaitingListEntryResponse>>(
            $"/api/waiting-list{new GetWaitingListQuerySpec(search: $"SECOND {marker}").ToQueryString()}");

        page!.Items.Select(e => e.ApplicationId).Should().Equal(matchingId);
    }

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

    private async Task ExpirePrequalificationAsync(string householdId, DateOnly originalExpiryDate)
    {
        using var scope = factory.Services.CreateScope();
        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

        await bus.InvokeForTenantAsync(
            ApiFactory.TestTrustId,
            new ExpireMortgagePrequalification(HouseholdId.Create(householdId), originalExpiryDate));
    }

    private static async Task<string> RegisterHouseholdAsync(HttpClient client, string email)
    {
        var response = await client.PostAsJsonAsync("/api/households",
            new RegisterHouseholdRequest(
                [new HouseholdMemberRequest("Waiting Applicant", email, "555-0100", true)],
                false,
                65000m));
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<RegisterHouseholdResponse>())!.HouseholdId;
    }

    private static async Task RecordReadinessAsync(HttpClient client, string householdId, string prequalificationExpiryDate)
    {
        (await client.PostAsJsonAsync($"/api/households/{householdId}/housing-counseling",
            new RecordHousingCounselingRequest("CERT-001", "2026-01-02", "Test Counseling Org")))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await client.PostAsJsonAsync($"/api/households/{householdId}/clt-orientation",
            new RecordCltOrientationRequest("2026-01-03")))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await client.PostAsJsonAsync($"/api/households/{householdId}/mortgage-prequalification",
            new RecordMortgagePrequalificationRequest("Test Lender", 210000m, prequalificationExpiryDate)))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    private static async Task<string> SubmitAsync(HttpClient client, string householdId)
    {
        var response = await client.PostAsJsonAsync("/api/homeowner-applications", new SubmitApplicationRequest(householdId));
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<SubmitApplicationResponse>())!.ApplicationId;
    }

    private static async Task<string> SubmitAndApproveAsync(HttpClient client, string householdId, string approvedDate)
    {
        var applicationId = await SubmitAsync(client, householdId);
        (await client.PostAsJsonAsync($"/api/homeowner-applications/{applicationId}/approve",
            new ApproveApplicationRequest(approvedDate, null)))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);
        return applicationId;
    }
}
