using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.GroundLeaseTemplates;
using Marten;

namespace CommonGround.Homeownership.Application.Queries;

/// <summary>Returns the trust's active ground lease templates and the homes that have an active lease, for a portfolio import.</summary>
public sealed record GetLeasingForImport;

/// <summary>The active templates and the IDs of homes with an active lease.</summary>
public sealed record LeasingForImport(IReadOnlyList<GroundLeaseTemplateDetailView> ActiveTemplates, IReadOnlyList<string> LeasedHomeIds);

/// <summary>Handles <see cref="GetLeasingForImport"/>.</summary>
public static class GetLeasingForImportHandler
{
    /// <summary>Loads the active templates and the homes under an active lease.</summary>
    public static async Task<LeasingForImport> Handle(
        GetLeasingForImport request,
        IQuerySession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(session);

        var activeTemplate = GroundLeaseTemplateStatus.Active.DisplayName;
        var templates = await session.Query<GroundLeaseTemplateDetailView>()
            .Where(t => t.Status == activeTemplate)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var activeLease = GroundLeaseStatus.Active.DisplayName;
        var leasedHomeIds = await session.Query<GroundLeaseListItemView>()
            .Where(l => l.Status == activeLease)
            .Select(l => l.HomeId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new LeasingForImport([.. templates], [.. leasedHomeIds]);
    }
}
