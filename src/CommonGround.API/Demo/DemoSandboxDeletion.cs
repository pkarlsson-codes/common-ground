using CommonGround.Documents.Application.ReadModels;
using CommonGround.Documents.Domain.Documents;
using CommonGround.Identity.Application.Commands;
using CommonGround.Identity.Application.ReadModels;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Services;
using CommonGround.TrustManagement.Application.ReadModels;
using CommonGround.TrustManagement.Infrastructure.Seeding;
using Marten;
using Wolverine;
using Wolverine.Persistence.Durability.ScheduledMessageManagement;
using Wolverine.Runtime;

namespace CommonGround.API.Demo;

/// <summary>
/// Deletes demo sandboxes past their delete date, with everything their trust holds. Each step can run again,
/// so a delete cut short finishes on the next pass: the <see cref="DemoSandbox"/> record goes last.
/// </summary>
/// <param name="store">The document store.</param>
/// <param name="bus">Takes members out of the trust.</param>
/// <param name="blobs">Holds the trust's document files.</param>
/// <param name="runtime">Holds the trust's scheduled messages.</param>
/// <param name="logger">The logger.</param>
public sealed class DemoSandboxDeletion(
    IDocumentStore store,
    IMessageBus bus,
    IDocumentBlobStore blobs,
    IWolverineRuntime runtime,
    ILogger<DemoSandboxDeletion> logger)
{
    private const int ScheduledMessagePageSize = 500;

    /// <summary>
    /// Deletes every sandbox whose delete date has passed. A sandbox still <see cref="DemoSandboxStatus.Seeding"/>
    /// waits: its seed run would write into the trust while it is deleted.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>How many sandboxes were deleted.</returns>
    public async Task<int> DeleteDueAsync(CancellationToken cancellationToken)
    {
        var now = TimeProviderContext.Current.UtcNow;
        IReadOnlyList<DemoSandbox> due;
        var session = store.QuerySession();
        await using (session.ConfigureAwait(false))
        {
            due = await session.Query<DemoSandbox>()
                .Where(s => s.DeletesAt <= now && s.Status != DemoSandboxStatus.Seeding)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        var deleted = 0;
        foreach (var sandbox in due)
        {
            try
            {
                await DeleteAsync(sandbox, cancellationToken).ConfigureAwait(false);
                deleted++;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Demo sandbox {TrustId} (run {RunKey}): delete failed; it is tried again on the next pass", sandbox.Id, sandbox.RunKey);
            }
        }

        return deleted;
    }

    /// <summary>Deletes one sandbox and its trust. A sandbox whose trust was never created is deleted without error.</summary>
    /// <param name="sandbox">The sandbox.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task.</returns>
    public async Task DeleteAsync(DemoSandbox sandbox, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(sandbox);
        var trustId = sandbox.Id;

        // Members first, so the membership check refuses them before their records disappear.
        var members = await RemoveMembersAsync(trustId, cancellationToken).ConfigureAwait(false);
        var cancelled = await CancelScheduledMessagesAsync(trustId, cancellationToken).ConfigureAwait(false);

        // File keys come from the trust's documents, so the files go before the tenant data.
        var files = await DeleteDocumentFilesAsync(trustId, cancellationToken).ConfigureAwait(false);
        await store.Advanced.DeleteAllTenantDataAsync(trustId, cancellationToken).ConfigureAwait(false);

        // The trust's own stream and documents live in the default tenant.
        var session = store.LightweightSession();
        await using (session.ConfigureAwait(false))
        {
            await store.Advanced.Clean.DeleteSingleEventStreamAsync(trustId, session.TenantId, cancellationToken).ConfigureAwait(false);
            session.HardDeleteWhere<TrustSeedLookup>(x => x.TrustId == trustId);
            session.HardDeleteWhere<TrustInvitationView>(x => x.TrustId == trustId);
            session.HardDelete<DemoSandbox>(trustId);
            await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Demo sandbox {TrustId} (run {RunKey}) deleted: {Members} members removed, {Files} files and {ScheduledMessages} scheduled messages deleted",
                trustId,
                sandbox.RunKey,
                members,
                files,
                cancelled);
        }
    }

    private async Task<int> RemoveMembersAsync(string trustId, CancellationToken cancellationToken)
    {
        IReadOnlyList<string> userIds;
        var session = store.QuerySession();
        await using (session.ConfigureAwait(false))
        {
            userIds = await session.Query<UserView>()
                .Where(x => x.TrustId == trustId)
                .Select(x => x.UserId)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        foreach (var userId in userIds)
        {
            await bus.InvokeAsync(new LeaveDeletedTrust(UserId.Create(userId), TrustId.Create(trustId)), cancellationToken).ConfigureAwait(false);
        }

        return userIds.Count;
    }

    // The scheduled message API can't filter by tenant, so this reads every scheduled message and cancels the trust's.
    private async Task<int> CancelScheduledMessagesAsync(string trustId, CancellationToken cancellationToken)
    {
        var scheduled = runtime.Storage.ScheduledMessages;
        var ids = new List<Guid>();
        for (var page = 0; ; page++)
        {
            var results = await scheduled.QueryAsync(new ScheduledMessageQuery { PageNumber = page, PageSize = ScheduledMessagePageSize }, cancellationToken).ConfigureAwait(false);
            ids.AddRange(results.Messages.Where(m => m.TenantId == trustId).Select(m => m.Id));
            if (results.Messages.Count < ScheduledMessagePageSize)
            {
                break;
            }
        }

        if (ids.Count > 0)
        {
            await scheduled.CancelAsync(new ScheduledMessageQuery { MessageIds = [.. ids] }, cancellationToken).ConfigureAwait(false);
        }

        return ids.Count;
    }

    private async Task<int> DeleteDocumentFilesAsync(string trustId, CancellationToken cancellationToken)
    {
        IReadOnlyList<DocumentView> documents;
        var session = store.QuerySession(trustId);
        await using (session.ConfigureAwait(false))
        {
            documents = await session.Query<DocumentView>().ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        var keys = documents.SelectMany(d => d.Versions).Select(v => v.BlobKey).Where(k => !string.IsNullOrWhiteSpace(k)).Distinct(StringComparer.Ordinal).ToList();
        foreach (var key in keys)
        {
            await blobs.DeleteAsync(key, cancellationToken).ConfigureAwait(false);
        }

        return keys.Count;
    }
}
