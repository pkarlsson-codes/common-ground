using CommonGround.Homeownership.Application.Errors;
using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;

/// <summary>
/// How long a homeowner has to pay what a notice of default says is owed (Model Lease Art 12.1): the cure days, and
/// the extra days given when they pay at least <see cref="PartialPaymentShare"/> of it within the cure days. Each
/// trust writes its own into its lease; a notice keeps the period it was sent with.
/// </summary>
public sealed class DefaultCurePeriod : ValueObject
{
    /// <summary>The cure days when a trust hasn't set its own, as in Art 12.1.</summary>
    public const int DefaultCureDays = 30;

    /// <summary>The extension days when a trust hasn't set its own, as in Art 12.1.</summary>
    public const int DefaultExtensionDays = 30;

    /// <summary>The share of the amount owed that, paid within the cure days, extends them (Art 12.1: two-thirds).</summary>
    public const decimal PartialPaymentShare = 2m / 3m;

    private DefaultCurePeriod(int cureDays, int extensionDays)
    {
        CureDays = cureDays;
        ExtensionDays = extensionDays;
    }

    /// <summary>Gets the period a trust has until it sets its own: 30 days, extended by 30.</summary>
    public static DefaultCurePeriod Default { get; } = new(DefaultCureDays, DefaultExtensionDays);

    /// <summary>Creates a cure period. Refused for cure days below 1 or negative extension days.</summary>
    /// <param name="cureDays">The days after the notice is sent to pay the amount owed.</param>
    /// <param name="extensionDays">The days added when the partial-payment share was paid within the cure days; zero for none.</param>
    public static DefaultCurePeriod Create(int cureDays, int extensionDays)
    {
        if (cureDays < 1)
            throw BusinessRuleViolation.DefaultCurePeriod.Create.CureDaysTooFew(cureDays);

        if (extensionDays < 0)
            throw BusinessRuleViolation.DefaultCurePeriod.Create.ExtensionDaysNegative(extensionDays);

        return new DefaultCurePeriod(cureDays, extensionDays);
    }

    /// <summary>Gets the days after the notice is sent to pay the amount owed.</summary>
    public int CureDays { get; }

    /// <summary>Gets the days added when the partial-payment share was paid within the cure days.</summary>
    public int ExtensionDays { get; }

    /// <inheritdoc/>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return CureDays;
        yield return ExtensionDays;
    }
}
