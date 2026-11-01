using CommonGround.API.Contracts.HomeBuyers.Requests;
using CommonGround.API.Contracts.HomeBuyers.Responses;
using CommonGround.SharedKernel.Infrastructure.API;

#pragma warning disable SA1611
#pragma warning disable SA1615

namespace CommonGround.API.Clients;

/// <summary>Typed HTTP client for home selection endpoints.</summary>
public interface IHomeSelectionsApiClient
{
    /// <summary>Starts a home selection from the current waiting list.</summary>
    Task<StartHomeSelectionResponse> StartHomeSelectionAsync(string homeId, CancellationToken cancellationToken);

    /// <summary>Gets every selection started for a home, newest first.</summary>
    Task<IReadOnlyList<HomeSelectionSummaryResponse>> GetHomeSelectionsForHomeAsync(string homeId, CancellationToken cancellationToken);

    /// <summary>Gets home selections across every home, newest first, optionally filtered by status.</summary>
    Task<PaginatedResponse<HomeSelectionListItemResponse>> GetHomeSelectionsAsync(GetHomeSelectionsQuerySpec querySpec, CancellationToken cancellationToken);

    /// <summary>Gets a home selection with its candidates' priority facts.</summary>
    Task<HomeSelectionResponse> GetHomeSelectionAsync(string selectionId, CancellationToken cancellationToken);

    /// <summary>Chooses a candidate by priority and selects their application for the home.</summary>
    Task SelectHouseholdByPriorityAsync(string selectionId, SelectHouseholdByPriorityRequest request, CancellationToken cancellationToken);

    /// <summary>Chooses a candidate by lottery and selects their application for the home.</summary>
    Task DrawSelectionLotteryAsync(string selectionId, DrawSelectionLotteryRequest request, CancellationToken cancellationToken);

    /// <summary>Cancels an open home selection without choosing anyone.</summary>
    Task CancelHomeSelectionAsync(string selectionId, CancelHomeSelectionRequest request, CancellationToken cancellationToken);
}

#pragma warning restore SA1615
#pragma warning restore SA1611
