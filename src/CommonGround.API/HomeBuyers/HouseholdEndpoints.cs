using System.Globalization;
using CommonGround.API.Contracts.HomeBuyers.Requests;
using CommonGround.API.Contracts.HomeBuyers.Responses;
using CommonGround.API.Infrastructure.Auth;
using CommonGround.HomeBuyers.Application.Commands;
using CommonGround.HomeBuyers.Application.Queries;
using CommonGround.HomeBuyers.Application.ReadModels;
using CommonGround.HomeBuyers.Domain.Households;
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

    [WolverineGet("/api/households/{householdId}")]
    public static async Task<IResult> GetHousehold(
        string householdId,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(householdId);

        var view = await bus.InvokeForTenantAsync<HouseholdView>(bus.TenantId!,
            new GetHousehold(HouseholdId.Create(householdId)),
            cancellationToken);

        return Results.Ok(MapToResponse(view));
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
                DateOnly.Parse(request.CompletedDate, CultureInfo.InvariantCulture),
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
                DateOnly.Parse(request.CompletedDate, CultureInfo.InvariantCulture),
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
                DateOnly.Parse(request.ExpiryDate, CultureInfo.InvariantCulture),
                userId),
            cancellationToken);

        return Results.NoContent();
    }

    private static HouseholdResponse MapToResponse(HouseholdView view)
    {
        var mortgagePrequalification = view.MortgagePrequalification is null ? null
            : new MortgagePrequalificationDto(
                view.MortgagePrequalification.LenderName,
                view.MortgagePrequalification.ApprovedAmount,
                view.MortgagePrequalification.ExpiryDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));

        return new HouseholdResponse(
            view.Id,
            [.. view.Members.Select(m => new HouseholdMemberDto(m.Name, m.Email, m.Phone, m.IsPrimary))],
            view.HouseholdSize,
            view.AnnualIncome,
            view.AccessibilityRequired,
            view.IsHousingCounselingCompleted,
            view.HousingCounselingCompletedDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            view.HousingCounselingCertificationNumber,
            view.HousingCounselorOrganization,
            view.IsCltOrientationCompleted,
            view.CltOrientationCompletedDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            mortgagePrequalification,
            view.RegisteredAt.ToString("O", CultureInfo.InvariantCulture),
            view.RegisteredBy);
    }
}
