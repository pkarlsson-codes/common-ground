using CommonGround.API.Contracts.Homeownership.Requests;
using CommonGround.API.Contracts.Homeownership.Responses;
using CommonGround.API.Infrastructure.Auth;
using CommonGround.Homeownership.Application.Commands;
using CommonGround.Homeownership.Application.Queries;
using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.GroundLeases.Mortgages;
using CommonGround.Homeownership.Domain.MortgageRequests;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Infrastructure.API;
using Wolverine;
using Wolverine.Http;

namespace CommonGround.API.Homeownership;

/// <summary>Homeowners' requests to put a new loan on the home, and the trust's decisions (Model Lease Art 8.3).</summary>
public static class MortgageRequestsEndpoints
{
    [WolverinePost("/api/ground-leases/{groundLeaseId}/mortgage-requests")]
    public static async Task<IResult> ReceiveMortgageRequest(
        string groundLeaseId,
        ReceiveMortgageRequestRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        ArgumentNullException.ThrowIfNull(request);

        if (!MortgagePurpose.TryFromDisplayName(request.Purpose, out var purpose))
        {
            return Results.BadRequest($"Unrecognized loan purpose: '{request.Purpose}'.");
        }

        if (!MortgageRateType.TryFromDisplayName(request.RateType, out var rateType))
        {
            return Results.BadRequest($"Unrecognized rate type: '{request.RateType}'.");
        }

        if (request.AppraisedValue is null != request.AppraisalDate is null)
        {
            return Results.BadRequest("Send both the appraised value and the appraisal date, or neither.");
        }

        var appraisal = request.AppraisedValue is { } value && request.AppraisalDate is { } date
            ? LoanAppraisal.Create(Money.Create(value), date, request.CurrentIndexValue)
            : null;
        var id = MortgageRequestId.Generate();

        await bus.InvokeForTenantAsync(bus.TenantId!,
            new ReceiveMortgageRequest(
                id,
                GroundLeaseId.Create(groundLeaseId),
                request.ReceivedOn,
                MortgageLoanTerms.Create(
                    request.Lender,
                    purpose,
                    Money.Create(request.Principal),
                    Money.Create(request.TotalDebtAfterClosing),
                    request.InterestRatePercent,
                    rateType,
                    request.TermYears),
                request.ExpectedClosingOn,
                request.ClosingCosts is { } costs ? Money.Create(costs) : null,
                appraisal,
                request.Notes,
                httpContext.GetUserId()),
            cancellationToken);

        return Results.Created($"/api/mortgage-requests/{id.Value}", new ReceiveMortgageRequestResponse(id.Value));
    }

    [WolverineGet("/api/ground-leases/{groundLeaseId}/mortgage-requests")]
    public static async Task<IResult> GetMortgageRequests(string groundLeaseId, IMessageBus bus, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);

        var list = await bus.InvokeForTenantAsync<MortgageRequestList>(bus.TenantId!,
            new GetMortgageRequests(GroundLeaseId.Create(groundLeaseId)), cancellationToken);

        return Results.Ok(list.Requests.Select(ToResponse).ToList());
    }

    [WolverineGet("/api/mortgage-requests/{mortgageRequestId}")]
    public static async Task<IResult> GetMortgageRequest(string mortgageRequestId, IMessageBus bus, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mortgageRequestId);

        var view = await bus.InvokeForTenantAsync<MortgageRequestView>(bus.TenantId!,
            new GetMortgageRequest(MortgageRequestId.Create(mortgageRequestId)), cancellationToken);

        return Results.Ok(ToResponse(view));
    }

    [WolverinePost("/api/mortgage-requests/{mortgageRequestId}/appraisal")]
    public static async Task<IResult> RecordMortgageRequestAppraisal(
        string mortgageRequestId,
        RecordMortgageRequestAppraisalRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mortgageRequestId);
        ArgumentNullException.ThrowIfNull(request);

        await bus.InvokeForTenantAsync(bus.TenantId!,
            new RecordMortgageRequestAppraisal(
                MortgageRequestId.Create(mortgageRequestId),
                LoanAppraisal.Create(Money.Create(request.AppraisedValue), request.AppraisalDate, request.CurrentIndexValue),
                request.ClosingCosts is { } costs ? Money.Create(costs) : null,
                httpContext.GetUserId()),
            cancellationToken);

        return Results.NoContent();
    }

    [WolverinePost("/api/mortgage-requests/{mortgageRequestId}/approve")]
    public static async Task<IResult> ApproveMortgageRequest(
        string mortgageRequestId,
        ApproveMortgageRequestRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mortgageRequestId);
        ArgumentNullException.ThrowIfNull(request);

        await bus.InvokeForTenantAsync(bus.TenantId!,
            new ApproveMortgageRequest(MortgageRequestId.Create(mortgageRequestId), request.ApprovedOn, request.Reason, httpContext.GetUserId()),
            cancellationToken);

        return Results.NoContent();
    }

    [WolverinePost("/api/mortgage-requests/{mortgageRequestId}/deny")]
    public static async Task<IResult> DenyMortgageRequest(
        string mortgageRequestId,
        DenyMortgageRequestRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mortgageRequestId);
        ArgumentNullException.ThrowIfNull(request);

        await bus.InvokeForTenantAsync(bus.TenantId!,
            new DenyMortgageRequest(MortgageRequestId.Create(mortgageRequestId), request.DeniedOn, request.Reason, httpContext.GetUserId()),
            cancellationToken);

        return Results.NoContent();
    }

    [WolverinePost("/api/mortgage-requests/{mortgageRequestId}/loan-closed")]
    public static async Task<IResult> RecordMortgageLoanClosed(
        string mortgageRequestId,
        RecordMortgageLoanClosedRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mortgageRequestId);
        ArgumentNullException.ThrowIfNull(request);

        var mortgageId = PermittedMortgageId.Generate();

        await bus.InvokeForTenantAsync(bus.TenantId!,
            new RecordMortgageLoanClosed(
                MortgageRequestId.Create(mortgageRequestId),
                mortgageId,
                request.ClosedOn,
                [.. request.PaidOffMortgageIds.Select(PermittedMortgageId.Create)],
                request.NoticeAddress,
                httpContext.GetUserId()),
            cancellationToken);

        return Results.Ok(new RecordMortgageLoanClosedResponse(mortgageId.Value));
    }

    [WolverinePost("/api/mortgage-requests/{mortgageRequestId}/withdraw")]
    public static async Task<IResult> WithdrawMortgageRequest(
        string mortgageRequestId,
        WithdrawMortgageRequestRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mortgageRequestId);
        ArgumentNullException.ThrowIfNull(request);

        await bus.InvokeForTenantAsync(bus.TenantId!,
            new WithdrawMortgageRequest(MortgageRequestId.Create(mortgageRequestId), request.WithdrawnOn, request.Reason, httpContext.GetUserId()),
            cancellationToken);

        return Results.NoContent();
    }

    private static MortgageRequestResponse ToResponse(MortgageRequestView v) => new(
        v.Id,
        v.GroundLeaseId,
        v.HomeId,
        v.HomeownerFullName,
        v.Status,
        v.ReceivedOn,
        v.Lender,
        v.Purpose,
        v.Principal,
        v.TotalDebtAfterClosing,
        v.InterestRatePercent,
        v.RateType,
        v.TermYears,
        v.ExpectedClosingOn,
        v.BusinessDaysBeforeClosing,
        v.BusinessDaysBeforeClosing < MortgageRequest.RequiredNoticeBusinessDays,
        v.ClosingCosts,
        v.Notes,
        v.AppraisedValue,
        v.AppraisalDate,
        v.CurrentIndexValue,
        v.PurchaseOptionPrice,
        v.MaxMortgageDebtPercent,
        v.DebtCapAmount,
        v.DebtPercent,
        v.DebtCapAmount is { } cap ? v.TotalDebtAfterClosing > cap : null,
        v.DecidedOn,
        v.DecisionReason,
        v.ClosedOn,
        v.PermittedMortgageId,
        v.PaidOffMortgageIds,
        v.WithdrawnOn,
        v.WithdrawnReason,
        v.RecordedAt);
}
