using CommonGround.API.Contracts.PortfolioImport;
using CommonGround.API.Infrastructure.Auth;
using CommonGround.API.Infrastructure.ExceptionHandling;
using CommonGround.Homeownership.Application.Queries;
using CommonGround.PropertyRegistry.Application.Queries;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.SharedKernel.Domain.Services;
using Wolverine;
using Wolverine.Http;

namespace CommonGround.API.PortfolioImport;

/// <summary>Imports a trust's existing land parcels, homes and ground leases from one CSV file.</summary>
public static class PortfolioImportEndpoints
{
    // Checks a file and says what importing it would create. Changes nothing.
    [WolverinePost("/api/portfolio-import/check")]
    public static async Task<IResult> CheckPortfolioImport(
        PortfolioImportRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        ITrustClock trustClock,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var plan = await PlanAsync(request, httpContext, bus, trustClock, cancellationToken).ConfigureAwait(false);

        return Results.Ok(ToResponse(plan, imported: false));
    }

    // Checks a file again and, when no row has an error, creates its parcels, homes and leases in row order.
    // Returns 422 with the errors when any row has one, or with the row it stopped at when a command fails.
    [WolverinePost("/api/portfolio-import")]
    public static async Task<IResult> ImportPortfolio(
        PortfolioImportRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        ITrustClock trustClock,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(bus);

        var plan = await PlanAsync(request, httpContext, bus, trustClock, cancellationToken).ConfigureAwait(false);
        if (plan.Errors.Count > 0)
        {
            return Results.UnprocessableEntity(ToResponse(plan, imported: false));
        }

        foreach (var step in plan.Steps)
        {
            try
            {
                await bus.InvokeForTenantAsync(bus.TenantId!, step.Command, cancellationToken).ConfigureAwait(false);
            }
            catch (DomainException ex)
            {
                return StoppedAt(plan, step.Row, DomainExceptionHandler.GetUserFacingMessage(ex));
            }
        }

        return Results.Ok(ToResponse(plan, imported: true));
    }

    private static async Task<PortfolioImportPlan> PlanAsync(
        PortfolioImportRequest request,
        HttpContext httpContext,
        IMessageBus bus,
        ITrustClock trustClock,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(trustClock);

        var tenantId = bus.TenantId!;
        var userId = httpContext.GetUserId();
        var today = await trustClock.TodayAsync(tenantId, cancellationToken).ConfigureAwait(false);
        var properties = await bus.InvokeForTenantAsync<RegisteredProperties>(tenantId, new GetRegisteredProperties(), cancellationToken).ConfigureAwait(false);
        var leasing = await bus.InvokeForTenantAsync<LeasingForImport>(tenantId, new GetLeasingForImport(), cancellationToken).ConfigureAwait(false);

        return PortfolioImportPlanner.Plan(CsvTable.Parse(request.Csv ?? string.Empty), properties, leasing, today, userId);
    }

    private static IResult StoppedAt(PortfolioImportPlan plan, int row, string message)
    {
        var response = ToResponse(plan, imported: false) with
        {
            Errors = [new PortfolioImportErrorDto(row, null, message)],
            StoppedAtRow = row,
        };

        return Results.UnprocessableEntity(response);
    }

    private static PortfolioImportResponse ToResponse(PortfolioImportPlan plan, bool imported) => new(
        [.. plan.Errors.Select(e => new PortfolioImportErrorDto(e.Row, e.Column, e.Message))],
        plan.ParcelsToCreate,
        plan.ParcelsExisting,
        plan.HomesToCreate,
        plan.HomesExisting,
        plan.LeasesToCreate,
        plan.LeasesExisting,
        imported);
}
