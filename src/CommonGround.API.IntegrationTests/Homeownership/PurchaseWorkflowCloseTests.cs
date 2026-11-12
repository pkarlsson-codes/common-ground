using System.Net;
using System.Net.Http.Json;
using CommonGround.API.Contracts.Homeownership.Requests;
using CommonGround.API.Contracts.Homeownership.Responses;
using CommonGround.Homeownership.Domain.PurchaseWorkflows.Events;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.SharedKernel.Infrastructure.API;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;

namespace CommonGround.API.IntegrationTests.Homeownership;

/// <summary>
/// Covers the purchase-closing flow that creates a ground lease from the workflow's template.
/// The test host runs Wolverine in MediatorOnly mode, so the async Marten-to-Wolverine event
/// forwarding does not run; the <c>PurchaseClosed</c> handler is invoked inline via the bus
/// to keep assertions deterministic.
/// </summary>
[Collection(IntegrationCollection.Name)]
public class PurchaseWorkflowCloseTests(ApiFactory factory)
{
    private static readonly DateOnly ClosingDate = new DateOnly(2026, 7, 15);

    [Fact]
    public async Task ClosePurchase_HappyPath_CreatesGroundLeaseFromTemplate()
    {
        var client = factory.CreateClient().WithAuthToken();
        var landParcelId = $"landparc-{Guid.NewGuid():N}";
        var homeId = $"home-{Guid.NewGuid():N}";

        var templateId = await CreateTemplateAsync(client);
        var workflowId = await InitiateWorkflowAsync(client, templateId, landParcelId, homeId);
        await SatisfyAllTracksAsync(client, workflowId);
        await ScheduleClosingAsync(client, workflowId);

        var closeResponse = await client.PostAsJsonAsync(
            $"/api/purchase-workflows/{workflowId}/closing/record",
            new RecordPurchaseClosedRequest(ClosingDate, "Smooth closing"));
        closeResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var workflow = await (await client.GetAsync($"/api/purchase-workflows/{workflowId}"))
            .Content.ReadFromJsonAsync<PurchaseWorkflowResponse>();
        workflow!.Phase.Should().Be("Closed");
        workflow.ActualClosingDate.Should().Be(ClosingDate);

        await InvokePurchaseClosedHandlerAsync(workflowId);

        var lease = await FindGroundLeaseAsync(client, landParcelId);
        lease.Should().NotBeNull("closing the purchase should create a ground lease via the PurchaseClosed handler");

        var detail = await (await client.GetAsync($"/api/ground-leases/{lease!.Id}"))
            .Content.ReadFromJsonAsync<GroundLeaseResponse>();

        detail!.GroundLeaseTemplateId.Should().Be(templateId);
        detail.PurchaseWorkflowId.Should().Be(workflowId);
        detail.LandParcelId.Should().Be(landParcelId);
        detail.HomeId.Should().Be(homeId);
        detail.LeaseStartDate.Should().Be(ClosingDate);
        detail.TermYears.Should().Be(99);
        detail.HomeownerFullName.Should().Be("Test Buyer");
        detail.HomeownerEmail.Should().Be("buyer@example.com");
    }

    [Fact]
    public async Task ClosePurchase_WithInactiveTemplate_GroundLeaseCreationIsRejected()
    {
        var client = factory.CreateClient().WithAuthToken();
        var landParcelId = $"landparc-{Guid.NewGuid():N}";
        var homeId = $"home-{Guid.NewGuid():N}";

        var templateId = await CreateTemplateAsync(client);
        var workflowId = await InitiateWorkflowAsync(client, templateId, landParcelId, homeId);
        await SatisfyAllTracksAsync(client, workflowId);
        await ScheduleClosingAsync(client, workflowId);

        var archiveResponse = await client.PostAsync($"/api/ground-lease-templates/{templateId}/archive", null);
        archiveResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var act = () => InvokePurchaseClosedHandlerAsync(workflowId);

        await act.Should().ThrowAsync<BusinessRuleViolationException>();

        var lease = await FindGroundLeaseAsync(client, landParcelId);
        lease.Should().BeNull("an inactive template must not produce a ground lease");
    }

    [Fact]
    public async Task ScheduleClosing_WithoutAppraisal_IsRejectedUntilTheAppraisalIsRecorded()
    {
        var client = factory.CreateClient().WithAuthToken();
        var templateId = await CreateTemplateAsync(client);
        var workflowId = await InitiateWorkflowAsync(client, templateId, $"landparc-{Guid.NewGuid():N}", $"home-{Guid.NewGuid():N}");
        await SatisfyAllTracksAsync(client, workflowId, includeAppraisal: false);

        var rejected = await client.PostAsJsonAsync(
            $"/api/purchase-workflows/{workflowId}/closing/schedule", new ScheduleClosingRequest(ClosingDate));

        rejected.IsSuccessStatusCode.Should().BeFalse("a purchase with no appraisal cannot schedule closing");

        var appraisal = await client.PostAsJsonAsync($"/api/purchase-workflows/{workflowId}/appraisal",
            new RecordAppraisalCompletedRequest(200000m, new DateOnly(2026, 6, 2), "Test Appraiser", "Fair"));
        appraisal.StatusCode.Should().Be(HttpStatusCode.NoContent);
        await ScheduleClosingAsync(client, workflowId);

        var workflow = await client.GetFromJsonAsync<PurchaseWorkflowResponse>($"/api/purchase-workflows/{workflowId}");
        workflow!.AppraisalDetails.Should().BeEquivalentTo(
            new AppraisalDetailsDto(200000m, new DateOnly(2026, 6, 2), "Test Appraiser", "Fair"));
        workflow.Phase.Should().Be("ClosingScheduled");
    }

    [Fact]
    public async Task RecordAppraisal_ValueNotAboveZero_IsRejected()
    {
        var client = factory.CreateClient().WithAuthToken();
        var templateId = await CreateTemplateAsync(client);
        var workflowId = await InitiateWorkflowAsync(client, templateId, $"landparc-{Guid.NewGuid():N}", $"home-{Guid.NewGuid():N}");

        var response = await client.PostAsJsonAsync($"/api/purchase-workflows/{workflowId}/appraisal",
            new RecordAppraisalCompletedRequest(0m, new DateOnly(2026, 6, 2), "Test Appraiser", null));

        response.IsSuccessStatusCode.Should().BeFalse();
    }

    private async Task InvokePurchaseClosedHandlerAsync(string workflowId)
    {
        using var scope = factory.Services.CreateScope();
        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

        await bus.InvokeForTenantAsync(
            ApiFactory.TestTrustId,
            new PurchaseClosed(
                workflowId,
                ClosingDate,
                null,
                "user-test"));
    }

    private static async Task<string> CreateTemplateAsync(HttpClient client)
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
                null));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<CreateGroundLeaseTemplateResponse>();
        return created!.GroundLeaseTemplateId;
    }

    private static async Task<string> InitiateWorkflowAsync(
        HttpClient client, string templateId, string landParcelId, string homeId)
    {
        var response = await client.PostAsJsonAsync("/api/purchase-workflows",
            new InitiatePurchaseWorkflowRequest(
                templateId,
                landParcelId,
                homeId,
                "Test Buyer",
                "buyer@example.com",
                null,
                150000m));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<InitiatePurchaseWorkflowResponse>();
        return created!.PurchaseWorkflowId;
    }

    private static async Task SatisfyAllTracksAsync(HttpClient client, string workflowId, bool includeAppraisal = true)
    {
        var responses = new List<HttpResponseMessage>
        {
            await client.PostAsJsonAsync($"/api/purchase-workflows/{workflowId}/eligibility",
                new RecordEligibilityVerifiedRequest(new DateOnly(2026, 6, 1), 50_000m, 1, null)),
            await client.PostAsJsonAsync($"/api/purchase-workflows/{workflowId}/financing",
                new RecordFinancingCommittedRequest("Test Lender", 140000m, new DateOnly(2026, 6, 2))),
            await client.PostAsJsonAsync($"/api/purchase-workflows/{workflowId}/inspection",
                new RecordInspectionCompletedRequest(new DateOnly(2026, 6, 3), "Jane Inspector", null)),
            await client.PostAsJsonAsync($"/api/purchase-workflows/{workflowId}/title",
                new RecordTitleClearedRequest(new DateOnly(2026, 6, 4), "Test Title Co")),
            await client.PostAsJsonAsync($"/api/purchase-workflows/{workflowId}/lease-execution",
                new RecordLeaseExecutedRequest(new DateOnly(2026, 6, 5))),
        };

        if (includeAppraisal)
        {
            responses.Add(await client.PostAsJsonAsync($"/api/purchase-workflows/{workflowId}/appraisal",
                new RecordAppraisalCompletedRequest(200000m, new DateOnly(2026, 6, 2), "Test Appraiser", null)));
        }

        responses.Should().AllSatisfy(r => r.StatusCode.Should().Be(HttpStatusCode.NoContent));
    }

    private static async Task ScheduleClosingAsync(HttpClient client, string workflowId)
    {
        var response = await client.PostAsJsonAsync(
            $"/api/purchase-workflows/{workflowId}/closing/schedule",
            new ScheduleClosingRequest(ClosingDate));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    private static async Task<GroundLeaseListItemResponse?> FindGroundLeaseAsync(
        HttpClient client, string landParcelId)
    {
        var page = await (await client.GetAsync($"/api/ground-leases?landParcelId={landParcelId}"))
            .Content.ReadFromJsonAsync<PaginatedResponse<GroundLeaseListItemResponse>>();

        return page!.Items.SingleOrDefault();
    }
}
