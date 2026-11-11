using System.Net;
using System.Net.Http.Json;
using CommonGround.API.Contracts.Homeownership.Requests;
using CommonGround.API.Contracts.Homeownership.Responses;
using CommonGround.API.Contracts.Today.Responses;
using CommonGround.SharedKernel.Infrastructure.API;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace CommonGround.API.IntegrationTests.Homeownership;

[Collection(IntegrationCollection.Name)]
public class LeaseFeesTests(ApiFactory factory)
{
    [Fact]
    public async Task RecordAndVoidPayment_ChangesTheBalanceOnTheLeaseAndInTheBalancesList()
    {
        var client = factory.CreateClient().WithAuthToken();
        var today = (await client.GetFromJsonAsync<TodayResponse>("/api/today"))!.Today;
        var thisMonth = new DateOnly(today.Year, today.Month, 1);
        var homeowner = UniqueName();
        var leaseId = await CreateLease(client, homeowner, leaseStart: thisMonth.AddMonths(-2));

        var recorded = await client.PostAsJsonAsync($"/api/ground-leases/{leaseId}/lease-fee-payments",
            new RecordLeaseFeePaymentRequest(today, 60m, "Check", "Check #1001"));
        recorded.StatusCode.Should().Be(HttpStatusCode.Created);
        var paymentId = (await recorded.Content.ReadFromJsonAsync<RecordLeaseFeePaymentResponse>())!.PaymentId;

        var afterPayment = await client.GetFromJsonAsync<GroundLeaseResponse>($"/api/ground-leases/{leaseId}");
        afterPayment!.LeaseFees.Should().BeEquivalentTo(new
        {
            MonthlyFee = 60m,
            Statement = new { TotalCharged = 180m, Balance = 120m, OldestUnpaidDueDate = (DateOnly?)thisMonth.AddMonths(-1) },
        });

        var listed = await client.GetFromJsonAsync<PaginatedResponse<LeaseFeeBalanceResponse>>(
            $"/api/lease-fee-balances?search={Uri.EscapeDataString(homeowner)}");
        listed!.Items.Should().ContainSingle().Which.Balance.Should().Be(120m);

        (await client.PostAsJsonAsync($"/api/ground-leases/{leaseId}/lease-fee-payments/{paymentId}/void",
            new VoidLeaseFeePaymentRequest("Check bounced"))).EnsureSuccessStatusCode();

        var afterVoid = await client.GetFromJsonAsync<GroundLeaseResponse>($"/api/ground-leases/{leaseId}");
        afterVoid!.LeaseFees.Should().BeEquivalentTo(new
        {
            Statement = new { Balance = 180m },
            Payments = new[] { new { PaymentId = paymentId, VoidReason = "Check bounced" } },
        });
    }

    [Fact]
    public async Task RecordPayment_InTheFuture_Returns422()
    {
        var client = factory.CreateClient().WithAuthToken();
        var today = (await client.GetFromJsonAsync<TodayResponse>("/api/today"))!.Today;
        var leaseId = await CreateLease(client, UniqueName(), leaseStart: today.AddYears(-1));

        var response = await client.PostAsJsonAsync($"/api/ground-leases/{leaseId}/lease-fee-payments",
            new RecordLeaseFeePaymentRequest(today.AddDays(1), 60m, "Cash", null));

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem!.Detail.Should().Be("Received date can't be in the future.");
    }

    [Fact]
    public async Task GetLeaseFeeBalances_PaidUpLease_IsListedOnlyWithAllLeases()
    {
        var client = factory.CreateClient().WithAuthToken();
        var today = (await client.GetFromJsonAsync<TodayResponse>("/api/today"))!.Today;
        var homeowner = UniqueName();
        var leaseId = await CreateLease(client, homeowner, leaseStart: new DateOnly(today.Year, today.Month, 1));
        (await client.PostAsJsonAsync($"/api/ground-leases/{leaseId}/lease-fee-payments",
            new RecordLeaseFeePaymentRequest(today, 60m, "BankTransfer", null))).EnsureSuccessStatusCode();
        var search = Uri.EscapeDataString(homeowner);

        var due = await client.GetFromJsonAsync<PaginatedResponse<LeaseFeeBalanceResponse>>($"/api/lease-fee-balances?search={search}");
        var all = await client.GetFromJsonAsync<PaginatedResponse<LeaseFeeBalanceResponse>>($"/api/lease-fee-balances?includeAll=true&search={search}");

        due!.Items.Should().BeEmpty();
        all!.Items.Should().ContainSingle().Which.Balance.Should().Be(0m);
    }

    [Fact]
    public async Task RecordNotice_OnALeaseWithANoticeDue_TakesItOffTheNoticesDueAndTodayAndShowsItOnTheLease()
    {
        var client = factory.CreateClient().WithAuthToken();
        var today = (await client.GetFromJsonAsync<TodayResponse>("/api/today"))!.Today;
        var homeowner = UniqueName();
        var leaseStart = new DateOnly(today.Year, today.Month, 1).AddMonths(-2);
        var leaseId = await CreateLease(client, homeowner, leaseStart: leaseStart);

        // Two months behind is 59 to 62 days overdue: the notice of default is due from 60.
        var level = today.DayNumber - leaseStart.DayNumber >= 60 ? "Default" : "Second";
        var due = await client.GetFromJsonAsync<LeaseFeeNoticesDueResponse>($"/api/lease-fee-notices/due?groundLeaseId={leaseId}");
        due!.Notices.Should().ContainSingle().Which.Should().BeEquivalentTo(new { NoticeDue = level, Balance = 180m });
        var listed = await client.GetFromJsonAsync<PaginatedResponse<LeaseFeeBalanceResponse>>(
            $"/api/lease-fee-balances?noticeDueOnly=true&search={Uri.EscapeDataString(homeowner)}");
        listed!.Items.Should().ContainSingle();
        (await client.GetFromJsonAsync<TodayResponse>("/api/today"))!.Items.Should().ContainSingle(i => i.RecordId == leaseId)
            .Which.Should().BeEquivalentTo(new
            {
                Area = "stewardship",
                Kind = "lease-fee-notice-due",
                RecordType = "ground-lease",
                Actions = new[] { "record-lease-fee-notice" },
            });

        var recorded = await client.PostAsJsonAsync($"/api/ground-leases/{leaseId}/lease-fee-notices",
            new RecordLeaseFeeNoticeRequest(level, today, "CertifiedMail"));
        recorded.StatusCode.Should().Be(HttpStatusCode.Created);

        var lease = await client.GetFromJsonAsync<GroundLeaseResponse>($"/api/ground-leases/{leaseId}");
        lease!.LeaseFees.Should().BeEquivalentTo(new
        {
            NoticeDue = (string?)null,
            Notices = new[] { new { Level = level, SentOn = today, Delivery = "CertifiedMail", BalanceDue = 180m } },
        });
        (await client.GetFromJsonAsync<LeaseFeeNoticesDueResponse>($"/api/lease-fee-notices/due?groundLeaseId={leaseId}"))!
            .Notices.Should().BeEmpty();
        (await client.GetFromJsonAsync<TodayResponse>("/api/today"))!.Items.Should().NotContain(i => i.RecordId == leaseId);
    }

    [Fact]
    public async Task RecordNotice_OnAPaidUpLease_Returns422()
    {
        var client = factory.CreateClient().WithAuthToken();
        var today = (await client.GetFromJsonAsync<TodayResponse>("/api/today"))!.Today;
        var leaseId = await CreateLease(client, UniqueName(), leaseStart: new DateOnly(today.Year, today.Month, 1));
        (await client.PostAsJsonAsync($"/api/ground-leases/{leaseId}/lease-fee-payments",
            new RecordLeaseFeePaymentRequest(today, 60m, "Check", null))).EnsureSuccessStatusCode();

        var response = await client.PostAsJsonAsync($"/api/ground-leases/{leaseId}/lease-fee-notices",
            new RecordLeaseFeeNoticeRequest("First", today, "FirstClassMail"));

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await response.Content.ReadFromJsonAsync<ProblemDetails>())!.Detail.Should().Be("This lease had no balance due on that date.");
    }

    [Fact]
    public async Task SetNoticePolicy_ChangesWhenNoticesFallDue_AndRefusesASecondNoticeNotAfterTheFirst()
    {
        var client = factory.CreateClient().WithAuthToken();
        var today = (await client.GetFromJsonAsync<TodayResponse>("/api/today"))!.Today;
        var leaseId = await CreateLease(client, UniqueName(), leaseStart: new DateOnly(today.Year, today.Month, 1).AddMonths(-2));

        try
        {
            var saved = await client.PutAsJsonAsync("/api/lease-fee-notice-policy", new SetLeaseFeeNoticePolicyRequest(100, 200, 300, 20, 10, "  Riverbend CLT  "));
            saved.EnsureSuccessStatusCode();
            (await saved.Content.ReadFromJsonAsync<LeaseFeeNoticePolicyResponse>()).Should().BeEquivalentTo(
                new LeaseFeeNoticePolicyResponse(100, 200, 300, 20, 10, "Riverbend CLT", false));
            var due = await client.GetFromJsonAsync<LeaseFeeNoticesDueResponse>($"/api/lease-fee-notices/due?groundLeaseId={leaseId}");
            due!.Should().BeEquivalentTo(new { LetterSignOff = "Riverbend CLT", CureDays = 20, CureExtensionDays = 10, Notices = Array.Empty<object>() });

            var refused = await client.PutAsJsonAsync("/api/lease-fee-notice-policy", new SetLeaseFeeNoticePolicyRequest(10, 10, 60, 30, 30, null));
            refused.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            (await refused.Content.ReadFromJsonAsync<ProblemDetails>())!.Detail.Should().Be("Second notice must come more days overdue than the first.");
        }
        finally
        {
            // Other tests share the trust and expect the default days.
            (await client.PutAsJsonAsync("/api/lease-fee-notice-policy", new SetLeaseFeeNoticePolicyRequest(10, 30, 60, 30, 30, null))).EnsureSuccessStatusCode();
        }
    }

    [Fact]
    public async Task RecordNoticeOfDefault_RefusesOrdinaryMail_StartsTheCurePeriod_AndPayingTheAmountCuresIt()
    {
        var client = factory.CreateClient().WithAuthToken();
        var today = (await client.GetFromJsonAsync<TodayResponse>("/api/today"))!.Today;
        var leaseId = await CreateLease(client, UniqueName(), leaseStart: new DateOnly(today.Year, today.Month, 1).AddMonths(-3));

        var due = await client.GetFromJsonAsync<LeaseFeeNoticesDueResponse>($"/api/lease-fee-notices/due?groundLeaseId={leaseId}");
        due!.Should().BeEquivalentTo(new { CureDays = 30, CureExtensionDays = 30, Notices = new[] { new { NoticeDue = "Default" } } });

        var refused = await client.PostAsJsonAsync($"/api/ground-leases/{leaseId}/lease-fee-notices",
            new RecordLeaseFeeNoticeRequest("Default", today, "FirstClassMail"));
        refused.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await refused.Content.ReadFromJsonAsync<ProblemDetails>())!.Detail.Should().Be(
            "A notice of default must be handed over in person or sent by certified or registered mail (Art 14.2).");

        (await client.PostAsJsonAsync($"/api/ground-leases/{leaseId}/lease-fee-notices",
            new RecordLeaseFeeNoticeRequest("Default", today, "CertifiedMail"))).StatusCode.Should().Be(HttpStatusCode.Created);

        var lease = await client.GetFromJsonAsync<GroundLeaseResponse>($"/api/ground-leases/{leaseId}");
        lease!.LeaseFees.Should().BeEquivalentTo(new
        {
            NoticeDue = (string?)null,
            Notices = new[] { new { Level = "Default", CureDays = 30, CureExtensionDays = 30 } },
            DefaultCure = new { SentOn = today, AmountOwed = 240m, CureBy = today.AddDays(30), Status = "Curing", StillToPay = 240m },
        });

        (await client.PostAsJsonAsync($"/api/ground-leases/{leaseId}/lease-fee-payments",
            new RecordLeaseFeePaymentRequest(today, 240m, "Check", null))).EnsureSuccessStatusCode();

        var cured = await client.GetFromJsonAsync<GroundLeaseResponse>($"/api/ground-leases/{leaseId}");
        cured!.LeaseFees.DefaultCure.Should().BeEquivalentTo(new { Status = "Cured", PaidInFullOn = today, StillToPay = 0m });
    }

    [Fact]
    public async Task WaiveInterest_OnALeaseWithInterestOwed_LowersTheBalanceAndRefusesMoreThanIsOwed()
    {
        var client = factory.CreateClient().WithAuthToken();
        var today = (await client.GetFromJsonAsync<TodayResponse>("/api/today"))!.Today;
        var oldest = new DateOnly(today.Year, today.Month, 1).AddMonths(-2);
        var leaseId = await CreateLease(client, UniqueName(), leaseStart: oldest, lateInterestRate: 36.5m);

        // 36.5% a year on $60 is 6 cents a day. The oldest charge is always more than 30 days late; the others may not be.
        var dues = new[] { oldest, oldest.AddMonths(1), oldest.AddMonths(2) };
        var days = dues.Select(d => today.DayNumber - d.DayNumber).ToList();
        var interest = days.Where(d => d > 30).Sum(d => 0.06m * d);
        var accruing = days.Where(d => d <= 30).Sum(d => 0.06m * d);

        var lease = await client.GetFromJsonAsync<GroundLeaseResponse>($"/api/ground-leases/{leaseId}");
        lease!.LeaseFees.Should().BeEquivalentTo(new
        {
            LateInterestRate = 36.5m,
            InterestWaiverDays = 30,
            Statement = new { InterestDue = interest, InterestAccruing = accruing, Balance = 180m + interest },
        });

        var refused = await client.PostAsJsonAsync($"/api/ground-leases/{leaseId}/lease-fee-interest-waivers",
            new WaiveLeaseFeeInterestRequest(interest + 0.01m, "Hardship"));
        refused.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await refused.Content.ReadFromJsonAsync<ProblemDetails>())!.Detail.Should().Be("That is more than the interest owed.");

        var waived = await client.PostAsJsonAsync($"/api/ground-leases/{leaseId}/lease-fee-interest-waivers",
            new WaiveLeaseFeeInterestRequest(interest, "  Job loss  "));
        waived.StatusCode.Should().Be(HttpStatusCode.Created);

        var after = await client.GetFromJsonAsync<GroundLeaseResponse>($"/api/ground-leases/{leaseId}");
        after!.LeaseFees.Should().BeEquivalentTo(new
        {
            Statement = new { InterestDue = 0m, InterestWaived = interest, Balance = 180m },
            InterestWaivers = new[] { new { WaivedOn = today, Amount = interest, Reason = "Job loss" } },
        });
    }

    private static string UniqueName() =>
        "Dana " + string.Concat(Guid.NewGuid().ToString("N").Where(char.IsLetter).Take(8).Select(char.ToUpperInvariant).Select((c, i) => i == 0 ? c : char.ToLowerInvariant(c)));

    private static async Task<string> CreateLease(HttpClient client, string homeowner, DateOnly leaseStart, decimal lateInterestRate = 0m)
    {
        var response = await client.PostAsJsonAsync("/api/ground-leases",
            new CreateGroundLeaseRequest(
                Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), homeowner, "dana@example.com", null,
                leaseStart, 99, 60, 45m, 15m, 200_000m, 250_000m, "Standard covenant", "AppraisalBased",
                0.25m, null, null, null, null, lateInterestRate));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CreateGroundLeaseResponse>())!.GroundLeaseId;
    }
}
