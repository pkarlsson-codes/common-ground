namespace CommonGround.Homeownership.Domain.PurchaseWorkflows.TrackDetails;

/// <summary>Details recorded when title is cleared.</summary>
/// <param name="ClearedDate">Date the title was cleared.</param>
/// <param name="TitleCompanyName">Optional name of the title company.</param>
public sealed record TitleDetails(DateOnly ClearedDate, string? TitleCompanyName);
