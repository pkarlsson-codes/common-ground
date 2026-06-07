using CommonGround.API.Contracts.Stewardship.Requests;
using CommonGround.API.Infrastructure.Auth;
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

        var id = await bus.InvokeForTenantAsync<string>(bus.TenantId!,
            new OpenForeclosurePreventionCaseCommand(
                request.GroundLeaseId,
                request.ServicerName,
                request.DelinquentMonths,
                request.RecordedDate,
                userId),
            ct).ConfigureAwait(false);

        return Results.Accepted(value: new { Id = id });
    }

    [WolverinePost("/api/stewardship/foreclosure-prevention/{id}/servicer-delinquency")]
    public static async Task<IResult> RecordServicerDelinquency(string id, RecordServicerDelinquencyRequest request, HttpContext httpContext, IMessageBus bus, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var userId = httpContext.GetUserId();
        await bus.InvokeForTenantAsync(bus.TenantId!, new RecordServicerDelinquencyCommand(
            id, request.ServicerName, request.DelinquentMonths, request.RecordedDate, userId), ct);
        return Results.NoContent();
    }

    [WolverinePost("/api/stewardship/foreclosure-prevention/{id}/counseling-referrals")]
    public static async Task<IResult> MakeCounselingReferral(string id, MakeCounselingReferralRequest request, HttpContext httpContext, IMessageBus bus, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var userId = httpContext.GetUserId();
        await bus.InvokeForTenantAsync(bus.TenantId!, new MakeCounselingReferralCommand(
            id, request.AgencyName, request.ReferralDate, userId), ct);
        return Results.NoContent();
    }

    [WolverinePost("/api/stewardship/foreclosure-prevention/{id}/counseling-referrals/{referralId}/outcome")]
    public static async Task<IResult> RecordCounselingOutcome(string id, Guid referralId, RecordCounselingOutcomeRequest request, HttpContext httpContext, IMessageBus bus, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var userId = httpContext.GetUserId();
        await bus.InvokeForTenantAsync(bus.TenantId!, new RecordCounselingOutcomeCommand(
            id, referralId, request.Outcome, request.OutcomeDate, userId), ct);
        return Results.NoContent();
    }

    [WolverinePost("/api/stewardship/foreclosure-prevention/{id}/forbearance-requests")]
    public static async Task<IResult> RequestForbearance(string id, RequestForbearanceRequest request, HttpContext httpContext, IMessageBus bus, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var userId = httpContext.GetUserId();
        await bus.InvokeForTenantAsync(bus.TenantId!, new RequestForbearanceCommand(
            id, request.LenderName, request.RequestedDate, userId), ct);
        return Results.NoContent();
    }

    [WolverinePost("/api/stewardship/foreclosure-prevention/{id}/forbearance-requests/{requestId}/outcome")]
    public static async Task<IResult> RecordForbearanceOutcome(string id, Guid requestId, RecordForbearanceOutcomeRequest request, HttpContext httpContext, IMessageBus bus, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var userId = httpContext.GetUserId();
        var outcome = ForbearanceOutcome.FromDisplayName(request.Outcome);
        await bus.InvokeForTenantAsync(bus.TenantId!, new RecordForbearanceOutcomeCommand(
            id, requestId, outcome, request.OutcomeDate, request.Notes, userId), ct);
        return Results.NoContent();
    }

    [WolverinePost("/api/stewardship/foreclosure-prevention/{id}/cure-period")]
    public static async Task<IResult> ActivateCurePeriod(string id, ActivateCurePeriodRequest request, HttpContext httpContext, IMessageBus bus, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var userId = httpContext.GetUserId();
        await bus.InvokeForTenantAsync(bus.TenantId!, new ActivateCurePeriodCommand(
            id, request.LraId, request.ActivationDate, request.CurePeriodDays, userId), ct);
        return Results.NoContent();
    }

    [WolverinePost("/api/stewardship/foreclosure-prevention/{id}/resolve")]
    public static async Task<IResult> ResolveCase(string id, ResolveForeclosurePreventionCaseRequest request, HttpContext httpContext, IMessageBus bus, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var userId = httpContext.GetUserId();
        var outcome = Enum.Parse<ForeclosurePreventionOutcome>(request.Outcome);
        await bus.InvokeForTenantAsync(bus.TenantId!, new ResolveForeclosurePreventionCaseCommand(
            id, outcome, request.ResolvedDate, userId), ct);
        return Results.NoContent();
    }
}

