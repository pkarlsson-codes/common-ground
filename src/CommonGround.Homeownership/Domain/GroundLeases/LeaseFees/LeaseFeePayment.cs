using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;

/// <summary>
/// A lease fee payment the trust received on a lease. A payment recorded by mistake is voided, not removed, so the
/// history shows what happened; a voided payment no longer counts toward the balance.
/// </summary>
public sealed class LeaseFeePayment : Entity<LeaseFeePaymentId>
{
    private LeaseFeePayment(
        LeaseFeePaymentId id,
        DateOnly receivedDate,
        decimal amount,
        PaymentMethod method,
        string? reference)
    {
        Id = id;
        ReceivedDate = receivedDate;
        Amount = amount;
        Method = method;
        Reference = reference;
    }

    /// <summary>Creates a payment from its recorded facts. The lease checks the rules before deciding on it.</summary>
    /// <param name="id">The payment's ID.</param>
    /// <param name="receivedDate">The day the trust received it.</param>
    /// <param name="amount">The amount received.</param>
    /// <param name="method">How it was paid.</param>
    /// <param name="reference">A check number or similar, if given.</param>
    public static LeaseFeePayment Create(
        LeaseFeePaymentId id,
        DateOnly receivedDate,
        decimal amount,
        PaymentMethod method,
        string? reference)
    {
        ArgumentNullException.ThrowIfNull(method);

        return new LeaseFeePayment(id, receivedDate, amount, method, reference);
    }

    /// <summary>Gets the day the trust received the payment.</summary>
    public DateOnly ReceivedDate { get; }

    /// <summary>Gets the amount received.</summary>
    public decimal Amount { get; }

    /// <summary>Gets how it was paid.</summary>
    public PaymentMethod Method { get; }

    /// <summary>Gets a check number or similar, if given.</summary>
    public string? Reference { get; }

    /// <summary>Gets why the payment was voided, or null while it counts.</summary>
    public string? VoidReason { get; private set; }

    /// <summary>Gets whether the payment was voided.</summary>
    public bool IsVoided => VoidReason is not null;

    /// <summary>Marks the payment voided. Called only from the lease's Apply.</summary>
    internal void MarkVoided(string reason) => VoidReason = reason;
}
