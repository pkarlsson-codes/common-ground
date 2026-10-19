using CommonGround.API.Contracts.ApplicantSelection.Requests;
using CommonGround.API.Contracts.ApplicantSelection.Responses;

#pragma warning disable SA1611
#pragma warning disable SA1615

namespace CommonGround.API.Clients;

/// <summary>Typed HTTP client for household endpoints.</summary>
public interface IHouseholdsApiClient
{
    /// <summary>Registers a new household.</summary>
    Task<RegisterHouseholdResponse> RegisterHouseholdAsync(RegisterHouseholdRequest request, CancellationToken cancellationToken);

    /// <summary>Gets a household by ID.</summary>
    Task<HouseholdResponse> GetHouseholdAsync(string householdId, CancellationToken cancellationToken);

    /// <summary>Records housing counseling completion for a household.</summary>
    Task RecordHousingCounselingAsync(string householdId, RecordHousingCounselingRequest request, CancellationToken cancellationToken);

    /// <summary>Records CLT orientation completion for a household.</summary>
    Task RecordCltOrientationAsync(string householdId, RecordCltOrientationRequest request, CancellationToken cancellationToken);

    /// <summary>Records mortgage prequalification for a household.</summary>
    Task RecordMortgagePrequalificationAsync(string householdId, RecordMortgagePrequalificationRequest request, CancellationToken cancellationToken);
}

#pragma warning restore SA1615
#pragma warning restore SA1611
