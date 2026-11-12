using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CommonGround.API.Contracts.HomeBuyers.Requests;
using CommonGround.API.Contracts.HomeBuyers.Responses;
using FluentAssertions;

namespace CommonGround.API.IntegrationTests.HomeBuyers;

/// <summary>
/// Covers entering, correcting and listing income limit schedules. Every test shares the test trust, and a
/// trust can't have two schedules on one date, so each test picks its own far-future date; only
/// <see cref="GetIncomeLimitSchedules_MarksTheLatestScheduleEffectiveOnOrBeforeTodayAsInEffect"/> enters past dates.
/// </summary>
[Collection(IntegrationCollection.Name)]
public class IncomeLimitScheduleTests(ApiFactory factory)
{
    private static readonly decimal[] EightyPercentLimits = [62_050, 70_900, 79_750, 88_600, 95_700, 102_800, 109_900, 116_950];
    private static readonly decimal[] FiftyPercentLimits = [38_800, 44_300, 49_850, 55_400, 59_850, 64_250, 68_700, 73_150];

    [Fact]
    public async Task EnterIncomeLimitSchedule_ThenList_ShowsItsLevelsLowestPercentFirst()
    {
        var client = factory.CreateClient().WithAuthToken();
        var date = FutureDate();

        var response = await client.PostAsJsonAsync("/api/income-limit-schedules",
            new IncomeLimitScheduleRequest(date, [new(80, EightyPercentLimits), new(50, FiftyPercentLimits)]));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var id = (await response.Content.ReadFromJsonAsync<EnterIncomeLimitScheduleResponse>())!.ScheduleId;
        var schedule = await GetScheduleAsync(client, id);
        schedule.Should().BeEquivalentTo(new
        {
            EffectiveFrom = date,
            IsInEffect = false,
            Levels = new[] { new { PercentOfAmi = 50, Limits = FiftyPercentLimits }, new { PercentOfAmi = 80, Limits = EightyPercentLimits } },
            CorrectedBy = (string?)null,
        }, o => o.WithStrictOrdering());
    }

    [Fact]
    public async Task EnterIncomeLimitSchedule_OnADateAnotherScheduleHas_Returns422()
    {
        var client = factory.CreateClient().WithAuthToken();
        var date = FutureDate();
        await EnterAsync(client, date);

        var response = await client.PostAsJsonAsync("/api/income-limit-schedules",
            new IncomeLimitScheduleRequest(date, [new(80, EightyPercentLimits)]));

        await ShouldBeViolationAsync(response, "home_buyers.income_limit_schedule.effective_from.taken");
    }

    [Fact]
    public async Task EnterIncomeLimitSchedule_WithSevenLimits_Returns422()
    {
        var client = factory.CreateClient().WithAuthToken();

        var response = await client.PostAsJsonAsync("/api/income-limit-schedules",
            new IncomeLimitScheduleRequest(FutureDate(), [new(80, EightyPercentLimits[..7])]));

        await ShouldBeViolationAsync(response, "home_buyers.income_limit_schedule.level.wrong_number_of_limits");
    }

    [Fact]
    public async Task CorrectIncomeLimitSchedule_ReplacesTheDateAndLevels()
    {
        var client = factory.CreateClient().WithAuthToken();
        var id = await EnterAsync(client, FutureDate());
        var correctedDate = FutureDate();
        decimal[] corrected = [.. EightyPercentLimits.Select(l => l + 100)];

        var response = await client.PutAsJsonAsync($"/api/income-limit-schedules/{id}",
            new IncomeLimitScheduleRequest(correctedDate, [new(80, corrected)]));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var schedule = await GetScheduleAsync(client, id);
        schedule.Should().BeEquivalentTo(new
        {
            EffectiveFrom = correctedDate,
            Levels = new[] { new { PercentOfAmi = 80, Limits = corrected } },
        }, o => o.WithStrictOrdering());
        schedule.CorrectedBy.Should().NotBeNull();
    }

    [Fact]
    public async Task CorrectIncomeLimitSchedule_ThatDoesNotExist_Returns404()
    {
        var client = factory.CreateClient().WithAuthToken();

        var response = await client.PutAsJsonAsync("/api/income-limit-schedules/incomelimitschedule-missing",
            new IncomeLimitScheduleRequest(FutureDate(), [new(80, EightyPercentLimits)]));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetIncomeLimitSchedules_MarksTheLatestScheduleEffectiveOnOrBeforeTodayAsInEffect()
    {
        var client = factory.CreateClient().WithAuthToken();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var older = await EnterAsync(client, today.AddYears(-2));
        var current = await EnterAsync(client, today.AddYears(-1));
        var upcoming = await EnterAsync(client, FutureDate());

        var schedules = await client.GetFromJsonAsync<List<IncomeLimitScheduleResponse>>("/api/income-limit-schedules");

        schedules!.Where(s => s.IsInEffect).Should().ContainSingle().Which.Id.Should().Be(current);
        schedules.Select(s => s.Id).Should().Contain([older, upcoming]);
        schedules.Select(s => s.EffectiveFrom).Should().BeInDescendingOrder();
    }

    // Far enough ahead that no test's date is ever today or earlier; random so tests sharing the trust don't collide.
    private static DateOnly FutureDate() => new DateOnly(2200, 1, 1).AddDays(Random.Shared.Next(0, 2_000_000));

    private static async Task<string> EnterAsync(HttpClient client, DateOnly effectiveFrom)
    {
        var response = await client.PostAsJsonAsync("/api/income-limit-schedules",
            new IncomeLimitScheduleRequest(effectiveFrom, [new(80, EightyPercentLimits)]));
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<EnterIncomeLimitScheduleResponse>())!.ScheduleId;
    }

    private static async Task<IncomeLimitScheduleResponse> GetScheduleAsync(HttpClient client, string id)
    {
        var schedules = await client.GetFromJsonAsync<List<IncomeLimitScheduleResponse>>("/api/income-limit-schedules");
        return schedules!.Single(s => s.Id == id);
    }

    private static async Task ShouldBeViolationAsync(HttpResponseMessage response, string code)
    {
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString().Should().Be(code);
    }
}
