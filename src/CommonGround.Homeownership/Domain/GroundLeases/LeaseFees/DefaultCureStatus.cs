using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;

/// <summary>Where a notice of default stands as of a day.</summary>
public sealed class DefaultCureStatus : Enumeration<DefaultCureStatus>
{
    /// <summary>The cure period is running and the amount owed isn't paid yet.</summary>
    public static readonly DefaultCureStatus Curing = new(1, "Curing");

    /// <summary>The amount owed was paid by the cure deadline.</summary>
    public static readonly DefaultCureStatus Cured = new(2, "Cured");

    /// <summary>The cure deadline passed unpaid, and the amount owed was paid since.</summary>
    public static readonly DefaultCureStatus PaidAfterDeadline = new(3, "PaidAfterDeadline");

    /// <summary>The cure deadline passed and the amount owed is still unpaid: the trust may end the lease (Art 12.4).</summary>
    public static readonly DefaultCureStatus Uncured = new(4, "Uncured");

    private DefaultCureStatus(int value, string displayName) : base(value, displayName) { }
}
