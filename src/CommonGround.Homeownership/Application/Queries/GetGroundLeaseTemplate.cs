using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.Homeownership.Domain.GroundLeaseTemplates;
using CommonGround.SharedKernel.Domain.Exceptions;
using Marten;

namespace CommonGround.Homeownership.Application.Queries;

/// <summary>Query that retrieves a single ground lease template by ID.</summary>
/// <param name="TemplateId">The ID of the ground lease template to retrieve.</param>
public sealed record GetGroundLeaseTemplate(GroundLeaseTemplateId TemplateId);

/// <summary>Handles <see cref="GetGroundLeaseTemplate"/> queries.</summary>
public static class GetGroundLeaseTemplateHandler
{
    /// <summary>Loads and returns the <see cref="GroundLeaseTemplateDetailView"/> for the given query.</summary>
    /// <param name="request">The query containing the template ID.</param>
    /// <param name="session">The Marten query session.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The detail view for the requested template.</returns>
    public static async Task<GroundLeaseTemplateDetailView> Handle(
        GetGroundLeaseTemplate request,
        IQuerySession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var view = await session.LoadAsync<GroundLeaseTemplateDetailView>(request.TemplateId.Value, cancellationToken).ConfigureAwait(false)
            ?? throw new AggregateNotFoundException(request.TemplateId.Value);

        return view;
    }
}
