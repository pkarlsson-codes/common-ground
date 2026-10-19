using System.Globalization;
using CommonGround.API.Contracts.ApplicantSelection.Requests;
using CommonGround.API.Contracts.ApplicantSelection.Responses;
using CommonGround.API.Infrastructure.Auth;
using CommonGround.HomeBuyers.Application.Commands;
using CommonGround.HomeBuyers.Application.Queries;
using CommonGround.HomeBuyers.Application.ReadModels;
using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.Households;
using CommonGround.SharedKernel.Infrastructure.API;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Wolverine.Http;

namespace CommonGround.API.HomeBuyers;
public static class HomeownerApplicationsEndpoints
{
    [WolverinePost("/api/homeowner-applications")]
    public static async Task<IResult> SubmitApplication(
        SubmitApplicationRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var userId = httpContext.GetUserId();

        var command = new SubmitApplication(
            HouseholdId.Create(request.HouseholdId),
            userId);

        var id = await bus.InvokeForTenantAsync<HomeownerApplicationId>(bus.TenantId!, command, cancellationToken);

        return Results.Created($"/api/homeowner-applications/{id.Value}", new SubmitApplicationResponse(id.Value));
    }

    [WolverineGet("/api/homeowner-applications")]
    public static async Task<IResult> GetHomeownerApplications(
        [FromQuery] GetHomeownerApplicationsQuerySpec request,
        IMessageBus bus,
        CancellationToken cancellationToken = default)
    {
        var query = new GetHomeownerApplications(request.Page, request.Phase);

        var result = await bus.InvokeForTenantAsync<PaginatedResponse<HomeownerApplicationSummary>>(bus.TenantId!, query, cancellationToken);

        var response = result.MapItems(s => new HomeownerApplicationListItemResponse(
            s.Application.Id,
            s.Application.HouseholdId,
            MapContact(s.PrimaryContact),
            s.Application.HouseholdSize,
            s.Application.AccessibilityRequired,
            s.Application.Phase,
            s.Application.SubmittedAt.ToString("O", CultureInfo.InvariantCulture)));

        return Results.Ok(response);
    }

    [WolverineGet("/api/homeowner-applications/{applicationId}")]
    public static async Task<IResult> GetHomeownerApplication(
        string applicationId,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationId);

        var details = await bus.InvokeForTenantAsync<HomeownerApplicationDetails>(bus.TenantId!,
            new GetHomeownerApplication(HomeownerApplicationId.Create(applicationId)),
            cancellationToken);

        return Results.Ok(MapToResponse(details));
    }

    [WolverinePost("/api/homeowner-applications/{applicationId}/approve")]
    public static async Task<IResult> ApproveApplication(
        string applicationId,
        ApproveEligibilityRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationId);
        ArgumentNullException.ThrowIfNull(request);

        var userId = httpContext.GetUserId();

        await bus.InvokeForTenantAsync(bus.TenantId!,
            new ApproveApplication(
                HomeownerApplicationId.Create(applicationId),
                DateOnly.Parse(request.ApprovedDate, CultureInfo.InvariantCulture),
                request.Notes,
                userId),
            cancellationToken);

        return Results.NoContent();
    }

    [WolverineGet("/api/homes/{homeId}/applicant-candidates")]
    public static async Task<IResult> GetHomeCandidates(
        string homeId,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(homeId);

        var candidates = await bus.InvokeForTenantAsync<IReadOnlyList<HomeCandidate>>(bus.TenantId!,
            new GetHomeCandidates(homeId),
            cancellationToken);

        var response = candidates.Select(c => new HomeCandidateResponse(
            c.Application.Id,
            c.Application.HouseholdId,
            MapContact(c.PrimaryContact)));

        return Results.Ok(response);
    }

    [WolverinePost("/api/homeowner-applications/{applicationId}/select")]
    public static async Task<IResult> SelectApplicant(
        string applicationId,
        SelectApplicantRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationId);
        ArgumentNullException.ThrowIfNull(request);

        var userId = httpContext.GetUserId();

        await bus.InvokeForTenantAsync(bus.TenantId!,
            new SelectApplicant(
                HomeownerApplicationId.Create(applicationId),
                request.HomeId,
                request.GroundLeaseTemplateId,
                request.PurchasePrice,
                DateOnly.Parse(request.SelectedDate, CultureInfo.InvariantCulture),
                userId),
            cancellationToken);

        return Results.NoContent();
    }

    [WolverinePost("/api/homeowner-applications/{applicationId}/deny")]
    public static async Task<IResult> DenyApplication(
        string applicationId,
        DenyApplicationRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationId);
        ArgumentNullException.ThrowIfNull(request);

        var userId = httpContext.GetUserId();

        await bus.InvokeForTenantAsync(bus.TenantId!,
            new DenyApplication(
                HomeownerApplicationId.Create(applicationId),
                request.Reason,
                DateOnly.Parse(request.DeniedDate, CultureInfo.InvariantCulture),
                userId),
            cancellationToken);

        return Results.NoContent();
    }

    [WolverinePost("/api/homeowner-applications/{applicationId}/withdraw")]
    public static async Task<IResult> WithdrawApplication(
        string applicationId,
        WithdrawApplicationRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationId);
        ArgumentNullException.ThrowIfNull(request);

        var userId = httpContext.GetUserId();

        await bus.InvokeForTenantAsync(bus.TenantId!,
            new WithdrawApplication(
                HomeownerApplicationId.Create(applicationId),
                DateOnly.Parse(request.WithdrawnDate, CultureInfo.InvariantCulture),
                request.Reason,
                userId),
            cancellationToken);

        return Results.NoContent();
    }

    private static HomeownerApplicationResponse MapToResponse(HomeownerApplicationDetails details)
    {
        var view = details.Application;

        return new HomeownerApplicationResponse(
            view.Id,
            view.HouseholdId,
            MapContact(details.PrimaryContact),
            view.HouseholdSize,
            view.AnnualIncome,
            view.AccessibilityRequired,
            view.Phase,
            view.SubmittedAt.ToString("O", CultureInfo.InvariantCulture),
            view.SubmittedBy,
            view.EligibilityApprovedDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            view.DenialReason,
            view.WithdrawalReason,
            view.AddedToWaitingListDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            view.SelectedDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            view.SelectedHomeId,
            view.SelectedLandParcelId,
            view.SelectedBy);
    }

    private static HouseholdContactDto? MapContact(HouseholdContact? contact) =>
        contact is null ? null : new HouseholdContactDto(contact.Name, contact.Email, contact.Phone);
}
