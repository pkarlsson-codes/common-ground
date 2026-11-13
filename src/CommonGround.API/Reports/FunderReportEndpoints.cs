using CommonGround.Homeownership.Application.Queries;
using CommonGround.PropertyRegistry.Application.Queries;
using CommonGround.SharedKernel.Domain.Services;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Wolverine.Http;

namespace CommonGround.API.Reports;

/// <summary>
/// The funder report: homes on the books, sales with the buyer's income band, and buybacks for a period. Worked out
/// from PropertyRegistry's homes and Homeownership's leases and resales each time it is asked for.
/// </summary>
public static class FunderReportEndpoints
{
    [WolverineGet("/api/reports/funder")]
    public static async Task<IResult> GetFunderReport(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        IMessageBus bus,
        ITrustClock trustClock,
        CancellationToken cancellationToken)
    {
        if (from is not { } first || to is not { } last)
        {
            return Results.BadRequest("Both 'from' and 'to' are required.");
        }

        if (last < first)
        {
            return Results.BadRequest("'to' must be on or after 'from'.");
        }

        var tenantId = bus.TenantId!;
        var timeZone = await trustClock.GetTimeZoneAsync(tenantId, cancellationToken).ConfigureAwait(false);

        var registrations = await bus.InvokeForTenantAsync<HomeRegistrations>(tenantId, new GetHomeRegistrations(), cancellationToken).ConfigureAwait(false);
        var records = await bus.InvokeForTenantAsync<FunderReportRecords>(tenantId, new GetFunderReportRecords(first, last), cancellationToken).ConfigureAwait(false);

        var registeredOn = registrations.Homes.ToDictionary(h => h.HomeId, h => timeZone.ToDate(h.RegisteredAt), StringComparer.Ordinal);

        return Results.Ok(FunderReportBuilder.Build(first, last, registeredOn, records));
    }
}
