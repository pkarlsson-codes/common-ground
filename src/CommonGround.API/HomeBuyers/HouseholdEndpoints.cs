using CommonGround.API.Contracts.HomeBuyers.Requests;
using CommonGround.API.Contracts.HomeBuyers.Responses;
using CommonGround.API.Infrastructure.Auth;
using CommonGround.HomeBuyers.Application.Commands;
using CommonGround.HomeBuyers.Application.Queries;
using CommonGround.HomeBuyers.Application.ReadModels;
using CommonGround.HomeBuyers.Domain.Households;
using CommonGround.SharedKernel.Infrastructure.API;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Wolverine.Http;

namespace CommonGround.API.HomeBuyers;

public static class HouseholdEndpoints
{
    [WolverinePost("/api/households")]
    public static async Task<IResult> RegisterHousehold(
        RegisterHouseholdRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var userId = httpContext.GetUserId();

        var command = new RegisterHousehold(
            [.. request.Members.Select(m => new RegisterHouseholdMember(m.Name, m.Email, m.Phone, m.IsPrimary))],
            request.RequiresAccessibility,
            request.AnnualIncome,
            userId);

        var id = await bus.InvokeForTenantAsync<HouseholdId>(bus.TenantId!, command, cancellationToken);

        return Results.Created($"/api/households/{id.Value}", new RegisterHouseholdResponse(id.Value));
    }

    [WolverineGet("/api/households")]
    public static async Task<IResult> GetHouseholds(
        [FromQuery] GetHouseholdsQuerySpec request,
        IMessageBus bus,
        CancellationToken cancellationToken = default)
    {
        var query = new GetHouseholds(request.Page, request.Search);

        var result = await bus.InvokeForTenantAsync<PaginatedResponse<HouseholdListItemView>>(bus.TenantId!, query, cancellationToken);

        var response = result.MapItems(h => new HouseholdListItemResponse(
            h.Id,
            new HouseholdContactDto(h.PrimaryMemberName, h.PrimaryMemberEmail, h.PrimaryMemberPhone),
            h.HouseholdSize,
            h.AccessibilityRequired,
            h.IsHousingCounselingCompleted,
            h.IsCltOrientationCompleted,
            h.MortgagePrequalificationExpiryDate,
            h.RegisteredAt));

        return Results.Ok(response);
    }

    [WolverineGet("/api/households/{householdId}")]
    public static async Task<IResult> GetHousehold(
        string householdId,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(householdId);

        var details = await bus.InvokeForTenantAsync<HouseholdDetails>(bus.TenantId!,
            new GetHousehold(HouseholdId.Create(householdId)),
            cancellationToken);

        return Results.Ok(MapToResponse(details));
    }

    [WolverinePost("/api/households/{householdId}/members")]
    public static async Task<IResult> AddHouseholdMember(
        string householdId,
        AddHouseholdMemberRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(householdId);
        ArgumentNullException.ThrowIfNull(request);

        var userId = httpContext.GetUserId();

        var memberId = await bus.InvokeForTenantAsync<HouseholdMemberId>(bus.TenantId!,
            new AddHouseholdMember(
                HouseholdId.Create(householdId),
                request.Name,
                request.Email,
                request.Phone,
                userId),
            cancellationToken);

        return Results.Created($"/api/households/{householdId}", new AddHouseholdMemberResponse(memberId.Value));
    }

    [WolverinePut("/api/households/{householdId}/members/{memberId}")]
    public static async Task<IResult> UpdateHouseholdMember(
        string householdId,
        string memberId,
        UpdateHouseholdMemberRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(householdId);
        ArgumentException.ThrowIfNullOrWhiteSpace(memberId);
        ArgumentNullException.ThrowIfNull(request);

        var userId = httpContext.GetUserId();

        await bus.InvokeForTenantAsync(bus.TenantId!,
            new UpdateHouseholdMember(
                HouseholdId.Create(householdId),
                HouseholdMemberId.Create(memberId),
                request.Name,
                request.Email,
                request.Phone,
                userId),
            cancellationToken);

        return Results.NoContent();
    }

    [WolverineDelete("/api/households/{householdId}/members/{memberId}")]
    public static async Task<IResult> RemoveHouseholdMember(
        string householdId,
        string memberId,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(householdId);
        ArgumentException.ThrowIfNullOrWhiteSpace(memberId);

        var userId = httpContext.GetUserId();

        await bus.InvokeForTenantAsync(bus.TenantId!,
            new RemoveHouseholdMember(
                HouseholdId.Create(householdId),
                HouseholdMemberId.Create(memberId),
                userId),
            cancellationToken);

        return Results.NoContent();
    }

    [WolverinePut("/api/households/{householdId}/circumstances")]
    public static async Task<IResult> UpdateHouseholdCircumstances(
        string householdId,
        UpdateHouseholdCircumstancesRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(householdId);
        ArgumentNullException.ThrowIfNull(request);

        var userId = httpContext.GetUserId();

        await bus.InvokeForTenantAsync(bus.TenantId!,
            new UpdateHouseholdCircumstances(
                HouseholdId.Create(householdId),
                request.RequiresAccessibility,
                request.AnnualIncome,
                userId),
            cancellationToken);

        return Results.NoContent();
    }

    [WolverinePost("/api/households/{householdId}/housing-counseling")]
    public static async Task<IResult> RecordHousingCounseling(
        string householdId,
        RecordHousingCounselingRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(householdId);
        ArgumentNullException.ThrowIfNull(request);

        var userId = httpContext.GetUserId();

        await bus.InvokeForTenantAsync(bus.TenantId!,
            new RecordHousingCounseling(
                HouseholdId.Create(householdId),
                request.CertificationNumber,
                request.CompletedDate,
                request.CounselorOrganization,
                userId),
            cancellationToken);

        return Results.NoContent();
    }

    [WolverinePost("/api/households/{householdId}/clt-orientation")]
    public static async Task<IResult> RecordCltOrientation(
        string householdId,
        RecordCltOrientationRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(householdId);
        ArgumentNullException.ThrowIfNull(request);

        var userId = httpContext.GetUserId();

        await bus.InvokeForTenantAsync(bus.TenantId!,
            new RecordCltOrientation(
                HouseholdId.Create(householdId),
                request.CompletedDate,
                userId),
            cancellationToken);

        return Results.NoContent();
    }

    [WolverinePost("/api/households/{householdId}/mortgage-prequalification")]
    public static async Task<IResult> RecordMortgagePrequalification(
        string householdId,
        RecordMortgagePrequalificationRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(householdId);
        ArgumentNullException.ThrowIfNull(request);

        var userId = httpContext.GetUserId();

        await bus.InvokeForTenantAsync(bus.TenantId!,
            new RecordMortgagePrequalification(
                HouseholdId.Create(householdId),
                request.LenderName,
                request.ApprovedAmount,
                request.ExpiryDate,
                userId),
            cancellationToken);

        return Results.NoContent();
    }

    private static HouseholdResponse MapToResponse(HouseholdDetails details)
    {
        var view = details.Household;
        var applications = details.Applications
            .Select(a => new HouseholdApplicationDto(a.Id, a.Phase, a.SubmittedAt))
            .ToList();

        var mortgagePrequalification = view.MortgagePrequalification is null ? null
            : new MortgagePrequalificationDto(
                view.MortgagePrequalification.LenderName,
                view.MortgagePrequalification.ApprovedAmount,
                view.MortgagePrequalification.ExpiryDate);

        return new HouseholdResponse(
            view.Id,
            [.. view.Members.Select(m => new HouseholdMemberDto(m.Id, m.Name, m.Email, m.Phone, m.IsPrimary))],
            view.HouseholdSize,
            view.AnnualIncome,
            view.AccessibilityRequired,
            view.IsHousingCounselingCompleted,
            view.HousingCounselingCompletedDate,
            view.HousingCounselingCertificationNumber,
            view.HousingCounselorOrganization,
            view.IsCltOrientationCompleted,
            view.CltOrientationCompletedDate,
            mortgagePrequalification,
            view.RegisteredAt,
            view.RegisteredBy,
            applications);
    }
}
