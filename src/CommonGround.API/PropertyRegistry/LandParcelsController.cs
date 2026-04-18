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
public sealed class LandParcelsController(IMessageBus bus) : BaseApiController
{
    /// <summary>
    /// Registers a new land parcel in the CLT portfolio.
    /// </summary>
    /// <param name="request">The request containing land parcel details.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The ID of the newly registered land parcel.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<string>), 201)]
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

        return CreatedAtAction(nameof(GetLandParcel), new { landParcelId = id.Value }, id.Value);
    }

    /// <summary>
    /// Retrieves detailed information about a specific land parcel by its ID.
    /// </summary>
    /// <param name="landParcelId">The ID of the land parcel to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The detailed information of the land parcel.</returns>
    [HttpGet("{landParcelId}")]
    [ProducesResponseType(typeof(ApiResponse<LandParcelResponse>), 200)]
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
            view.Status, view.RegisteredAt, view.RegisteredBy);

        return Ok(response);
    }

    /// <summary>Gets a paginated list of land parcels.</summary>
    /// <param name="request">The pagination and filter parameters.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paginated list of land parcels.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<LandParcelListItemResponse>>), 200)]
    [EndpointName("getLandParcels")]
    public async Task<IActionResult> GetLandParcels(
        [FromQuery] PaginationQuerySpec<GetLandParcelsFilter> request,
        CancellationToken cancellationToken = default)
    {
        var statusFilter = request.Filter?.Statuses?
            .Select(ParcelStatus.FromDisplayName).ToList().AsReadOnly();
        var query = new GetLandParcels(request.Page, statusFilter);

        var result = await bus
            .InvokeAsync<PaginatedResponse<LandParcelListItemView>>(
                query, cancellationToken);

        var response = new PaginatedResponse<LandParcelListItemResponse>
        {
            Items = [..result.Items.Select(v => new LandParcelListItemResponse(
                v.Id, v.LegalParcelNumber, v.Address, v.AreaAcres, v.Status, v.RegisteredAt))],
            TotalCount = result.TotalCount,
            PageSize = result.PageSize,
            PageNumber = result.PageNumber,
        };

        return Ok(response);
    }

    /// <summary>Updates a land parcel's status.</summary>
    /// <param name="landParcelId">The ID of the land parcel to update.</param>
    /// <param name="request">The request containing the new status.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content if the update is successful.</returns>
    [HttpPatch("{landParcelId}/status")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [EndpointName("updateLandParcelStatus")]
    public async Task<IActionResult> UpdateLandParcelStatus(
        string landParcelId,
        [FromBody] UpdateLandParcelStatusRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(landParcelId);
        ArgumentNullException.ThrowIfNull(request);

        var userId = HttpContext.GetUserId();
        var newStatus = ParcelStatus.FromDisplayName(request.NewStatus);

        await bus.InvokeAsync(
            new UpdateLandParcelStatus(LandParcelId.Create(landParcelId), newStatus, userId),
            cancellationToken).ConfigureAwait(false);

        return NoContent();
    }
}