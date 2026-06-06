using CommonGround.ApplicantSelection.Application.ReadModels;
using CommonGround.ApplicantSelection.Domain.HomeownerApplications;
using CommonGround.SharedKernel.Domain.Exceptions;
using Marten;

namespace CommonGround.ApplicantSelection.Application.Queries;

/// <summary>Returns a single homeowner application by ID.</summary>
public sealed record GetHomeownerApplication(HomeownerApplicationId ApplicationId);

/// <summary>Handles <see cref="GetHomeownerApplication"/>.</summary>
public static class GetHomeownerApplicationHandler
{
    /// <summary>Loads the application view by ID, throwing if not found.</summary>
    public static async Task<HomeownerApplicationView> Handle(
        GetHomeownerApplication request,
        IQuerySession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var view = await session.LoadAsync<HomeownerApplicationView>(request.ApplicationId.Value, cancellationToken).ConfigureAwait(false)
            ?? throw new AggregateNotFoundException(request.ApplicationId.Value);

        return view;
    }
}