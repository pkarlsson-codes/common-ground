using System.Net;
using System.Net.Http.Json;
using CommonGround.API.Contracts.Stewardship.Requests;
using CommonGround.Stewardship.Domain.StewardshipCases;
using CommonGround.Stewardship.Domain.StewardshipCases.Events;
using CommonGround.Stewardship.Infrastructure.Persistence;
using FluentAssertions;
using Marten;
using Microsoft.Extensions.DependencyInjection;

namespace CommonGround.API.IntegrationTests.Stewardship;

[Collection(IntegrationCollection.Name)]
public class StewardshipCaseTests(ApiFactory factory)
{
    [Fact]
    public async Task GetCase_Unauthenticated_Returns401()
    {
        var client = factory.CreateClient();
        var response = await client.GetAsync("/api/stewardship/cases/stewardshipcase-00000000000000000000000001");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCase_AfterSeedingEvent_ReturnsView()
    {
        var (client, caseId) = await SeedStewardshipCaseAsync();

        var response = await client.GetAsync($"/api/stewardship/cases/{caseId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var view = await response.Content.ReadFromJsonAsync<StewardshipCaseView>();
        view!.Id.Should().Be(caseId);
        view.Status.Should().Be(nameof(StewardshipCaseStatus.Active));
    }

    [Fact]
    public async Task InitiateOccupancyVerification_ValidRequest_Returns204()
    {
        var (client, caseId) = await SeedStewardshipCaseAsync();

        var response = await client.PostAsJsonAsync(
            $"/api/stewardship/cases/{caseId}/occupancy-verifications",
            new InitiateOccupancyVerificationRequest(2026, null));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task InitiateAndSubmitOccupancyVerification_Returns204()
    {
        var (client, caseId) = await SeedStewardshipCaseAsync();

        await client.PostAsJsonAsync(
            $"/api/stewardship/cases/{caseId}/occupancy-verifications",
            new InitiateOccupancyVerificationRequest(2026, null));

        var caseView = await (await client.GetAsync($"/api/stewardship/cases/{caseId}"))
            .Content.ReadFromJsonAsync<StewardshipCaseView>();
        var verificationId = caseView!.OccupancyVerifications[0].Id;

        var response = await client.PostAsJsonAsync(
            $"/api/stewardship/cases/{caseId}/occupancy-verifications/{verificationId}/submit",
            new SubmitOccupancyVerificationRequest(true, false, null, new DateOnly(2026, 3, 1)));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ScheduleInspection_ValidRequest_Returns204()
    {
        var (client, caseId) = await SeedStewardshipCaseAsync();

        var response = await client.PostAsJsonAsync(
            $"/api/stewardship/cases/{caseId}/inspections",
            new SchedulePropertyInspectionRequest(new DateOnly(2026, 5, 10), "Jane Inspector"));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task CompleteInspection_AfterScheduling_Returns204()
    {
        var (client, caseId) = await SeedStewardshipCaseAsync();

        await client.PostAsJsonAsync(
            $"/api/stewardship/cases/{caseId}/inspections",
            new SchedulePropertyInspectionRequest(new DateOnly(2026, 5, 10), "Jane Inspector"));

        var caseView = await (await client.GetAsync($"/api/stewardship/cases/{caseId}"))
            .Content.ReadFromJsonAsync<StewardshipCaseView>();
        var inspectionId = caseView!.PropertyInspections[0].Id;

        var response = await client.PostAsJsonAsync(
            $"/api/stewardship/cases/{caseId}/inspections/{inspectionId}/complete",
            new CompletePropertyInspectionRequest(true, null, new DateOnly(2026, 5, 10)));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task CancelInspection_AfterScheduling_Returns204()
    {
        var (client, caseId) = await SeedStewardshipCaseAsync();

        await client.PostAsJsonAsync(
            $"/api/stewardship/cases/{caseId}/inspections",
            new SchedulePropertyInspectionRequest(new DateOnly(2026, 5, 10), "Jane Inspector"));

        var caseView = await (await client.GetAsync($"/api/stewardship/cases/{caseId}"))
            .Content.ReadFromJsonAsync<StewardshipCaseView>();
        var inspectionId = caseView!.PropertyInspections[0].Id;

        var response = await client.PostAsJsonAsync(
            $"/api/stewardship/cases/{caseId}/inspections/{inspectionId}/cancel",
            new CancelPropertyInspectionRequest("Access denied"));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task IdentifyViolation_ValidRequest_Returns204()
    {
        var (client, caseId) = await SeedStewardshipCaseAsync();

        var response = await client.PostAsJsonAsync(
            $"/api/stewardship/cases/{caseId}/violations",
            new IdentifyViolationRequest("Occupancy", "Suspected sublet without disclosure", new DateOnly(2026, 3, 15)));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task IssueViolationNotice_AfterIdentifying_Returns204()
    {
        var (client, caseId) = await SeedStewardshipCaseAsync();

        await client.PostAsJsonAsync(
            $"/api/stewardship/cases/{caseId}/violations",
            new IdentifyViolationRequest("Occupancy", "Suspected sublet", new DateOnly(2026, 3, 15)));

        var caseView = await (await client.GetAsync($"/api/stewardship/cases/{caseId}"))
            .Content.ReadFromJsonAsync<StewardshipCaseView>();
        var violationId = caseView!.LeaseViolations[0].Id;

        var response = await client.PostAsJsonAsync(
            $"/api/stewardship/cases/{caseId}/violations/{violationId}/notice",
            new IssueViolationNoticeRequest(new DateOnly(2026, 3, 20), new DateOnly(2026, 4, 20)));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task CloseCase_ValidRequest_Returns204()
    {
        var (client, caseId) = await SeedStewardshipCaseAsync();

        var response = await client.PostAsJsonAsync(
            $"/api/stewardship/cases/{caseId}/close",
            new CloseStewardshipCaseRequest("Lease terminated", new DateOnly(2026, 4, 1)));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task InitiateOccupancyVerification_OnClosedCase_Returns422()
    {
        var (client, caseId) = await SeedStewardshipCaseAsync();

        await client.PostAsJsonAsync(
            $"/api/stewardship/cases/{caseId}/close",
            new CloseStewardshipCaseRequest("Lease terminated", new DateOnly(2026, 4, 1)));

        var response = await client.PostAsJsonAsync(
            $"/api/stewardship/cases/{caseId}/occupancy-verifications",
            new InitiateOccupancyVerificationRequest(2026, null));

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    private async Task<(HttpClient client, string caseId)> SeedStewardshipCaseAsync()
    {
        var client = factory.CreateClient().WithAuthToken();
        var groundLeaseId = $"groundle-{Guid.NewGuid():N}";
        var caseId = StewardshipCaseId.Generate().Value;

        using var scope = factory.Services.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
        await using var session = store.LightweightSession(ApiFactory.TestTrustId);

        var openedEvent = new StewardshipCaseOpened(
            caseId,
            groundLeaseId,
            new DateOnly(2026, 1, 1),
            "user-system");

        session.Events.StartStream<StewardshipCase>(caseId, openedEvent);
        await session.SaveChangesAsync();

        return (client, caseId);
    }
}
