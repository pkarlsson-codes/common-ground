using Marten;

namespace CommonGround.API.Demo;

/// <summary>Keeps a demo server under <see cref="DemoOptions.MaxSandboxes"/>.</summary>
public static class DemoCapacity
{
    /// <summary>Throws <see cref="DemoViolation.Full"/> when the demo already holds its maximum number of sandboxes.</summary>
    /// <remarks>Two requests at the same moment can both pass and go one over the cap. The cap guards disk and seed time, so one over is fine.</remarks>
    /// <param name="session">A session in the default tenant.</param>
    /// <param name="options">The demo options.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task.</returns>
    public static async Task EnsureRoomAsync(IQuerySession session, DemoOptions options, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(options);

        var count = await session.Query<DemoSandbox>().CountAsync(cancellationToken).ConfigureAwait(false);
        if (count >= options.MaxSandboxes)
        {
            throw DemoViolation.Full();
        }
    }
}
