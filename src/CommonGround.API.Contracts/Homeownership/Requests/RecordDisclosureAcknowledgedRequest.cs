using System.Text.Json.Serialization;

namespace CommonGround.API.Contracts.Homeownership.Requests;

/// <summary>Request to record the buyer's signed disclosure acknowledgment.</summary>
/// <param name="AcknowledgedDate">The date the buyer signed. Not in the future.</param>
/// <param name="DocumentId">The signed acknowledgment in Documents.</param>
/// <param name="Notes">Optional notes.</param>
public sealed record RecordDisclosureAcknowledgedRequest(
    DateOnly AcknowledgedDate,
    [property: JsonRequired] string DocumentId,
    string? Notes);
