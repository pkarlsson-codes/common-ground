using CommonGround.HomeBuyers.Application.ReadModels;
using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.SharedKernel.Domain.Exceptions;
using Marten;

namespace CommonGround.HomeBuyers.Application.Queries;

/// <summary>Returns a single homeowner application by ID.</summary>
public sealed record GetHomeownerApplication(HomeownerApplicationId ApplicationId);

/// <summary>Handles <see cref="GetHomeownerApplication"/>.</summary>
public static class GetHomeownerApplicationHandler
{
    /// <summary>Loads the application view by ID with its household's primary contact, throwing if not found.</summary>
    public static async Task<HomeownerApplicationDetails> Handle(
        GetHomeownerApplication request,
        IQuerySession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var view = await session.LoadAsync<HomeownerApplicationView>(request.ApplicationId.Value, cancellationToken).ConfigureAwait(false)
            ?? throw new AggregateNotFoundException(request.ApplicationId.Value);

        var household = await session.LoadAsync<HouseholdListItemView>(view.HouseholdId, cancellationToken).ConfigureAwait(false);

        return new HomeownerApplicationDetails(view, household is null ? null : HouseholdContact.From(household));
    }
}
