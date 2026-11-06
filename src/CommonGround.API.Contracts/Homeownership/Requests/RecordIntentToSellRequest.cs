namespace CommonGround.API.Contracts.Homeownership.Requests;

/// <summary>Request to record that a homeowner has given notice they intend to sell.</summary>
/// <param name="NoticeDate">The day the homeowner gave notice.</param>
/// <param name="Channel">How the notice reached the trust: Phone, Email, Letter or InPerson.</param>
/// <param name="Notes">Anything staff want to note about the notice.</param>
public sealed record RecordIntentToSellRequest(
    DateOnly NoticeDate,
    string Channel,
    string? Notes);
