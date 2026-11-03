namespace CommonGround.SharedKernel.Infrastructure.Seeding;

/// <summary>Checks once whether events are still propagating.</summary>
public interface IPropagationProbe
{
    /// <summary>Describes what is still propagating, or returns null when nothing is.</summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A description of the pending work, or null when idle.</returns>
    Task<string?> FindPendingAsync(CancellationToken cancellationToken);
}
