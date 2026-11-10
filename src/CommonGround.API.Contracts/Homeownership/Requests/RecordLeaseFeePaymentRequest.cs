namespace CommonGround.API.Contracts.Homeownership.Requests;

/// <summary>Request to record a lease fee payment the trust received.</summary>
/// <param name="ReceivedDate">The day the trust received it.</param>
/// <param name="Amount">The amount received.</param>
/// <param name="Method">How it was paid: Check, BankTransfer, Cash, MoneyOrder, Card or Other.</param>
/// <param name="Reference">A check number or similar, if given.</param>
public sealed record RecordLeaseFeePaymentRequest(
    DateOnly ReceivedDate,
    decimal Amount,
    string Method,
    string? Reference);
