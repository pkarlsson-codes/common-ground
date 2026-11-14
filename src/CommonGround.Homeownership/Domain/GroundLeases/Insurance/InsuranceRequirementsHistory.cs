using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Domain.GroundLeases.Insurance;

/// <summary>
/// The insurance requirements a lease has had: the ones it started with, then each change in effective-date order.
/// On a day, the last change effective on or before it applies; before the first, the first applies.
/// </summary>
public sealed class InsuranceRequirementsHistory : ValueObject
{
    private readonly InsuranceRequirementsChange[] _changes;

    private InsuranceRequirementsHistory(InsuranceRequirementsChange[] changes)
    {
        _changes = changes;
    }

    /// <summary>Starts a history with the requirements a lease starts with.</summary>
    public static InsuranceRequirementsHistory Start(DateOnly leaseStartDate, InsuranceRequirements requirements) =>
        new([InsuranceRequirementsChange.Create(leaseStartDate, requirements)]);

    /// <summary>
    /// Rebuilds a history from its changes, first one first. Throws if there are none, or one is effective before the
    /// change before it.
    /// </summary>
    public static InsuranceRequirementsHistory From(IEnumerable<InsuranceRequirementsChange> changes)
    {
        ArgumentNullException.ThrowIfNull(changes);

        var list = changes.ToArray();
        if (list.Length == 0)
            throw new ArgumentException("A history needs at least the requirements the lease started with.", nameof(changes));

        for (var i = 1; i < list.Length; i++)
        {
            if (list[i].EffectiveFrom < list[i - 1].EffectiveFrom)
                throw new ArgumentException("Changes must be in effective-date order.", nameof(changes));
        }

        return new InsuranceRequirementsHistory(list);
    }

    /// <summary>Gets every change, first one first.</summary>
    public IReadOnlyList<InsuranceRequirementsChange> Changes => _changes;

    /// <summary>Gets the day the latest change takes effect.</summary>
    public DateOnly LatestEffectiveFrom => _changes[^1].EffectiveFrom;

    /// <summary>Gets the requirements that apply on a day.</summary>
    public InsuranceRequirements InEffectOn(DateOnly date) =>
        (_changes.LastOrDefault(c => c.EffectiveFrom <= date) ?? _changes[0]).Requirements;

    /// <summary>Adds a change effective on or after the latest one. Throws for one effective before it.</summary>
    public InsuranceRequirementsHistory With(InsuranceRequirementsChange change) => From([.. _changes, change]);

    /// <inheritdoc/>
    protected override IEnumerable<object> GetEqualityComponents() => _changes;
}
