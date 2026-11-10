using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;

/// <summary>How a lease fee payment reached the trust.</summary>
public sealed class PaymentMethod : Enumeration<PaymentMethod>
{
    /// <summary>A paper check.</summary>
    public static readonly PaymentMethod Check = new(1, "Check");

    /// <summary>A bank transfer, ACH or standing order.</summary>
    public static readonly PaymentMethod BankTransfer = new(2, "BankTransfer");

    /// <summary>Cash handed to trust staff.</summary>
    public static readonly PaymentMethod Cash = new(3, "Cash");

    /// <summary>A money order.</summary>
    public static readonly PaymentMethod MoneyOrder = new(4, "MoneyOrder");

    /// <summary>A debit or credit card.</summary>
    public static readonly PaymentMethod Card = new(5, "Card");

    /// <summary>Any other way.</summary>
    public static readonly PaymentMethod Other = new(6, "Other");

    private PaymentMethod(int value, string displayName) : base(value, displayName) { }
}
