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

namespace CommonGround.API.Homeownership;

/// <summary>API controller for ground lease management.</summary>
/// <param name="bus">The Wolverine message bus.</param>
[ApiController]
[Route("api/ground-leases")]
public sealed class GroundLeasesController(IMessageBus bus) : ControllerBase
{
    /// <summary>Creates a new ground lease.</summary>
    /// <param name="request">The request containing ground lease details.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The ID of the newly created ground lease.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CreateGroundLeaseResponse), 201)]
    [EndpointName("createGroundLease")]
    public async Task<IActionResult> CreateGroundLease(
        [FromBody] CreateGroundLeaseRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var userId = HttpContext.GetUserId();

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

        var id = await bus.InvokeAsync<GroundLeaseId>(command, cancellationToken);

        return CreatedAtAction(nameof(GetGroundLease), new { groundLeaseId = id.Value }, new CreateGroundLeaseResponse(id.Value));
    }

    /// <summary>Gets a paginated list of ground leases.</summary>
    /// <param name="request">The pagination and filter parameters.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paginated list of ground leases.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<GroundLeaseListItemResponse>), 200)]
    [EndpointName("getGroundLeases")]
    public async Task<IActionResult> GetGroundLeases(
        [FromQuery] PaginationQuerySpec<GetGroundLeasesFilter> request,
        CancellationToken cancellationToken = default)
    {
        var statusFilter = request.Filter?.Statuses?
            .Select(s => Enum.Parse<GroundLeaseStatus>(s)).ToList().AsReadOnly();
        var landParcelFilter = request.Filter?.LandParcelId is not null
            ? LandParcelId.Create(request.Filter.LandParcelId)
            : null;
        var unitFilter = request.Filter?.HomeId is not null
            ? HomeId.Create(request.Filter.HomeId)
            : null;

        var query = new GetGroundLeases(request.Page, statusFilter, landParcelFilter, unitFilter, request.Filter?.Search);

        var result = await bus.InvokeAsync<PaginatedResponse<GroundLeaseListItemView>>(query, cancellationToken);

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

        return Ok(response);
    }

    /// <summary>Gets a ground lease by ID.</summary>
    /// <param name="groundLeaseId">The ID of the ground lease to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The detailed information of the ground lease.</returns>
    [HttpGet("{groundLeaseId}")]
    [ProducesResponseType(typeof(GroundLeaseResponse), 200)]
    [ProducesResponseType(404)]
    [EndpointName("getGroundLease")]
    public async Task<IActionResult> GetGroundLease(
        string groundLeaseId,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);

        var view = await bus.InvokeAsync<GroundLeaseView>(
            new GetGroundLease(GroundLeaseId.Create(groundLeaseId)),
            cancellationToken);

        var response = new GroundLeaseResponse(
            view.Id,
            view.LandParcelId,
            view.BuildingId,
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

        return Ok(response);
    }

    /// <summary>Changes the homeowner on a ground lease.</summary>
    /// <param name="groundLeaseId">The ID of the ground lease.</param>
    /// <param name="request">The request containing the new homeowner details.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content if successful.</returns>
    [HttpPatch("{groundLeaseId}/homeowner")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [EndpointName("changeHomeowner")]
    public async Task<IActionResult> ChangeHomeowner(
        string groundLeaseId,
        [FromBody] ChangeHomeownerRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        ArgumentNullException.ThrowIfNull(request);

        var userId = HttpContext.GetUserId();

        await bus.InvokeAsync(
            new ChangeHomeowner(
                GroundLeaseId.Create(groundLeaseId),
                new Homeowner(request.FullName, request.Email, request.Phone),
                userId),
            cancellationToken);

        return NoContent();
    }

    /// <summary>Records a stewardship check-in visit.</summary>
    /// <param name="groundLeaseId">The ID of the ground lease.</param>
    /// <param name="request">The request containing check-in details.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content if successful.</returns>
    [HttpPost("{groundLeaseId}/stewardship-check-ins")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [EndpointName("recordStewardshipCheckIn")]
    public async Task<IActionResult> RecordStewardshipCheckIn(
        string groundLeaseId,
        [FromBody] RecordStewardshipCheckInRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        ArgumentNullException.ThrowIfNull(request);

        var userId = HttpContext.GetUserId();

        await bus.InvokeAsync(
            new RecordStewardshipCheckIn(
                GroundLeaseId.Create(groundLeaseId),
                DateOnly.Parse(request.CheckInDate, CultureInfo.InvariantCulture),
                request.Notes,
                userId),
            cancellationToken);

        return NoContent();
    }

    /// <summary>Calculates a resale price estimate for a ground lease.</summary>
    /// <param name="groundLeaseId">The ID of the ground lease.</param>
    /// <param name="request">The request containing calculation inputs.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The calculated maximum resale price estimate.</returns>
    [HttpPost("{groundLeaseId}/resale-estimate")]
    [ProducesResponseType(typeof(ResaleEstimateResponse), 200)]
    [ProducesResponseType(404)]
    [EndpointName("calculateResaleEstimate")]
    public async Task<IActionResult> CalculateResaleEstimate(
        string groundLeaseId,
        [FromBody] CalculateResaleEstimateRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groundLeaseId);
        ArgumentNullException.ThrowIfNull(request);

        var view = await bus.InvokeAsync<GroundLeaseView>(
            new GetGroundLease(GroundLeaseId.Create(groundLeaseId)),
            cancellationToken);

        var terms = view.FormulaKind switch
        {
            "AppraisalBased" => ResalePolicyTerms.AppraisalBased(view.AppreciationShare!.Value, view.ImprovementsCap),
            "IndexBased" => ResalePolicyTerms.IndexBased(view.IndexType!, view.BaselineIndexValue!.Value, view.ImprovementsCap),
            "FixedPercentage" => ResalePolicyTerms.FixedPercentage(view.AnnualPercentage!.Value, view.ImprovementsCap),
            _ => throw new InvalidOperationException($"Unknown formula kind: {view.FormulaKind}")
        };

        var input = new ResaleCalculationInput(
            request.OriginalPurchasePrice,
            request.CurrentAppraisalValue,
            request.OriginalAppraisalValue,
            request.OwnershipYears,
            request.ApprovedImprovements,
            request.CurrentIndexValue);

        var maxResalePrice = ResalePriceCalculator.Calculate(terms, input);

        return Ok(new ResaleEstimateResponse(maxResalePrice));
    }
}
