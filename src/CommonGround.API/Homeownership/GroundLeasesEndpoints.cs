using CommonGround.API.Contracts.Homeownership.Requests;
using CommonGround.API.Contracts.Homeownership.Responses;
using CommonGround.API.Infrastructure.Auth;
using CommonGround.Homeownership.Application.Commands;
using CommonGround.Homeownership.Application.Queries;
using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;
using CommonGround.Homeownership.Domain.GroundLeases.Mortgages;
using CommonGround.Homeownership.Domain.GroundLeases.ResaleFormulas;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Infrastructure.API;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Wolverine.Http;

namespace CommonGround.API.Homeownership;

public static class GroundLeasesEndpoints
{
    [WolverinePost("/api/ground-leases")]
    public static async Task<IResult> CreateGroundLease(
        CreateGroundLeaseRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var userId = httpContext.GetUserId();

        var resalePolicyTerms = ResalePolicyTerms.FromString(
            request.FormulaKind,
            request.AppreciationShare,
            request.IndexType,
            request.BaselineIndexValue,
            request.AnnualPercentage,
            request.ImprovementsCap);

        var leaseFeeTracking = request.FeesTrackedFrom is { } trackedFrom
            ? LeaseFeeTracking.Create(trackedFrom, Money.Create(request.OpeningBalance ?? 0m), request.LeaseStartDate)
            : null;

        var id = GroundLeaseId.Generate();

        var command = new CreateGroundLease(
            id,
            request.LandParcelId,
            request.HomeId,
            Homeowner.From(request.HomeownerFullName, request.HomeownerEmail, request.HomeownerPhone),
            request.LeaseStartDate,
            request.TermYears,
            request.OptionWindowDays,
            LeaseFeeTerms.Create(Money.Create(request.LandUseFee), Money.Create(request.RepairReserveFee), request.LateInterestRate, request.InterestWaiverDays),
            MortgageDebtCap.Create(request.MaxMortgageDebtPercent),
            request.BasePrice,
            request.OriginalAppraisedValue,
            request.CovenantTerms,
            resalePolicyTerms,
            userId,
            leaseFeeTracking);

        await bus.InvokeForTenantAsync(bus.TenantId!, command, cancellationToken);

        return Results.Created($"/api/ground-leases/{id.Value}", new CreateGroundLeaseResponse(id.Value));
    }

    [WolverineGet("/api/ground-leases")]
    public static async Task<IResult> GetGroundLeases(
        [FromQuery] GetGroundLeasesQuerySpec request,
        IMessageBus bus,
        CancellationToken cancellationToken = default)
    {
        var statusFilter = request.Statuses?
            .Select(s => GroundLeaseStatus.FromDisplayName(s)).ToList().AsReadOnly();

        var query = new GetGroundLeases(request.Page, statusFilter, request.LandParcelId, request.HomeId, request.Search);

        var result = await bus.InvokeForTenantAsync<PaginatedResponse<GroundLeaseListItemView>>(bus.TenantId!, query, cancellationToken);

        var response = result.MapItems(v => new GroundLeaseListItemResponse(
            v.Id,
            v.LandParcelId,
            v.HomeId,
            v.LeaseStartDate,
            v.TermYears,
            v.Status,
            v.FormulaKind,
            v.HomeownerFullName,
            v.HomeownerEmail,
            v.CreatedAt,
            v.PurchaseWorkflowId));

        return Results.Ok(response);
    }

    [WolverineGet("/api/ground-leases/{groundLeaseId}")]
    public static async Task<IResult> GetGroundLease(
        string groundLeaseId,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);

        var (view, recordedOn, leaseFees, noticeDue, defaultCure) = await bus.InvokeForTenantAsync<GroundLeaseDetails>(bus.TenantId!,
            new GetGroundLease(GroundLeaseId.Create(groundLeaseId)),
            cancellationToken);

        var response = new GroundLeaseResponse(
            view.Id,
            view.GroundLeaseTemplateId,
            view.PurchaseWorkflowId,
            view.LandParcelId,
            view.HomeId,
            view.LeaseStartDate,
            view.TermYears,
            view.OptionWindowDays,
            view.BasePrice,
            view.OriginalAppraisedValue,
            view.CovenantTerms,
            view.Status,
            view.FormulaKind,
            view.AppreciationShare,
            view.IndexType,
            view.BaselineIndexValue,
            view.AnnualPercentage,
            view.ImprovementsCap,
            view.HomeownerFullName,
            view.HomeownerEmail,
            view.HomeownerPhone,
            view.StewardshipCheckIns.Select(c => new StewardshipCheckInDto(
                c.CheckInDate,
                c.Notes,
                c.RecordedBy,
                c.OccurredAt)).ToList().AsReadOnly(),
            new LeaseFeesDto(
                view.LandUseFee,
                view.RepairReserveFee,
                leaseFees.MonthlyFee.Amount,
                view.LateInterestRate,
                view.InterestWaiverDays,
                view.FeesTrackedFrom,
                view.OpeningBalance,
                new LeaseFeeStatementDto(
                    leaseFees.AsOf,
                    leaseFees.TotalCharged.Amount,
                    leaseFees.TotalPaid.Amount,
                    leaseFees.Balance.Amount,
                    leaseFees.OldestUnpaidDueDate,
                    leaseFees.DaysOverdue,
                    leaseFees.ArrearsSince,
                    leaseFees.InterestCharged.Amount,
                    leaseFees.InterestWaived.Amount,
                    leaseFees.InterestDue.Amount,
                    leaseFees.InterestAccruing.Amount,
                    leaseFees.InterestWaivedIfPaidBy),
                view.LeaseFeePayments
                    .OrderByDescending(p => p.ReceivedDate)
                    .ThenByDescending(p => p.RecordedAt)
                    .Select(p => new LeaseFeePaymentDto(
                        p.PaymentId,
                        p.ReceivedDate,
                        p.Amount,
                        p.Method,
                        p.Reference,
                        p.RecordedBy,
                        p.RecordedAt,
                        p.VoidReason,
                        p.VoidedAt))
                    .ToList()
                    .AsReadOnly(),
                noticeDue?.DisplayName,
                view.LeaseFeeNotices
                    .OrderByDescending(n => n.SentOn)
                    .ThenByDescending(n => n.RecordedAt)
                    .Select(n => new LeaseFeeNoticeDto(
                        n.NoticeId,
                        n.Level,
                        n.SentOn,
                        n.Delivery,
                        n.BalanceDue,
                        n.OldestUnpaidDueDate,
                        n.RecordedBy,
                        n.RecordedAt,
                        n.CureDays,
                        n.CureExtensionDays))
                    .ToList()
                    .AsReadOnly(),
                view.LeaseFeeInterestWaivers
                    .AsEnumerable()
                    .Reverse()
                    .Select(w => new LeaseFeeInterestWaiverDto(w.WaiverId, w.WaivedOn, w.Amount, w.Reason, w.WaivedBy))
                    .ToList()
                    .AsReadOnly(),
                LeaseFeesEndpoints.ToResponse(defaultCure)),
            new MortgagesDto(
                view.MaxMortgageDebtPercent,
                view.PermittedMortgages
                    .OrderBy(m => m.PaidOffOn is not null)
                    .ThenByDescending(m => m.PermittedOn)
                    .Select(m => new PermittedMortgageDto(
                        m.MortgageId, m.Lender, m.NoticeAddress, m.OriginalPrincipal, m.PermittedOn, m.Source, m.PaidOffOn))
                    .ToList()
                    .AsReadOnly()),
            view.CreatedAt,
            recordedOn,
            view.CreatedBy);

        return Results.Ok(response);
    }

    [WolverinePost("/api/ground-leases/{groundLeaseId}/stewardship-check-ins")]
    public static async Task<IResult> RecordStewardshipCheckIn(
        string groundLeaseId,
        RecordStewardshipCheckInRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        ArgumentNullException.ThrowIfNull(request);

        var userId = httpContext.GetUserId();

        await bus.InvokeForTenantAsync(bus.TenantId!,
            new RecordStewardshipCheckIn(
                GroundLeaseId.Create(groundLeaseId),
                request.CheckInDate,
                request.Notes,
                userId),
            cancellationToken);

        return Results.NoContent();
    }

    [WolverinePost("/api/ground-leases/{groundLeaseId}/resale-estimate")]
    public static async Task<IResult> CalculateResaleEstimate(
        string groundLeaseId,
        CalculateResaleEstimateRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        ArgumentNullException.ThrowIfNull(request);

        var query = new GetResaleEstimate(
            GroundLeaseId.Create(groundLeaseId),
            request.OriginalPurchasePrice,
            request.CurrentAppraisalValue,
            request.OriginalAppraisalValue,
            request.OwnershipYears,
            request.ApprovedImprovements,
            request.CurrentIndexValue);

        var maxResalePrice = await bus.InvokeForTenantAsync<decimal>(bus.TenantId!, query, cancellationToken);

        return Results.Ok(new ResaleEstimateResponse(maxResalePrice));
    }
}
