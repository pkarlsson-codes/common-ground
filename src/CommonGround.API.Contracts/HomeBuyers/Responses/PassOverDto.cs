namespace CommonGround.API.Contracts.HomeBuyers.Responses;

/// <summary>
/// A home selection the household was passed over in. <see cref="SelectionMethod"/> is how the
/// chosen household was picked: "Priority" or "Lottery".
/// </summary>
public sealed record PassOverDto(
    string ApplicationId,
    string SelectionId,
    string HomeId,
    DateTimeOffset PassedOverAt,
    string? SelectionMethod);
