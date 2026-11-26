using CommonGround.API.Contracts.HomeBuyers.Requests;
using CommonGround.API.Contracts.HomeBuyers.Responses;
using CommonGround.API.Infrastructure.Auth;
using CommonGround.HomeBuyers.Application.Commands;
using CommonGround.HomeBuyers.Application.Queries;
using CommonGround.HomeBuyers.Application.ReadModels;
using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.HomeSelections;
using CommonGround.SharedKernel.Infrastructure.API;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Wolverine.Http;

namespace CommonGround.API.HomeBuyers;

public static class HomeSelectionsEndpoints
{
    [WolverineGet("/api/homes/{homeId}/selection")]
    public static async Task<IResult> GetHomeSelectionOverview(
        string homeId,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(homeId);

        var overview = await bus.InvokeForTenantAsync<HomeSelectionOverview>(bus.TenantId!,
            new GetHomeSelectionOverview(homeId),
            cancellationToken);

        return Results.Ok(MapToResponse(overview));
    }

    [WolverineGet("/api/home-selections")]
    public static async Task<IResult> GetHomeSelections(
        [FromQuery] GetHomeSelectionsQuerySpec request,
        IMessageBus bus,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = await bus.InvokeForTenantAsync<PaginatedResponse<HomeSelectionListItem>>(bus.TenantId!,
            new GetHomeSelections(request.Page),
            cancellationToken);

        var response = result.MapItems(i => new HomeSelectionListItemResponse(
            i.Selection.Id,
            i.Selection.HomeId,
            i.HomeLabel,
            i.Selection.SelectedDate,
            i.Selection.RecordedAt,
            i.Selection.SelectionMethod,
            i.Selection.Applications.Count,
            i.Selection.SelectedApplicationId,
            MapContact(i.ChosenContact)));

        return Results.Ok(response);
    }

    [WolverinePost("/api/homes/{homeId}/home-selections/select-by-priority")]
    public static async Task<IResult> SelectHouseholdByPriority(
        string homeId,
        SelectHouseholdByPriorityRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(homeId);
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

        var id = HomeSelectionId.Generate();
        await bus.InvokeForTenantAsync(bus.TenantId!,
            new SelectHouseholdByPriority(
                id,
                homeId,
                HomeownerApplicationId.Create(request.ApplicationId),
                priorities,
                request.Reason,
                request.GroundLeaseTemplateId,
                request.PurchasePrice,
                request.SelectedDate,
                userId),
            cancellationToken);

        return Results.Created($"/api/homes/{homeId}/selection", new HomeSelectionCreatedResponse(id.Value));
    }

    [WolverinePost("/api/homes/{homeId}/home-selections/draw-lottery")]
    public static async Task<IResult> DrawSelectionLottery(
        string homeId,
        DrawSelectionLotteryRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(homeId);
        ArgumentNullException.ThrowIfNull(request);

        var userId = httpContext.GetUserId();

        var id = HomeSelectionId.Generate();
        await bus.InvokeForTenantAsync(bus.TenantId!,
            new DrawSelectionLottery(
                id,
                homeId,
                request.TiedApplicationIds?.Select(HomeownerApplicationId.Create).ToList(),
                request.GroundLeaseTemplateId,
                request.PurchasePrice,
                request.SelectedDate,
                userId),
            cancellationToken);

        return Results.Created($"/api/homes/{homeId}/selection", new HomeSelectionCreatedResponse(id.Value));
    }

    private static HomeSelectionOverviewResponse MapToResponse(HomeSelectionOverview overview)
    {
        var home = overview.Home;

        var waitingList = overview.WaitingList
            .Select(a => new WaitingListComparisonEntryDto(
                a.ApplicationId,
                a.HouseholdId,
                MapContact(a.PrimaryContact),
                a.AnnualIncome,
                a.HouseholdSize,
                a.RequiresAccessibility,
                a.WaitingSince,
                a.IncomeLimit,
                a.Reasons.Count == 0,
                a.Reasons,
                a.PrequalifiedAmount))
            .ToList();

        return new HomeSelectionOverviewResponse(
            home.Id,
            home.Label,
            home.Bedrooms,
            home.Bathrooms,
            home.SquareFootage,
            home.IsAccessible,
            home.IncomeLimitPercentOfAmi,
            overview.IncomeLimitsEffectiveFrom,
            overview.HasIncomeLimitsInEffect,
            overview.SaleStage,
            overview.IsOffered,
            overview.Today,
            waitingList,
            [.. overview.Decisions.Select(MapDecision)]);
    }

    private static HomeSelectionDecisionDto MapDecision(HomeSelectionDecision decision)
    {
        var s = decision.Selection;
        var applications = decision.Applications
            .Select(a => new HomeSelectionDecisionApplicationDto(
                a.Entry.ApplicationId,
                a.Entry.HouseholdId,
                MapContact(a.PrimaryContact),
                a.Entry.AnnualIncome,
                a.Entry.HouseholdSize,
                a.Entry.RequiresAccessibility,
                a.Entry.WaitingSince,
                a.Entry.IncomeLimit,
                a.Entry.Outcome,
                a.Entry.Reasons))
            .ToList();

        return new HomeSelectionDecisionDto(
            s.Id,
            s.SelectedApplicationId,
            s.SelectedHouseholdId,
            s.SelectedDate,
            s.SelectedBy,
            s.RecordedAt,
            s.SelectionMethod,
            s.GroundLeaseTemplateId,
            s.PurchasePrice,
            s.IncomeLimitPercentOfAmi,
            s.IncomeLimitsEffectiveFrom,
            s.Priorities,
            s.SelectionReason,
            s.LotteryTiedApplicationIds,
            s.LotteryPoolApplicationIds,
            s.LotterySeed,
            applications);
    }

    private static HouseholdContactDto? MapContact(HouseholdContact? contact) =>
        contact is null ? null : new HouseholdContactDto(contact.Name, contact.Email, contact.Phone);
}
