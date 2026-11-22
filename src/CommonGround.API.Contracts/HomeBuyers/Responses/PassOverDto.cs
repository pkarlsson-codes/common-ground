namespace CommonGround.API.Contracts.HomeBuyers.Responses;

/// <summary>
/// A home selection the household was passed over in. <see cref="SelectionMethod"/> is how the
/// chosen household was picked: "Priority" or "Lottery". <see cref="SelectionReason"/> is staff's
/// explanation of a priority choice; null for a lottery or a selection that recorded none.
/// </summary>
public sealed record PassOverDto(
    string ApplicationId,
    string SelectionId,
    string HomeId,
    string? HomeLabel,
    DateTimeOffset PassedOverAt,
    string? SelectionMethod,
    string? SelectionReason);
