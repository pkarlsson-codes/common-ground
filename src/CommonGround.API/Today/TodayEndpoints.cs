using CommonGround.API.Contracts.Today.Responses;
using CommonGround.Documents.Application.Queries;
using CommonGround.HomeBuyers.Application.Queries;
using CommonGround.Homeownership.Application.Queries;
using CommonGround.PropertyRegistry.Application.Queries;
using CommonGround.SharedKernel.Application.WorkItems;
using CommonGround.SharedKernel.Domain.Services;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Wolverine.Http;

namespace CommonGround.API.Today;

/// <summary>
/// Today: what needs attention across every context. Each context derives its own work items from its own read
/// models; this asks each of them and puts the answers in one queue.
/// </summary>
public static class TodayEndpoints
{
    [WolverineGet("/api/today")]
    public static async Task<IResult> GetToday(
        [FromQuery] string[]? areas,
        IMessageBus bus,
        ITrustClock trustClock,
        CancellationToken cancellationToken)
    {
        var tenantId = bus.TenantId!;
        var today = await trustClock.TodayAsync(tenantId, cancellationToken).ConfigureAwait(false);

        var homeBuyers = await bus.InvokeForTenantAsync<WorkItemList>(tenantId, new GetHomeBuyersWorkItems(today), cancellationToken).ConfigureAwait(false);
        var propertyRegistry = await bus.InvokeForTenantAsync<WorkItemList>(tenantId, new GetPropertyRegistryWorkItems(), cancellationToken).ConfigureAwait(false);
        var homeownership = await bus.InvokeForTenantAsync<WorkItemList>(tenantId, new GetHomeownershipWorkItems(today), cancellationToken).ConfigureAwait(false);
        var documents = await bus.InvokeForTenantAsync<WorkItemList>(tenantId, new GetDocumentsWorkItems(today), cancellationToken).ConfigureAwait(false);

        var all = WorkUrgency.Order([
            .. homeBuyers.Items,
            .. propertyRegistry.Items,
            .. homeownership.Items,
            .. documents.Items,
        ]);

        var requested = areas is { Length: > 0 } ? all.Where(i => areas.Contains(i.Area, StringComparer.Ordinal)) : all;

        WorkItemResponse[] items = [.. requested.Select(i => new WorkItemResponse(
            i.Area, i.Kind, i.RecordType, i.RecordId, i.Label, i.Detail, i.DueDate, i.WaitingSince, WorkUrgency.UrgencyOf(i, today)))];

        return Results.Ok(new TodayResponse(
            today,
            all.Count,
            items,
            [.. WorkArea.All.Select(a => new WorkCountResponse(a, all.Count(i => i.Area == a)))],
            [.. all.GroupBy(i => i.Kind).Select(g => new WorkCountResponse(g.Key, g.Count()))]));
    }
}
