using CommonGround.API.Contracts.Homeownership.Requests;
using CommonGround.API.Contracts.Homeownership.Responses;
using CommonGround.API.Infrastructure.Auth;
using CommonGround.Homeownership.Application.Commands;
using CommonGround.Homeownership.Application.Queries;
using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.HeirTransfers;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Infrastructure.API;
using Wolverine;
using Wolverine.Http;

namespace CommonGround.API.Homeownership;

/// <summary>The transfer of a deceased homeowner's home to an heir (Model Lease Art 10.3).</summary>
public static class HeirTransfersEndpoints
{
    [WolverinePost("/api/ground-leases/{groundLeaseId}/heir-transfers")]
    public static async Task<IResult> OpenHeirTransfer(
        string groundLeaseId,
        OpenHeirTransferRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        ArgumentNullException.ThrowIfNull(request);

        var id = HeirTransferId.Generate();

        await bus.InvokeForTenantAsync(bus.TenantId!,
            new OpenHeirTransfer(
                id,
                GroundLeaseId.Create(groundLeaseId),
                request.DiedOn,
                request.NoticeReceivedOn,
                request.NotifiedBy,
                request.NotifiedByContact,
                request.Notes,
                httpContext.GetUserId()),
            cancellationToken);

        return Results.Created($"/api/heir-transfers/{id.Value}", new OpenHeirTransferResponse(id.Value));
    }

    [WolverineGet("/api/ground-leases/{groundLeaseId}/heir-transfers")]
    public static async Task<IResult> GetHeirTransfers(string groundLeaseId, IMessageBus bus, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);

        var list = await bus.InvokeForTenantAsync<HeirTransferList>(bus.TenantId!,
            new GetHeirTransfers(GroundLeaseId.Create(groundLeaseId)), cancellationToken);

        return Results.Ok(list.Transfers.Select(ToResponse).ToList());
    }

    [WolverineGet("/api/heir-transfers/{heirTransferId}")]
    public static async Task<IResult> GetHeirTransfer(string heirTransferId, IMessageBus bus, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(heirTransferId);

        var view = await bus.InvokeForTenantAsync<HeirTransferView>(bus.TenantId!,
            new GetHeirTransfer(HeirTransferId.Create(heirTransferId)), cancellationToken);

        return Results.Ok(ToResponse(view));
    }

    [WolverineGet("/api/heir-transfers/{heirTransferId}/income-limit")]
    public static async Task<IResult> GetHeirIncomeLimit(string heirTransferId, IMessageBus bus, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(heirTransferId);

        var result = await bus.InvokeForTenantAsync<HeirIncomeLimitResult>(bus.TenantId!,
            new GetHeirIncomeLimit(HeirTransferId.Create(heirTransferId)), cancellationToken);

        if (result.Limit is not { } limit)
        {
            return Results.Ok(new HeirIncomeLimitResponse(null, null, []));
        }

        var level = limit.Schedule.LevelFor(limit.PercentOfAmi);

        return Results.Ok(new HeirIncomeLimitResponse(
            limit.PercentOfAmi,
            limit.Schedule.EffectiveFrom,
            level is null ? [] : [.. level.Limits.Select(l => l.Amount)]));
    }

    [WolverinePost("/api/heir-transfers/{heirTransferId}/heir")]
    public static async Task<IResult> RecordHeir(
        string heirTransferId,
        RecordHeirRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(heirTransferId);
        ArgumentNullException.ThrowIfNull(request);

        if (!HeirRelationship.TryFromDisplayName(request.Relationship, out var relationship))
        {
            return Results.BadRequest($"Unrecognized relationship: '{request.Relationship}'.");
        }

        await bus.InvokeForTenantAsync(bus.TenantId!,
            new RecordHeir(
                HeirTransferId.Create(heirTransferId),
                request.FullName,
                request.Email,
                request.Phone,
                relationship,
                request.MovedInOn,
                request.VerifiedAnnualIncome is { } income ? Money.Create(income) : null,
                request.VerifiedHouseholdSize,
                request.IncomeDocumentIds,
                httpContext.GetUserId()),
            cancellationToken);

        return Results.NoContent();
    }

    [WolverinePost("/api/heir-transfers/{heirTransferId}/complete")]
    public static async Task<IResult> CompleteHeirTransfer(
        string heirTransferId,
        CompleteHeirTransferRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(heirTransferId);
        ArgumentNullException.ThrowIfNull(request);

        await bus.InvokeForTenantAsync(bus.TenantId!,
            new CompleteHeirTransfer(
                HeirTransferId.Create(heirTransferId),
                request.TransferredOn,
                request.LetterOfAgreementDocumentId,
                request.AttorneyAcknowledgementDocumentId,
                request.Notes,
                httpContext.GetUserId()),
            cancellationToken);

        return Results.NoContent();
    }

    [WolverinePost("/api/heir-transfers/{heirTransferId}/close")]
    public static async Task<IResult> CloseHeirTransfer(
        string heirTransferId,
        CloseHeirTransferRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(heirTransferId);
        ArgumentNullException.ThrowIfNull(request);

        if (!HeirTransferCloseReason.TryFromDisplayName(request.Reason, out var reason))
        {
            return Results.BadRequest($"Unrecognized reason: '{request.Reason}'.");
        }

        await bus.InvokeForTenantAsync(bus.TenantId!,
            new CloseHeirTransfer(HeirTransferId.Create(heirTransferId), reason, request.Notes, httpContext.GetUserId()),
            cancellationToken);

        return Results.NoContent();
    }

    private static HeirTransferResponse ToResponse(HeirTransferView v) => new(
        v.Id,
        v.GroundLeaseId,
        v.HomeId,
        v.DeceasedHomeownerFullName,
        v.Status,
        v.DiedOn,
        v.NoticeReceivedOn,
        v.NoticeDueOn,
        v.NoticeWasLate,
        v.NotifiedBy,
        v.NotifiedByContact,
        v.Notes,
        v.HeirFullName,
        v.HeirEmail,
        v.HeirPhone,
        v.Relationship,
        v.HeirMovedInOn,
        v.HeirVerifiedAnnualIncome,
        v.HeirVerifiedHouseholdSize,
        v.IncomeDocumentIds,
        v.IncomeLimit,
        v.IncomeLimitPercentOfAmi,
        v.IncomeLimitsEffectiveFrom,
        v.TransferredOn,
        v.LetterOfAgreementDocumentId,
        v.AttorneyAcknowledgementDocumentId,
        v.CompletionNotes,
        v.ClosedReason,
        v.ClosedNotes,
        v.ClosedAt,
        v.OpenedAt);
}
