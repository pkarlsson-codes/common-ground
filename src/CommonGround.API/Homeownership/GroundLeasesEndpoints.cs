using System.Globalization;
using CommonGround.API.Contracts.Homeownership.Requests;
using CommonGround.API.Contracts.Homeownership.Responses;
using CommonGround.API.Infrastructure.Auth;
using CommonGround.Homeownership.Application.Commands;
using CommonGround.Homeownership.Application.Queries;
using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.GroundLeases.ResaleFormulas;
using CommonGround.PropertyRegistry.Domain.Homes;
using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.SharedKernel.Infrastructure.API;
using CommonGround.SharedKernel.Infrastructure.Querying;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Wolverine.Http;

#pragma warning disable SA1600
#pragma warning disable CA2007

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

        var resalePolicyTerms = request.FormulaKind switch
        {
            "AppraisalBased" => ResalePolicyTerms.AppraisalBased(request.AppreciationShare!.Value, request.ImprovementsCap),
            "IndexBased" => ResalePolicyTerms.IndexBased(request.IndexType!, request.BaselineIndexValue!.Value, request.ImprovementsCap),
            "FixedPercentage" => ResalePolicyTerms.FixedPercentage(request.AnnualPercentage!.Value, request.ImprovementsCap),
            _ => throw new ArgumentException($"Unknown formula kind: {request.FormulaKind}")
        };

        var command = new CreateGroundLease(
            LandParcelId.Create(request.LandParcelId),
            HomeId.Create(request.HomeId),
            DateOnly.Parse(request.LeaseStartDate, CultureInfo.InvariantCulture),
            request.TermYears,
            request.CovenantTerms,
            resalePolicyTerms,
            userId);

        var id = await bus.InvokeForTenantAsync<GroundLeaseId>(bus.TenantId!, command, cancellationToken);

        return Results.Created($"/api/ground-leases/{id.Value}", new CreateGroundLeaseResponse(id.Value));
    }

    [WolverineGet("/api/ground-leases")]
    public static async Task<IResult> GetGroundLeases(
        [FromQuery] GetGroundLeasesQuerySpec request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken = default)
    {
        var statusFilter = request.Statuses?
            .Select(s => Enum.Parse<GroundLeaseStatus>(s)).ToList().AsReadOnly();
        var landParcelFilter = request.LandParcelId is not null
            ? LandParcelId.Create(request.LandParcelId)
            : null;
        var unitFilter = request.HomeId is not null
            ? HomeId.Create(request.HomeId)
            : null;

        var query = new GetGroundLeases(request.Page, statusFilter, landParcelFilter, unitFilter, request.Search);

        var result = await bus.InvokeForTenantAsync<PaginatedResponse<GroundLeaseListItemView>>(bus.TenantId!, query, cancellationToken);

        var response = result.MapItems(v => new GroundLeaseListItemResponse(
            v.Id,
            v.LandParcelId,
            v.HomeId,
            v.LeaseStartDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            v.TermYears,
            v.Status,
            v.HomeownerFullName,
            v.HomeownerEmail,
            v.CreatedAt.ToString("O", CultureInfo.InvariantCulture)));

        return Results.Ok(response);
    }

    [WolverineGet("/api/ground-leases/{groundLeaseId}")]
    public static async Task<IResult> GetGroundLease(
        string groundLeaseId,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);

        var view = await bus.InvokeForTenantAsync<GroundLeaseView>(bus.TenantId!,
            new GetGroundLease(GroundLeaseId.Create(groundLeaseId)),
            cancellationToken);

        var response = new GroundLeaseResponse(
            view.Id,
            view.LandParcelId,
            view.HomeId,
            view.LeaseStartDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            view.TermYears,
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
                c.CheckInDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                c.Notes,
                c.RecordedBy,
                c.OccurredAt.ToString("O", CultureInfo.InvariantCulture))).ToList().AsReadOnly(),
            view.CreatedAt.ToString("O", CultureInfo.InvariantCulture),
            view.CreatedBy);

        return Results.Ok(response);
    }

    [WolverinePatch("/api/ground-leases/{groundLeaseId}/homeowner")]
    public static async Task<IResult> ChangeHomeowner(
        string groundLeaseId,
        ChangeHomeownerRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        ArgumentNullException.ThrowIfNull(request);

        var userId = httpContext.GetUserId();

        await bus.InvokeForTenantAsync(bus.TenantId!,
            new ChangeHomeowner(
                GroundLeaseId.Create(groundLeaseId),
                new Homeowner(request.FullName, request.Email, request.Phone),
                userId),
            cancellationToken);

        return Results.NoContent();
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
                DateOnly.Parse(request.CheckInDate, CultureInfo.InvariantCulture),
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

#pragma warning restore CA2007
#pragma warning restore SA1600