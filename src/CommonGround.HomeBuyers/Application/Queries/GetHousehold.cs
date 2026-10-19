using CommonGround.HomeBuyers.Application.ReadModels;
using CommonGround.HomeBuyers.Domain.Households;
using CommonGround.SharedKernel.Domain.Exceptions;
using Marten;

namespace CommonGround.HomeBuyers.Application.Queries;

/// <summary>Returns a single household by ID.</summary>
public sealed record GetHousehold(HouseholdId HouseholdId);

/// <summary>Handles <see cref="GetHousehold"/>.</summary>
public static class GetHouseholdHandler
{
    /// <summary>Loads the household view by ID with its application history, throwing if not found.</summary>
    public static async Task<HouseholdDetails> Handle(
        GetHousehold request,
        IQuerySession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var view = await session.LoadAsync<HouseholdView>(request.HouseholdId.Value, cancellationToken).ConfigureAwait(false)
            ?? throw new AggregateNotFoundException(request.HouseholdId.Value);

        var applications = await session.Query<HomeownerApplicationListItemView>()
            .Where(a => a.HouseholdId == view.Id)
            .OrderByDescending(a => a.SubmittedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new HouseholdDetails(view, applications);
    }
}
