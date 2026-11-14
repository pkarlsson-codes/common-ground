using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.GroundLeases.Insurance;
using CommonGround.SharedKernel.Domain;
using Marten;

namespace CommonGround.Homeownership.Application.Queries;

/// <summary>An active lease and where its insurance stands.</summary>
/// <param name="Lease">The lease.</param>
/// <param name="HomeLabel">The label of the lease's home, if it has arrived.</param>
/// <param name="Cover">Where its insurance stands.</param>
public sealed record LeaseInsurance(GroundLeaseView Lease, string? HomeLabel, InsuranceCover Cover)
{
    /// <summary>Works out a lease's insurance cover from its read model, leaving out removed certificates.</summary>
    public static InsuranceCover CoverOf(GroundLeaseView lease)
    {
        ArgumentNullException.ThrowIfNull(lease);

        return InsuranceCover.Of(
            lease.LeaseStartDate,
            lease.InsuranceCertificates.Where(c => c.RemovedReason is null).Select(CoverageOf),
            GetGroundLeaseHandler.InsuranceRequirementsOf(lease));
    }

    /// <summary>A certificate's coverage, from its read model entry.</summary>
    public static InsuranceCoverage CoverageOf(InsuranceCertificateEntry certificate)
    {
        ArgumentNullException.ThrowIfNull(certificate);

        return InsuranceCoverage.Create(
            certificate.PolicyStart,
            certificate.PolicyEnd,
            Money.Create(certificate.DwellingCoverage),
            Money.Create(certificate.LiabilityPerOccurrence),
            certificate.TrustNamedAdditionalInsured,
            certificate.ReceivedOn);
    }

    /// <summary>
    /// Loads every active lease, a resale in progress included (the homeowner still owns the home), with its cover.
    /// A trust has hundreds of leases, so the cover is worked out in memory.
    /// </summary>
    public static async Task<List<LeaseInsurance>> LoadActiveAsync(IQuerySession session, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(session);

        var active = GroundLeaseStatus.Active.DisplayName;
        var leases = await session.Query<GroundLeaseView>()
            .Where(l => l.Status == active)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var labels = (await session.LoadManyAsync<HomeLabelView>(cancellationToken, leases.Select(l => l.HomeId).Distinct())
                .ConfigureAwait(false))
            .ToDictionary(l => l.Id, l => l.Label);

        return [.. leases.Select(l => new LeaseInsurance(l, labels.GetValueOrDefault(l.HomeId), CoverOf(l)))];
    }
}
