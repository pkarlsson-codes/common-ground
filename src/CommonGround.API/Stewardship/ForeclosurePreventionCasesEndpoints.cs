using CommonGround.API.Contracts.Stewardship.Requests;
using CommonGround.API.Infrastructure.Auth;
using CommonGround.Homeownership.Application.Queries;
using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;
using CommonGround.SharedKernel.Domain.Services;
using CommonGround.SharedKernel.Infrastructure.API;
using CommonGround.Stewardship.Application.Commands.ForeclosurePreventionCases;
using CommonGround.Stewardship.Application.Queries;
using CommonGround.Stewardship.Domain.ForeclosurePreventionCases;
using CommonGround.Stewardship.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Wolverine.Http;

namespace CommonGround.API.Stewardship;

public static class ForeclosurePreventionCasesEndpoints
{
    [WolverineGet("/api/stewardship/foreclosure-prevention/{id}")]
    public static async Task<IResult> GetById(string id, HttpContext httpContext, IMessageBus bus, CancellationToken ct)
    {
        var result = await bus.InvokeForTenantAsync<ForeclosurePreventionCaseView?>(bus.TenantId!,
            new GetForeclosurePreventionCaseQuery(id), ct);
        return result is null ? Results.NotFound() : Results.Ok(result);
    }

    [WolverineGet("/api/stewardship/foreclosure-prevention")]
    public static async Task<IResult> GetByGroundLease([FromQuery] GetForeclosurePreventionCasesQuerySpec spec, HttpContext httpContext, IMessageBus bus, CancellationToken ct)
    {
        PaginatedResponse<ForeclosurePreventionCaseView> result = string.IsNullOrWhiteSpace(spec.GroundLeaseId)
            ? await bus.InvokeForTenantAsync<PaginatedResponse<ForeclosurePreventionCaseView>>(
                bus.TenantId!, new GetAllForeclosurePreventionCasesQuery(spec.Page), ct)
            : await bus.InvokeForTenantAsync<PaginatedResponse<ForeclosurePreventionCaseView>>(
                bus.TenantId!, new GetForeclosurePreventionCasesByGroundLeaseQuery(spec.GroundLeaseId, spec.Page), ct);
        return Results.Ok(result);
    }

    [WolverinePost("/api/stewardship/foreclosure-prevention")]
    public static async Task<IResult> OpenForServicerDelinquency(OpenForeclosurePreventionCaseRequest request, HttpContext httpContext, IMessageBus bus, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var userId = httpContext.GetUserId();

        var id = ForeclosurePreventionCaseId.Generate();
        await bus.InvokeForTenantAsync(bus.TenantId!,
            new OpenForeclosurePreventionCaseCommand(
                id,
                request.GroundLeaseId,
                request.ServicerName,
                request.DelinquentMonths,
                request.RecordedDate,
                userId),
            ct).ConfigureAwait(false);

        return Results.Accepted(value: new { Id = id.Value });
    }

    // Opens a case for a lease whose notice of default is unpaid (cure period running or deadline passed). The
    // figures come from the lease's fee ledger as of the trust's today, never from the client.
    [WolverinePost("/api/stewardship/foreclosure-prevention/lease-default")]
    public static async Task<IResult> OpenForLeaseDefault(
        OpenForeclosurePreventionCaseForLeaseDefaultRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        ITrustClock trustClock,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var userId = httpContext.GetUserId();
        var tenantId = bus.TenantId!;

        var lease = await bus.InvokeForTenantAsync<GroundLeaseDetails>(tenantId,
            new GetGroundLease(GroundLeaseId.Create(request.GroundLeaseId)), ct).ConfigureAwait(false);
        var today = await trustClock.TodayAsync(tenantId, ct).ConfigureAwait(false);

        var id = ForeclosurePreventionCaseId.Generate();
        await bus.InvokeForTenantAsync(tenantId,
            new OpenForeclosurePreventionCaseForLeaseDefaultCommand(id, lease.Lease.Id, UnpaidDefaultOf(lease.DefaultCure), today, userId),
            ct).ConfigureAwait(false);

        return Results.Accepted(value: new { Id = id.Value });
    }

    [WolverinePost("/api/stewardship/foreclosure-prevention/{id}/servicer-delinquency")]
    public static async Task<IResult> RecordServicerDelinquency(string id, RecordServicerDelinquencyRequest request, HttpContext httpContext, IMessageBus bus, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var userId = httpContext.GetUserId();
        await bus.InvokeForTenantAsync(bus.TenantId!, new RecordServicerDelinquencyCommand(
            ForeclosurePreventionCaseId.Create(id), request.ServicerName, request.DelinquentMonths, request.RecordedDate, userId), ct);
        return Results.NoContent();
    }

    [WolverinePost("/api/stewardship/foreclosure-prevention/{id}/counseling-referrals")]
    public static async Task<IResult> MakeCounselingReferral(string id, MakeCounselingReferralRequest request, HttpContext httpContext, IMessageBus bus, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var userId = httpContext.GetUserId();
        await bus.InvokeForTenantAsync(bus.TenantId!, new MakeCounselingReferralCommand(
            ForeclosurePreventionCaseId.Create(id), request.AgencyName, request.ReferralDate, userId), ct);
        return Results.NoContent();
    }

    [WolverinePost("/api/stewardship/foreclosure-prevention/{id}/counseling-referrals/{referralId}/outcome")]
    public static async Task<IResult> RecordCounselingOutcome(string id, Guid referralId, RecordCounselingOutcomeRequest request, HttpContext httpContext, IMessageBus bus, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var userId = httpContext.GetUserId();
        await bus.InvokeForTenantAsync(bus.TenantId!, new RecordCounselingOutcomeCommand(
            ForeclosurePreventionCaseId.Create(id), referralId, request.Outcome, request.OutcomeDate, userId), ct);
        return Results.NoContent();
    }

    [WolverinePost("/api/stewardship/foreclosure-prevention/{id}/forbearance-requests")]
    public static async Task<IResult> RequestForbearance(string id, RequestForbearanceRequest request, HttpContext httpContext, IMessageBus bus, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var userId = httpContext.GetUserId();
        await bus.InvokeForTenantAsync(bus.TenantId!, new RequestForbearanceCommand(
            ForeclosurePreventionCaseId.Create(id), request.LenderName, request.RequestedDate, userId), ct);
        return Results.NoContent();
    }

    [WolverinePost("/api/stewardship/foreclosure-prevention/{id}/forbearance-requests/{requestId}/outcome")]
    public static async Task<IResult> RecordForbearanceOutcome(string id, Guid requestId, RecordForbearanceOutcomeRequest request, HttpContext httpContext, IMessageBus bus, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var userId = httpContext.GetUserId();
        if (!ForbearanceOutcome.TryFromDisplayName(request.Outcome, out var outcome))
        {
            return Results.BadRequest($"Unrecognized forbearance outcome: '{request.Outcome}'.");
        }

        await bus.InvokeForTenantAsync(bus.TenantId!, new RecordForbearanceOutcomeCommand(
            ForeclosurePreventionCaseId.Create(id), requestId, outcome, request.OutcomeDate, request.Notes, userId), ct);
        return Results.NoContent();
    }

    [WolverinePost("/api/stewardship/foreclosure-prevention/{id}/cure-period")]
    public static async Task<IResult> ActivateCurePeriod(string id, ActivateCurePeriodRequest request, HttpContext httpContext, IMessageBus bus, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var userId = httpContext.GetUserId();
        await bus.InvokeForTenantAsync(bus.TenantId!, new ActivateCurePeriodCommand(
            ForeclosurePreventionCaseId.Create(id), request.LraId, request.ActivationDate, request.CurePeriodDays, userId), ct);
        return Results.NoContent();
    }

    [WolverinePost("/api/stewardship/foreclosure-prevention/{id}/resolve")]
    public static async Task<IResult> ResolveCase(string id, ResolveForeclosurePreventionCaseRequest request, HttpContext httpContext, IMessageBus bus, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var userId = httpContext.GetUserId();
        if (!Enum.TryParse<ForeclosurePreventionOutcome>(request.Outcome, out var outcome)
            || !Enum.IsDefined(outcome))
        {
            return Results.BadRequest($"Unrecognized resolution outcome: '{request.Outcome}'.");
        }

        await bus.InvokeForTenantAsync(bus.TenantId!, new ResolveForeclosurePreventionCaseCommand(
            ForeclosurePreventionCaseId.Create(id), outcome, request.ResolvedDate, userId), ct);
        return Results.NoContent();
    }

    /// <summary>The notice of default while it is unpaid (Curing or Uncured), or null.</summary>
    private static LeaseDefaultNotice? UnpaidDefaultOf(DefaultCure? cure) =>
        cure is not null && (cure.Status == DefaultCureStatus.Curing || cure.Status == DefaultCureStatus.Uncured)
            ? LeaseDefaultNotice.Create(cure.SentOn, cure.AmountOwed, cure.CureBy, cure.StillToPay)
            : null;
}
