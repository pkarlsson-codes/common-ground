namespace CommonGround.API.Contracts.Homeownership.Responses;

/// <summary>Response after recording a lease fee payment.</summary>
/// <param name="PaymentId">The new payment's ID.</param>
public sealed record RecordLeaseFeePaymentResponse(string PaymentId);
