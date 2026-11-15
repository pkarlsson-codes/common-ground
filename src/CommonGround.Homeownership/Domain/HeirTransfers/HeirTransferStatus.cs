using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Domain.HeirTransfers;

/// <summary>Where a transfer of the home to a deceased homeowner's heir stands.</summary>
public sealed class HeirTransferStatus : Enumeration<HeirTransferStatus>
{
    /// <summary>The executor's notice is recorded; no heir yet.</summary>
    public static readonly HeirTransferStatus Open = new(1, "Open");

    /// <summary>An heir is recorded and qualifies; the transfer isn't completed.</summary>
    public static readonly HeirTransferStatus HeirRecorded = new(2, "Heir recorded");

    /// <summary>The lease names the heir as homeowner.</summary>
    public static readonly HeirTransferStatus Completed = new(3, "Completed");

    /// <summary>The home did not pass to the heir; the lease is unchanged.</summary>
    public static readonly HeirTransferStatus Closed = new(4, "Closed");

    private HeirTransferStatus(int value, string displayName) : base(value, displayName) { }
}
