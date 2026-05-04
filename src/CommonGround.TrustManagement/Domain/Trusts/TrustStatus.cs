using CommonGround.SharedKernel.Domain;

namespace CommonGround.TrustManagement.Domain;

/// <summary>Represents the status of a trust.</summary>
public sealed class TrustStatus : Enumeration<TrustStatus>
{
    /// <summary>The trust is active and operational.</summary>
    public static readonly TrustStatus Active = new(1, nameof(Active));

    /// <summary>The trust is inactive and not operational.</summary>
    public static readonly TrustStatus Suspended = new(2, nameof(Suspended));

    private TrustStatus(int value, string name) : base(value, name)
    {
    }
}