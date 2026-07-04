using System.Globalization;
using CommonGround.API.Contracts.ApplicantSelection.Requests;
using CommonGround.API.Contracts.ApplicantSelection.Responses;
using CommonGround.API.Infrastructure.Auth;
using CommonGround.ApplicantSelection.Application.Commands;
using CommonGround.ApplicantSelection.Application.Queries;
using CommonGround.ApplicantSelection.Application.ReadModels;
using CommonGround.ApplicantSelection.Domain.HomeownerApplications;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Infrastructure.API;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Wolverine.Http;

namespace CommonGround.API.ApplicantSelection;
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
            new Applicant(request.ApplicantFullName, request.ApplicantEmail, PhoneNumber.FromOptional(request.ApplicantPhone)),
            request.HouseholdSize,
            request.AnnualIncome,
            request.AccessibilityRequired,
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
        var query = new GetHomeownerApplications(request.Page, request.Phase, request.Search);

        var result = await bus.InvokeForTenantAsync<PaginatedResponse<HomeownerApplicationListItemView>>(bus.TenantId!, query, cancellationToken);

        var response = result.MapItems(v => new HomeownerApplicationListItemResponse(
            v.Id,
            v.ApplicantFullName,
            v.ApplicantEmail,
            v.HouseholdSize,
            v.AccessibilityRequired,
            v.Phase,
            v.SubmittedAt.ToString("O", CultureInfo.InvariantCulture)));

        return Results.Ok(response);
    }

    [WolverineGet("/api/homeowner-applications/{applicationId}")]
    public static async Task<IResult> GetHomeownerApplication(
        string applicationId,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationId);

        var view = await bus.InvokeForTenantAsync<HomeownerApplicationView>(bus.TenantId!,
            new GetHomeownerApplication(HomeownerApplicationId.Create(applicationId)),
            cancellationToken);

        return Results.Ok(MapToResponse(view));
    }

    [WolverinePost("/api/homeowner-applications/{applicationId}/eligibility/approve")]
    public static async Task<IResult> ApproveEligibility(
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
            new ApproveEligibility(
                HomeownerApplicationId.Create(applicationId),
                DateOnly.Parse(request.ApprovedDate, CultureInfo.InvariantCulture),
                request.Notes,
                userId),
            cancellationToken);

        return Results.NoContent();
    }

    [WolverinePost("/api/homeowner-applications/{applicationId}/eligibility/deny")]
    public static async Task<IResult> DenyEligibility(
        string applicationId,
        DenyEligibilityRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationId);
        ArgumentNullException.ThrowIfNull(request);

        var userId = httpContext.GetUserId();

        await bus.InvokeForTenantAsync(bus.TenantId!,
            new DenyEligibility(
                HomeownerApplicationId.Create(applicationId),
                request.Reason,
                DateOnly.Parse(request.DeniedDate, CultureInfo.InvariantCulture),
                userId),
            cancellationToken);

        return Results.NoContent();
    }

    [WolverinePost("/api/homeowner-applications/{applicationId}/housing-counseling")]
    public static async Task<IResult> RecordHousingCounseling(
        string applicationId,
        RecordHousingCounselingRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationId);
        ArgumentNullException.ThrowIfNull(request);

        var userId = httpContext.GetUserId();

        await bus.InvokeForTenantAsync(bus.TenantId!,
            new RecordHousingCounseling(
                HomeownerApplicationId.Create(applicationId),
                request.CertificationNumber,
                DateOnly.Parse(request.CompletedDate, CultureInfo.InvariantCulture),
                request.CounselorOrganization,
                userId),
            cancellationToken);

        return Results.NoContent();
    }

    [WolverinePost("/api/homeowner-applications/{applicationId}/clt-orientation")]
    public static async Task<IResult> RecordCltOrientation(
        string applicationId,
        RecordCltOrientationRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationId);
        ArgumentNullException.ThrowIfNull(request);

        var userId = httpContext.GetUserId();

        await bus.InvokeForTenantAsync(bus.TenantId!,
            new RecordCltOrientation(
                HomeownerApplicationId.Create(applicationId),
                DateOnly.Parse(request.CompletedDate, CultureInfo.InvariantCulture),
                userId),
            cancellationToken);

        return Results.NoContent();
    }

    [WolverinePost("/api/homeowner-applications/{applicationId}/mortgage-prequalification")]
    public static async Task<IResult> RecordMortgagePrequalification(
        string applicationId,
        RecordMortgagePrequalificationRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationId);
        ArgumentNullException.ThrowIfNull(request);

        var userId = httpContext.GetUserId();

        await bus.InvokeForTenantAsync(bus.TenantId!,
            new RecordMortgagePrequalification(
                HomeownerApplicationId.Create(applicationId),
                request.LenderName,
                request.ApprovedAmount,
                DateOnly.Parse(request.ExpiryDate, CultureInfo.InvariantCulture),
                userId),
            cancellationToken);

        return Results.NoContent();
    }

    [WolverineGet("/api/homes/{homeId}/applicant-candidates")]
    public static async Task<IResult> GetHomeCandidates(
        string homeId,
        [FromQuery] string? search,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(homeId);

        var candidates = await bus.InvokeForTenantAsync<IReadOnlyList<HomeownerApplicationListItemView>>(bus.TenantId!,
            new GetHomeCandidates(homeId, search),
            cancellationToken);

        var response = candidates.Select(v => new HomeCandidateResponse(
            v.Id,
            v.ApplicantFullName,
            v.ApplicantEmail,
            v.ApplicantPhone));

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
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationId);
        ArgumentNullException.ThrowIfNull(request);

        await bus.InvokeForTenantAsync(bus.TenantId!,
            new WithdrawApplication(
                HomeownerApplicationId.Create(applicationId),
                DateOnly.Parse(request.WithdrawnDate, CultureInfo.InvariantCulture),
                request.Reason),
            cancellationToken);

        return Results.NoContent();
    }

    private static HomeownerApplicationResponse MapToResponse(HomeownerApplicationView view)
    {
        var mortgagePrequalification = view.MortgagePrequalification is null ? null
            : new MortgagePrequalificationDto(
                view.MortgagePrequalification.LenderName,
                view.MortgagePrequalification.ApprovedAmount,
                view.MortgagePrequalification.ExpiryDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));

        return new HomeownerApplicationResponse(
            view.Id,
            view.ApplicantFullName,
            view.ApplicantEmail,
            view.ApplicantPhone,
            view.HouseholdSize,
            view.AnnualIncome,
            view.AccessibilityRequired,
            view.Phase,
            view.SubmittedAt.ToString("O", CultureInfo.InvariantCulture),
            view.SubmittedBy,
            view.EligibilityApprovedDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            view.IsHousingCounselingCompleted,
            view.IsCltOrientationCompleted,
            mortgagePrequalification,
            view.DenialReason,
            view.WithdrawalReason,
            view.AddedToWaitingListDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            view.SelectedDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            view.SelectedHomeId,
            view.SelectedLandParcelId,
            view.SelectedBy);
    }
}
