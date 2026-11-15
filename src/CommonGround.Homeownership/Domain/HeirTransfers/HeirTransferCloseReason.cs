using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Domain.HeirTransfers;

/// <summary>Why a transfer closed without the home passing to the heir.</summary>
public sealed class HeirTransferCloseReason : Enumeration<HeirTransferCloseReason>
{
    /// <summary>The heir isn't income-qualified.</summary>
    public static readonly HeirTransferCloseReason HeirNotQualified = new(1, "HeirNotQualified");

    /// <summary>The heir doesn't want the home.</summary>
    public static readonly HeirTransferCloseReason HeirDeclined = new(2, "HeirDeclined");

    /// <summary>Another reason, given in the notes.</summary>
    public static readonly HeirTransferCloseReason Other = new(3, "Other");

    private HeirTransferCloseReason(int value, string displayName) : base(value, displayName) { }
}
