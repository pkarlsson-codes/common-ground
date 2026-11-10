namespace CommonGround.API.Contracts.Homeownership.Requests;

/// <summary>Request to void a lease fee payment recorded by mistake.</summary>
/// <param name="Reason">Why it is voided.</param>
public sealed record VoidLeaseFeePaymentRequest(string Reason);
