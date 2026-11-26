using CommonGround.API.Contracts.HomeBuyers.Requests;
using CommonGround.API.Contracts.HomeBuyers.Responses;
using CommonGround.SharedKernel.Infrastructure.API;

#pragma warning disable SA1611
#pragma warning disable SA1615

namespace CommonGround.API.Clients;

/// <summary>Typed HTTP client for home selection endpoints.</summary>
public interface IHomeSelectionsApiClient
{
    /// <summary>Gets a home's waiting list compared against it today, and the households chosen for it.</summary>
    Task<HomeSelectionOverviewResponse> GetHomeSelectionOverviewAsync(string homeId, CancellationToken cancellationToken);

    /// <summary>Gets the households chosen for homes across every home, newest first.</summary>
    Task<PaginatedResponse<HomeSelectionListItemResponse>> GetHomeSelectionsAsync(GetHomeSelectionsQuerySpec querySpec, CancellationToken cancellationToken);

    /// <summary>Chooses a household for a home by priority and selects its application for the home.</summary>
    Task<HomeSelectionCreatedResponse> SelectHouseholdByPriorityAsync(string homeId, SelectHouseholdByPriorityRequest request, CancellationToken cancellationToken);

    /// <summary>Chooses a household for a home by lottery and selects its application for the home.</summary>
    Task<HomeSelectionCreatedResponse> DrawSelectionLotteryAsync(string homeId, DrawSelectionLotteryRequest request, CancellationToken cancellationToken);
}

#pragma warning restore SA1615
#pragma warning restore SA1611
