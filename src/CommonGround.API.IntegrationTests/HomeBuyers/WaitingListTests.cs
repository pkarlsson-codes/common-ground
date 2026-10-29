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

        var laterId = await SubmitAndApproveAsync(client, await RegisterHouseholdAsync(client, "waiting-later@example.com"), new DateOnly(2026, 5, 1));
        var earlierId = await SubmitAndApproveAsync(client, await RegisterHouseholdAsync(client, "waiting-earlier@example.com"), new DateOnly(2026, 3, 1));
        var submittedId = await SubmitAsync(client, await RegisterHouseholdAsync(client, "waiting-submitted@example.com"));
        var withdrawnId = await SubmitAndApproveAsync(client, await RegisterHouseholdAsync(client, "waiting-withdrawn@example.com"), new DateOnly(2026, 3, 1));
        (await client.PostAsJsonAsync($"/api/homeowner-applications/{withdrawnId}/withdraw",
            new WithdrawApplicationRequest(new DateOnly(2026, 6, 5), null)))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        var waitingList = await GetWholeWaitingListAsync(client);

        var ids = waitingList.Select(e => e.ApplicationId).ToList();
        ids.Should().NotContain([submittedId, withdrawnId]);
        ids.Should().ContainInOrder(earlierId, laterId);

        var earlier = waitingList.Single(e => e.ApplicationId == earlierId);
        earlier.ApprovedDate.Should().Be(new DateOnly(2026, 3, 1));
        earlier.PrimaryContact!.Email.Should().Be("waiting-earlier@example.com");
    }

    [Fact]
    public async Task GetWaitingList_DerivesReadinessAndReverificationFromTheHousehold()
    {
        var client = factory.CreateClient().WithAuthToken();

        var readyHouseholdId = await RegisterHouseholdAsync(client, "waiting-ready@example.com");
        await RecordReadinessAsync(client, readyHouseholdId, new DateOnly(2099, 1, 1));
        var readyId = await SubmitAndApproveAsync(client, readyHouseholdId, new DateOnly(2026, 6, 1));

        var neverPrequalifiedId = await SubmitAndApproveAsync(
            client, await RegisterHouseholdAsync(client, "waiting-never-prequalified@example.com"), new DateOnly(2026, 6, 1));

        var pastExpiryHouseholdId = await RegisterHouseholdAsync(client, "waiting-past-expiry@example.com");
        await RecordReadinessAsync(client, pastExpiryHouseholdId, new DateOnly(2026, 1, 1));
        var pastExpiryId = await SubmitAndApproveAsync(client, pastExpiryHouseholdId, new DateOnly(2026, 6, 1));

        var lapsedHouseholdId = await RegisterHouseholdAsync(client, "waiting-lapsed@example.com");
        await RecordReadinessAsync(client, lapsedHouseholdId, new DateOnly(2026, 2, 1));
        await ExpirePrequalificationAsync(lapsedHouseholdId, new DateOnly(2026, 2, 1));
        var lapsedId = await SubmitAndApproveAsync(client, lapsedHouseholdId, new DateOnly(2026, 6, 1));

        var waitingList = (await GetWholeWaitingListAsync(client)).ToDictionary(e => e.ApplicationId);

        waitingList[readyId].Should().BeEquivalentTo(new
        {
            IsHousingCounselingCompleted = true,
            IsCltOrientationCompleted = true,
            MortgagePrequalificationExpiryDate = new DateOnly(2099, 1, 1),
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
            MortgagePrequalificationExpiryDate = new DateOnly(2026, 1, 1),
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
        var matchingId = await SubmitAndApproveAsync(client, householdId, new DateOnly(2026, 6, 1));
        await SubmitAndApproveAsync(client, await RegisterHouseholdAsync(client, "waiting-unsearched@example.com"), new DateOnly(2026, 6, 1));

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

    private static async Task RecordReadinessAsync(HttpClient client, string householdId, DateOnly prequalificationExpiryDate)
    {
        (await client.PostAsJsonAsync($"/api/households/{householdId}/housing-counseling",
            new RecordHousingCounselingRequest("CERT-001", new DateOnly(2026, 1, 2), "Test Counseling Org")))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await client.PostAsJsonAsync($"/api/households/{householdId}/clt-orientation",
            new RecordCltOrientationRequest(new DateOnly(2026, 1, 3))))
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

    private static async Task<string> SubmitAndApproveAsync(HttpClient client, string householdId, DateOnly approvedDate)
    {
        var applicationId = await SubmitAsync(client, householdId);
        (await client.PostAsJsonAsync($"/api/homeowner-applications/{applicationId}/approve",
            new ApproveApplicationRequest(approvedDate, null)))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);
        return applicationId;
    }
}
