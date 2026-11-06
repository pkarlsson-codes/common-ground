namespace CommonGround.API.Infrastructure.Seeding;

/// <summary>
/// Refuses requests, health checks included, while the development seeders run. Seeders move the
/// application's clock back in time, so a request served meanwhile would see a past "today", and the
/// data would be half seeded. Callers that wait for <c>/health</c> (e2e, the gateway) wait for seeding too.
/// </summary>
internal sealed class SeedingGate
{
    private volatile bool _seeding;

    /// <summary>Closes the gate in Development, where the seeders run, and refuses requests until it opens.</summary>
    /// <param name="app">The web application, before its other middleware is added.</param>
    public static void Use(WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        if (!app.Environment.IsDevelopment())
        {
            return;
        }

        var gate = app.Services.GetRequiredService<SeedingGate>();
        gate._seeding = true;

        app.Use(async (context, next) =>
        {
            if (gate._seeding)
            {
                context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                context.Response.Headers.RetryAfter = "5";
                await context.Response.WriteAsync("Development data is still being seeded.").ConfigureAwait(false);
                return;
            }

            await next(context).ConfigureAwait(false);
        });
    }

    /// <summary>Lets requests through once seeding is over.</summary>
    public void Open() => _seeding = false;
}
