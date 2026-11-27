namespace CommonGround.API.Contracts.HomeBuyers.Requests;

/// <summary>Request to confirm that an approved household still qualifies on its changed income or size.</summary>
public sealed record ConfirmQualificationRequest(DateOnly ConfirmedDate);
