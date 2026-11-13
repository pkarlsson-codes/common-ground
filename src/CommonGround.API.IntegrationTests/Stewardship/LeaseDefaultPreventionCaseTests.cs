using System.Net;
using System.Net.Http.Json;
using CommonGround.API.Contracts.Homeownership.Requests;
using CommonGround.API.Contracts.Homeownership.Responses;
using CommonGround.API.Contracts.Stewardship.Requests;
using CommonGround.API.Contracts.Today.Responses;
using CommonGround.Stewardship.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace CommonGround.API.IntegrationTests.Stewardship;

[Collection(IntegrationCollection.Name)]
public class LeaseDefaultPreventionCaseTests(ApiFactory factory)
{
    [Fact]
    public async Task OpenForLeaseDefault_OnAnUncuredDefault_RecordsTheNotice_TakesItOffToday_AndRefusesASecondCase()
    {
        var client = factory.CreateClient().WithAuthToken();
        var today = (await client.GetFromJsonAsync<TodayResponse>("/api/today"))!.Today;
        var leaseId = await CreateLease(client, new DateOnly(today.Year, today.Month, 1).AddMonths(-5));

        // Sent 40 days ago with 30 cure days and nothing paid: the deadline passed 10 days ago.
        (await client.PostAsJsonAsync($"/api/ground-leases/{leaseId}/lease-fee-notices",
            new RecordLeaseFeeNoticeRequest("Default", today.AddDays(-40), "CertifiedMail"))).StatusCode.Should().Be(HttpStatusCode.Created);
        var cure = (await client.GetFromJsonAsync<GroundLeaseResponse>($"/api/ground-leases/{leaseId}"))!.LeaseFees.DefaultCure!;
        cure.Status.Should().Be("Uncured");
        (await client.GetFromJsonAsync<TodayResponse>("/api/today"))!.Items
            .Should().ContainSingle(i => i.RecordId == leaseId && i.Kind == "lease-default-uncured");

        var opened = await client.PostAsJsonAsync("/api/stewardship/foreclosure-prevention/lease-default",
            new OpenForeclosurePreventionCaseForLeaseDefaultRequest(leaseId));
        opened.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var caseId = (await opened.Content.ReadFromJsonAsync<OpenCaseResponse>())!.Id;

        var view = await client.GetFromJsonAsync<ForeclosurePreventionCaseView>($"/api/stewardship/foreclosure-prevention/{caseId}");
        view.Should().BeEquivalentTo(new
        {
            GroundLeaseId = leaseId,
            Trigger = "LeaseFeeArrears",
            Status = "Opened",
            OpenedDate = today,
            LeaseFeeArrears = new { NoticeSentOn = today.AddDays(-40), cure.AmountOwed, cure.CureBy, StillToPay = cure.AmountOwed },
        });
        (await client.GetFromJsonAsync<TodayResponse>("/api/today"))!.Items.Should().NotContain(i => i.RecordId == leaseId);

        var again = await client.PostAsJsonAsync("/api/stewardship/foreclosure-prevention/lease-default",
            new OpenForeclosurePreventionCaseForLeaseDefaultRequest(leaseId));
        await ShouldRefuse(again, "This lease already has an open foreclosure prevention case.");
        var servicer = await client.PostAsJsonAsync("/api/stewardship/foreclosure-prevention",
            new OpenForeclosurePreventionCaseRequest(leaseId, "Test Servicer", 2, today));
        await ShouldRefuse(servicer, "This lease already has an open foreclosure prevention case.");

        // Resolving the case with the default still uncured puts it back on Today.
        (await client.PostAsJsonAsync($"/api/stewardship/foreclosure-prevention/{caseId}/counseling-referrals",
            new MakeCounselingReferralRequest("Housing Center", today))).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await client.PostAsJsonAsync($"/api/stewardship/foreclosure-prevention/{caseId}/resolve",
            new ResolveForeclosurePreventionCaseRequest("LeaseTerminated", today))).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await client.GetFromJsonAsync<TodayResponse>("/api/today"))!.Items
            .Should().ContainSingle(i => i.RecordId == leaseId && i.Kind == "lease-default-uncured");
    }

    [Fact]
    public async Task OpenForLeaseDefault_WithNoNoticeOfDefault_Returns422()
    {
        var client = factory.CreateClient().WithAuthToken();
        var today = (await client.GetFromJsonAsync<TodayResponse>("/api/today"))!.Today;
        var leaseId = await CreateLease(client, new DateOnly(today.Year, today.Month, 1).AddMonths(-3));

        var response = await client.PostAsJsonAsync("/api/stewardship/foreclosure-prevention/lease-default",
            new OpenForeclosurePreventionCaseForLeaseDefaultRequest(leaseId));

        await ShouldRefuse(response, "This lease has no unpaid notice of default.");
    }

    private static async Task ShouldRefuse(HttpResponseMessage response, string detail)
    {
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await response.Content.ReadFromJsonAsync<ProblemDetails>())!.Detail.Should().Be(detail);
    }

    private static async Task<string> CreateLease(HttpClient client, DateOnly leaseStart)
    {
        var homeowner = "Dana " + string.Concat(Guid.NewGuid().ToString("N").Where(char.IsLetter).Take(8));
        var response = await client.PostAsJsonAsync("/api/ground-leases",
            new CreateGroundLeaseRequest(
                Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), homeowner, "dana@example.com", null,
                leaseStart, 99, 60, 45m, 15m, 200_000m, 250_000m, "Standard covenant", "AppraisalBased",
                0.25m, null, null, null, null, 0m));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CreateGroundLeaseResponse>())!.GroundLeaseId;
    }

    private sealed record OpenCaseResponse(string Id);
}
