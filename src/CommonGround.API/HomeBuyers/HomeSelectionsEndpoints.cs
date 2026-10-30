using CommonGround.API.Contracts.HomeBuyers.Requests;
using CommonGround.API.Contracts.HomeBuyers.Responses;
using CommonGround.API.Infrastructure.Auth;
using CommonGround.HomeBuyers.Application.Commands;
using CommonGround.HomeBuyers.Application.Queries;
using CommonGround.HomeBuyers.Application.ReadModels;
using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.HomeSelections;
using Wolverine;
using Wolverine.Http;

namespace CommonGround.API.HomeBuyers;

public static class HomeSelectionsEndpoints
{
    [WolverinePost("/api/homes/{homeId}/home-selections")]
    public static async Task<IResult> StartHomeSelection(
        string homeId,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(homeId);

        var userId = httpContext.GetUserId();

        var id = await bus.InvokeForTenantAsync<HomeSelectionId>(bus.TenantId!,
            new StartHomeSelection(homeId, userId),
            cancellationToken);

        return Results.Created($"/api/home-selections/{id.Value}", new StartHomeSelectionResponse(id.Value));
    }

    [WolverineGet("/api/homes/{homeId}/home-selections")]
    public static async Task<IResult> GetHomeSelectionsForHome(
        string homeId,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(homeId);

        var selections = await bus.InvokeForTenantAsync<IReadOnlyList<HomeSelectionView>>(bus.TenantId!,
            new GetHomeSelectionsForHome(homeId),
            cancellationToken);

        var response = selections.Select(s => new HomeSelectionSummaryResponse(
            s.Id,
            s.HomeId,
            s.Status,
            s.StartedBy,
            s.StartedAt,
            s.Candidates.Count,
            s.SelectedApplicationId,
            s.SelectionMethod,
            s.ClosedAt));

        return Results.Ok(response);
    }

    [WolverineGet("/api/home-selections/{selectionId}")]
    public static async Task<IResult> GetHomeSelection(
        string selectionId,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(selectionId);

        var details = await bus.InvokeForTenantAsync<HomeSelectionDetails>(bus.TenantId!,
            new GetHomeSelection(HomeSelectionId.Create(selectionId)),
            cancellationToken);

        return Results.Ok(MapToResponse(details));
    }

    [WolverinePost("/api/home-selections/{selectionId}/select-by-priority")]
    public static async Task<IResult> SelectHouseholdByPriority(
        string selectionId,
        SelectHouseholdByPriorityRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(selectionId);
        ArgumentNullException.ThrowIfNull(request);

        var priorities = new List<SelectionPriority>();
        foreach (var name in request.Priorities ?? [])
        {
            if (!SelectionPriority.TryFromDisplayName(name, out var priority))
            {
                return Results.BadRequest($"Unrecognized selection priority: '{name}'.");
            }

            priorities.Add(priority);
        }

        var userId = httpContext.GetUserId();

        await bus.InvokeForTenantAsync(bus.TenantId!,
            new SelectHouseholdByPriority(
                HomeSelectionId.Create(selectionId),
                HomeownerApplicationId.Create(request.ApplicationId),
                priorities,
                request.Reason,
                request.GroundLeaseTemplateId,
                request.PurchasePrice,
                request.SelectedDate,
                userId),
            cancellationToken);

        return Results.NoContent();
    }

    [WolverinePost("/api/home-selections/{selectionId}/draw-lottery")]
    public static async Task<IResult> DrawSelectionLottery(
        string selectionId,
        DrawSelectionLotteryRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(selectionId);
        ArgumentNullException.ThrowIfNull(request);

        var userId = httpContext.GetUserId();

        await bus.InvokeForTenantAsync(bus.TenantId!,
            new DrawSelectionLottery(
                HomeSelectionId.Create(selectionId),
                request.TiedApplicationIds?.Select(HomeownerApplicationId.Create).ToList(),
                request.GroundLeaseTemplateId,
                request.PurchasePrice,
                request.SelectedDate,
                userId),
            cancellationToken);

        return Results.NoContent();
    }

    [WolverinePost("/api/home-selections/{selectionId}/cancel")]
    public static async Task<IResult> CancelHomeSelection(
        string selectionId,
        CancelHomeSelectionRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(selectionId);
        ArgumentNullException.ThrowIfNull(request);

        var userId = httpContext.GetUserId();

        await bus.InvokeForTenantAsync(bus.TenantId!,
            new CancelHomeSelection(HomeSelectionId.Create(selectionId), request.Reason, userId),
            cancellationToken);

        return Results.NoContent();
    }

    private static HomeSelectionResponse MapToResponse(HomeSelectionDetails details)
    {
        var view = details.Selection;

        return new HomeSelectionResponse(
            view.Id,
            view.HomeId,
            details.Home?.Bedrooms,
            details.Home?.IsAccessible,
            view.Status,
            view.StartedBy,
            view.StartedAt,
            [.. details.Candidates.Select(MapCandidate)],
            view.SelectedApplicationId,
            view.SelectionMethod,
            view.Priorities,
            view.SelectionReason,
            view.LotterySeed,
            view.LotteryPoolApplicationIds,
            view.LotteryIneligibleApplicationIds,
            view.CancellationReason,
            view.ClosedBy,
            view.ClosedAt);
    }

    private static HomeSelectionCandidateDto MapCandidate(HomeSelectionCandidateDetails candidate) =>
        new(
            candidate.Facts.ApplicationId,
            candidate.Facts.HouseholdId,
            MapContact(candidate.PrimaryContact),
            candidate.Facts.AnnualIncome,
            candidate.Facts.HouseholdSize,
            candidate.Facts.RequiresAccessibility,
            candidate.Facts.WaitingSince,
            candidate.ApplicationPhase,
            candidate.CanBeSelected);

    private static HouseholdContactDto? MapContact(HouseholdContact? contact) =>
        contact is null ? null : new HouseholdContactDto(contact.Name, contact.Email, contact.Phone);
}
