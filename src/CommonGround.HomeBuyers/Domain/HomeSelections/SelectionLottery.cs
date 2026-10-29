using CommonGround.HomeBuyers.Domain.HomeownerApplications;

namespace CommonGround.HomeBuyers.Domain.HomeSelections;

/// <summary>
/// Draws one application from a pool by seeded lottery. The same seed and the same pool always
/// give the same winner, whatever order the pool is passed in, so anyone can re-run a recorded
/// draw and check its result.
/// </summary>
public static class SelectionLottery
{
    /// <summary>Draws the winning application from <paramref name="pool"/> using <paramref name="seed"/>.</summary>
    public static HomeownerApplicationId Draw(IReadOnlyCollection<HomeownerApplicationId> pool, int seed)
    {
        ArgumentNullException.ThrowIfNull(pool);
        if (pool.Count == 0)
            throw new ArgumentException("The lottery pool is empty.", nameof(pool));

        var ordered = pool.OrderBy(id => id.Value, StringComparer.Ordinal).ToList();
        if (ordered.Select(id => id.Value).Distinct().Count() != ordered.Count)
            throw new ArgumentException("The lottery pool contains the same application more than once.", nameof(pool));

        var index = (int)(SplitMix64((ulong)(uint)seed) % (ulong)ordered.Count);
        return ordered[index];
    }

    // System.Random's seeded sequence isn't guaranteed to stay the same across .NET versions,
    // so the draw uses SplitMix64, whose output is fixed by its definition.
    private static ulong SplitMix64(ulong x)
    {
        x += 0x9E3779B97F4A7C15UL;
        x = (x ^ (x >> 30)) * 0xBF58476D1CE4E5B9UL;
        x = (x ^ (x >> 27)) * 0x94D049BB133111EBUL;
        return x ^ (x >> 31);
    }
}
