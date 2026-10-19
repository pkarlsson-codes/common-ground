using CommonGround.API.Contracts.HomeBuyers.Requests;
using CommonGround.API.Contracts.HomeBuyers.Responses;
using CommonGround.SharedKernel.Infrastructure.API;

#pragma warning disable SA1611
#pragma warning disable SA1615

namespace CommonGround.API.Clients;

/// <summary>Typed HTTP client for household endpoints.</summary>
public interface IHouseholdsApiClient
{
    /// <summary>Registers a new household.</summary>
    Task<RegisterHouseholdResponse> RegisterHouseholdAsync(RegisterHouseholdRequest request, CancellationToken cancellationToken);

    /// <summary>Gets a paginated list of households.</summary>
    Task<PaginatedResponse<HouseholdListItemResponse>> GetHouseholdsAsync(GetHouseholdsQuerySpec querySpec, CancellationToken cancellationToken);

    /// <summary>Gets a household by ID.</summary>
    Task<HouseholdResponse> GetHouseholdAsync(string householdId, CancellationToken cancellationToken);

    /// <summary>Adds a non-primary member to a household.</summary>
    Task<AddHouseholdMemberResponse> AddHouseholdMemberAsync(string householdId, AddHouseholdMemberRequest request, CancellationToken cancellationToken);

    /// <summary>Replaces a household member's contact details.</summary>
    Task UpdateHouseholdMemberAsync(string householdId, string memberId, UpdateHouseholdMemberRequest request, CancellationToken cancellationToken);

    /// <summary>Removes a non-primary member from a household.</summary>
    Task RemoveHouseholdMemberAsync(string householdId, string memberId, CancellationToken cancellationToken);

    /// <summary>Replaces a household's circumstances.</summary>
    Task UpdateHouseholdCircumstancesAsync(string householdId, UpdateHouseholdCircumstancesRequest request, CancellationToken cancellationToken);

    /// <summary>Records housing counseling completion for a household.</summary>
    Task RecordHousingCounselingAsync(string householdId, RecordHousingCounselingRequest request, CancellationToken cancellationToken);

    /// <summary>Records CLT orientation completion for a household.</summary>
    Task RecordCltOrientationAsync(string householdId, RecordCltOrientationRequest request, CancellationToken cancellationToken);

    /// <summary>Records mortgage prequalification for a household.</summary>
    Task RecordMortgagePrequalificationAsync(string householdId, RecordMortgagePrequalificationRequest request, CancellationToken cancellationToken);
}

#pragma warning restore SA1615
#pragma warning restore SA1611
