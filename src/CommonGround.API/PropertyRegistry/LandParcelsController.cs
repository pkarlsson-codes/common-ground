using CommonGround.API.Contracts.PropertyRegistry.Requests;
using CommonGround.API.Contracts.PropertyRegistry.Responses;
using CommonGround.API.Infrastructure.Auth;
using CommonGround.PropertyRegistry.Application.Commands;
using CommonGround.PropertyRegistry.Application.Queries;
using CommonGround.PropertyRegistry.Application.ReadModels;
using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.SharedKernel.Infrastructure.API;
using CommonGround.SharedKernel.Infrastructure.Querying;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace CommonGround.API.PropertyRegistry;

/// <summary>API controller for land parcel management.</summary>
/// <param name="bus">The Wolverine message bus.</param>
[ApiController]
[Route("api/land-parcels")]
public sealed class LandParcelsController(IMessageBus bus) : ControllerBase
{
    /// <summary>
    /// Registers a new land parcel in the CLT portfolio.
    /// </summary>
    /// <param name="request">The request containing land parcel details.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The ID of the newly registered land parcel.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(RegisterLandParcelResponse), 201)]
    [EndpointName("registerLandParcel")]
    public async Task<IActionResult> RegisterLandParcel(
        [FromBody] RegisterLandParcelRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var userId = HttpContext.GetUserId();
        var acquisitionMethod = AcquisitionMethod.FromDisplayName(request.AcquisitionMethod);

        var command = new RegisterLandParcel(
            request.LegalParcelNumber,
            request.Address,
            request.AreaAcres,
            request.AcquisitionDate,
            acquisitionMethod,
            request.CovenantTerms,
            userId);

        var id = await bus.InvokeAsync<LandParcelId>(command, cancellationToken);

        return CreatedAtAction(nameof(GetLandParcel), new { landParcelId = id.Value }, new RegisterLandParcelResponse(id.Value));
    }

    /// <summary>
    /// Retrieves detailed information about a specific land parcel by its ID.
    /// </summary>
    /// <param name="landParcelId">The ID of the land parcel to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The detailed information of the land parcel.</returns>
    [HttpGet("{landParcelId}")]
    [ProducesResponseType(typeof(LandParcelResponse), 200)]
    [ProducesResponseType(404)]
    [EndpointName("getLandParcel")]
    public async Task<IActionResult> GetLandParcel(
        string landParcelId,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(landParcelId);

        var view = await bus.InvokeAsync<LandParcelView>(
            new GetLandParcel(LandParcelId.Create(landParcelId)),
            cancellationToken);

        var response = new LandParcelResponse(
            view.Id, view.LegalParcelNumber, view.Address, view.AreaAcres,
            view.AcquisitionDate, view.AcquisitionMethod, view.CovenantTerms,
            view.IsDecommissioned, view.RegisteredAt, view.RegisteredBy);

        return Ok(response);
    }

    /// <summary>Gets a paginated list of land parcels.</summary>
    /// <param name="request">The pagination and filter parameters.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paginated list of land parcels.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<LandParcelListItemResponse>), 200)]
    [EndpointName("getLandParcels")]
    public async Task<IActionResult> GetLandParcels(
        [FromQuery] PaginationQuerySpec<GetLandParcelsFilter> request,
        CancellationToken cancellationToken = default)
    {
        var query = new GetLandParcels(request.Page, request.Filter?.Search);

        var result = await bus
            .InvokeAsync<PaginatedResponse<LandParcelListItemView>>(
                query, cancellationToken);

        var response = result.MapItems(v => new LandParcelListItemResponse(
            v.Id, v.LegalParcelNumber, v.Address, v.AreaAcres, v.IsDecommissioned, v.RegisteredAt));

        return Ok(response);
    }

    /// <summary>Decommissions a land parcel.</summary>
    /// <param name="landParcelId">The ID of the land parcel to transition.</param>
    /// <param name="request">The transition request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content if successful.</returns>
    [HttpPost("{landParcelId}/decommission")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [EndpointName("decommissionLandParcel")]
    public async Task<IActionResult> DecommissionLandParcel(
        string landParcelId,
        [FromBody] DecommissionParcelRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(landParcelId);
        ArgumentNullException.ThrowIfNull(request);

        var userId = HttpContext.GetUserId();

        await bus.InvokeAsync(
            new DecommissionParcel(LandParcelId.Create(landParcelId), userId),
            cancellationToken).ConfigureAwait(false);

        return NoContent();
    }

    /// <summary>Recommissions a land parcel.</summary>
    /// <param name="landParcelId">The ID of the land parcel to transition.</param>
    /// <param name="request">The transition request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content if successful.</returns>
    [HttpPost("{landParcelId}/recommission")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [EndpointName("recommissionLandParcel")]
    public async Task<IActionResult> RecommissionLandParcel(
        string landParcelId,
        [FromBody] RecommissionParcelRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(landParcelId);
        ArgumentNullException.ThrowIfNull(request);

        var userId = HttpContext.GetUserId();

        await bus.InvokeAsync(
            new RecommissionParcel(LandParcelId.Create(landParcelId), userId),
            cancellationToken).ConfigureAwait(false);

        return NoContent();
    }
}
