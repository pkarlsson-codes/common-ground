using System.Net;
using System.Net.Http.Json;
using CommonGround.API.Contracts.HomeBuyers.Requests;
using CommonGround.API.Contracts.HomeBuyers.Responses;
using CommonGround.API.Contracts.Homeownership.Requests;
using CommonGround.API.Contracts.Homeownership.Responses;
using CommonGround.API.Contracts.PropertyRegistry.Requests;
using CommonGround.API.Contracts.PropertyRegistry.Responses;
using CommonGround.HomeBuyers.Contracts.Events;
using CommonGround.SharedKernel.Infrastructure.API;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;

namespace CommonGround.API.IntegrationTests.HomeBuyers;

/// <summary>
/// Covers the home-driven applicant selection flow: the <c>HomeAvailabilityClaimView</c>
/// concurrency guard, the Home status transition to UnderContract, and ApplicationId
/// propagation onto the resulting PurchaseWorkflow.
///
/// As in <see cref="Homeownership.PurchaseWorkflowCloseTests"/>, the test host runs Wolverine
/// in MediatorOnly mode, so the async Marten-to-Wolverine forwarding of raw domain events does
/// not run, and <c>IMessageBus.PublishAsync</c> (used by the handlers that translate a domain
/// event into an integration event) throws under MediatorOnly. Two integration events are
/// therefore constructed directly and dispatched via <c>InvokeForTenantAsync</c>, skipping the
/// domain-event-to-integration-event translation step (which is not part of what this test
/// covers) while still exercising the real local handlers that react to those integration events:
/// - PropertyRegistry.Contracts.Events.HomeRegistered → seeds HomeBuyers's HomeFactsView.
/// - HomeBuyers.Contracts.Events.ApplicationSelected → fans out to PropertyRegistry's
///   (marks the Home UnderContract) and Homeownership's (creates the PurchaseWorkflow) handlers.
/// </summary>
[Collection(IntegrationCollection.Name)]
public class SelectApplicantFromHomeTests(ApiFactory factory)
{
    [Fact]
    public async Task SelectApplicant_HappyPath_MarksHomeUnderContractAndCreatesWorkflowLinkedToApplicationAndHousehold()
    {
        var client = factory.CreateClient().WithAuthToken();

        var landParcelId = await CreateLandParcelAsync(client);
        var homeId = await CreateHomeAsync(client, landParcelId);
        await SeedHomeFactsViewAsync(homeId, landParcelId);

        var templateId = await CreateActiveGroundLeaseTemplateAsync(client);
        var applicant = await CreateApprovedApplicationAsync(client, "Happy Path Applicant", "happy-path@example.com");

        var selectedDate = new DateOnly(2026, 7, 20);
        var selectResponse = await client.PostAsJsonAsync(
            $"/api/homeowner-applications/{applicant.ApplicationId}/select",
            new SelectApplicantRequest(homeId, templateId, 200000m, selectedDate));
        selectResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var application = await (await client.GetAsync($"/api/homeowner-applications/{applicant.ApplicationId}"))
            .Content.ReadFromJsonAsync<HomeownerApplicationResponse>();
        application!.Phase.Should().Be("Selected");

        await InvokeApplicationSelectedForwardingAsync(
            applicant.ApplicationId, applicant.HouseholdId, applicant.FullName, applicant.Email,
            homeId, landParcelId, templateId, 200000m, selectedDate);

        var home = await (await client.GetAsync($"/api/homes/{homeId}"))
            .Content.ReadFromJsonAsync<HomeResponse>();
        home!.Status.Should().Be("UnderContract");

        var workflow = await FindPurchaseWorkflowAsync(client, templateId);
        workflow.Should().NotBeNull("selecting an applicant should automatically initiate a purchase workflow");

        var workflowDetail = await (await client.GetAsync($"/api/purchase-workflows/{workflow!.Id}"))
            .Content.ReadFromJsonAsync<PurchaseWorkflowResponse>();
        workflowDetail!.ApplicationId.Should().Be(applicant.ApplicationId);
        workflowDetail.HouseholdId.Should().Be(applicant.HouseholdId);
        workflowDetail.HomeId.Should().Be(homeId);
        workflowDetail.LandParcelId.Should().Be(landParcelId);
    }

    [Fact]
    public async Task SelectApplicant_HomeAlreadyClaimedByAnotherApplication_IsRejected()
    {
        var client = factory.CreateClient().WithAuthToken();

        var landParcelId = await CreateLandParcelAsync(client);
        var homeId = await CreateHomeAsync(client, landParcelId);
        await SeedHomeFactsViewAsync(homeId, landParcelId);

        var templateId = await CreateActiveGroundLeaseTemplateAsync(client);
        var firstApplicant = await CreateApprovedApplicationAsync(client, "First Claimant", "first-claimant@example.com");
        var secondApplicant = await CreateApprovedApplicationAsync(client, "Second Claimant", "second-claimant@example.com");

        var selectedDate = new DateOnly(2026, 7, 20);

        var firstSelectResponse = await client.PostAsJsonAsync(
            $"/api/homeowner-applications/{firstApplicant.ApplicationId}/select",
            new SelectApplicantRequest(homeId, templateId, 200000m, selectedDate));
        firstSelectResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var secondSelectResponse = await client.PostAsJsonAsync(
            $"/api/homeowner-applications/{secondApplicant.ApplicationId}/select",
            new SelectApplicantRequest(homeId, templateId, 210000m, selectedDate));
        secondSelectResponse.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        var secondApplication = await (await client.GetAsync($"/api/homeowner-applications/{secondApplicant.ApplicationId}"))
            .Content.ReadFromJsonAsync<HomeownerApplicationResponse>();
        secondApplication!.Phase.Should().Be(
            "Approved", "the rejected selection must not advance the second applicant's phase");
    }

    private async Task SeedHomeFactsViewAsync(string homeId, string landParcelId)
    {
        using var scope = factory.Services.CreateScope();
        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

        await bus.InvokeForTenantAsync(
            ApiFactory.TestTrustId,
            new CommonGround.PropertyRegistry.Contracts.Events.HomeRegistered(homeId, landParcelId, Bedrooms: 3, IsAccessible: false));
    }

    private async Task InvokeApplicationSelectedForwardingAsync(
        string applicationId, string householdId, string applicantFullName, string applicantEmail,
        string homeId, string landParcelId, string groundLeaseTemplateId,
        decimal purchasePrice, DateOnly selectedDate)
    {
        using var scope = factory.Services.CreateScope();
        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

        await bus.InvokeForTenantAsync(
            ApiFactory.TestTrustId,
            new ApplicationSelected(
                applicationId,
                householdId,
                applicantFullName,
                applicantEmail,
                null,
                homeId,
                landParcelId,
                groundLeaseTemplateId,
                purchasePrice,
                selectedDate,
                "user-test"));
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
        var created = await response.Content.ReadFromJsonAsync<RegisterLandParcelResponse>();
        return created!.ParcelId;
    }

    private static async Task<string> CreateHomeAsync(HttpClient client, string landParcelId)
    {
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
        var created = await response.Content.ReadFromJsonAsync<RegisterHomeResponse>();
        return created!.HomeId;
    }

    private static async Task<string> CreateActiveGroundLeaseTemplateAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/ground-lease-templates",
            new CreateGroundLeaseTemplateRequest(
                $"Standard 99-Year {Guid.NewGuid():N}",
                99,
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

    private static async Task<(string ApplicationId, string HouseholdId, string FullName, string Email)> CreateApprovedApplicationAsync(
        HttpClient client, string fullName, string email)
    {
        var registerResponse = await client.PostAsJsonAsync("/api/households",
            new RegisterHouseholdRequest(
                [
                    new HouseholdMemberRequest(fullName, email, "555-0100", true),
                    new HouseholdMemberRequest("Second Member", "second-member@example.com", "555-0101", false),
                    new HouseholdMemberRequest("Third Member", "third-member@example.com", "555-0102", false),
                ],
                false,
                65000m));
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var registered = await registerResponse.Content.ReadFromJsonAsync<RegisterHouseholdResponse>();
        var householdId = registered!.HouseholdId;

        (await client.PostAsJsonAsync($"/api/households/{householdId}/housing-counseling",
            new RecordHousingCounselingRequest("CERT-001", new DateOnly(2026, 6, 2), "Test Counseling Org")))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        (await client.PostAsJsonAsync($"/api/households/{householdId}/clt-orientation",
            new RecordCltOrientationRequest(new DateOnly(2026, 6, 3))))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        (await client.PostAsJsonAsync($"/api/households/{householdId}/mortgage-prequalification",
            new RecordMortgagePrequalificationRequest("Test Lender", 210000m, new DateOnly(2027, 1, 1))))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        var submitResponse = await client.PostAsJsonAsync("/api/homeowner-applications",
            new SubmitApplicationRequest(householdId));
        submitResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var submitted = await submitResponse.Content.ReadFromJsonAsync<SubmitApplicationResponse>();
        var applicationId = submitted!.ApplicationId;

        (await client.PostAsJsonAsync($"/api/homeowner-applications/{applicationId}/approve",
            new ApproveApplicationRequest(new DateOnly(2026, 6, 1), null)))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        var application = await (await client.GetAsync($"/api/homeowner-applications/{applicationId}"))
            .Content.ReadFromJsonAsync<HomeownerApplicationResponse>();
        application!.Phase.Should().Be("Approved");
        application.HouseholdId.Should().Be(householdId);
        application.HouseholdSize.Should().Be(3);
        application.PrimaryContact.Should().Be(new HouseholdContactDto(fullName, email, "555-0100"));

        return (applicationId, householdId, fullName, email);
    }

    private static async Task<PurchaseWorkflowListItemResponse?> FindPurchaseWorkflowAsync(HttpClient client, string groundLeaseTemplateId)
    {
        var page = await (await client.GetAsync($"/api/purchase-workflows?groundLeaseTemplateId={groundLeaseTemplateId}"))
            .Content.ReadFromJsonAsync<PaginatedResponse<PurchaseWorkflowListItemResponse>>();

        return page!.Items.SingleOrDefault();
    }
}
