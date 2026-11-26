using System.Net;
using System.Net.Http.Json;
using CommonGround.API.Contracts.Reminders.Requests;
using CommonGround.API.Contracts.Reminders.Responses;
using CommonGround.API.Contracts.Today.Responses;
using CommonGround.Reminders.Application.ReadModels;
using CommonGround.SharedKernel.Domain;
using FluentAssertions;
using Marten;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace CommonGround.API.IntegrationTests.Reminders;

/// <summary>
/// Covers reminders: set for a day, they show on Today from that day in their section whatever areas are asked for,
/// every member of the trust sees them, and marking one done takes it off Today.
/// </summary>
[Collection(IntegrationCollection.Name)]
public class RemindersTests(ApiFactory factory)
{
    [Fact]
    public async Task SetReminder_ForALaterDay_IsUpcomingForAColleagueAndNotOnToday()
    {
        var client = factory.CreateClient().WithAuthToken();
        var today = (await GetTodayAsync(client)).Today;
        var setterId = UserId.Generate().Value;
        var setter = factory.CreateClient().WithAuthToken(setterId);

        var id = await SetAsync(setter, new SetReminderRequest(today.AddDays(3), "  call the lender about the rate lock ", "stewardship"));

        var list = await ListAsync(client);
        list.Upcoming.Should().ContainSingle(r => r.Id == id).Which.Should().BeEquivalentTo(new
        {
            RemindOn = today.AddDays(3),
            Note = "call the lender about the rate lock",
            Section = "stewardship",
            SetBy = setterId,
            IsDone = false,
        });
        list.Showing.Should().NotContain(r => r.Id == id);
        (await GetTodayAsync(client)).Items.Should().NotContain(i => i.RecordId == id);
    }

    [Fact]
    public async Task SetReminder_ForToday_ShowsInItsSectionEvenWhenOtherAreasAreAsked()
    {
        var client = factory.CreateClient().WithAuthToken();
        var today = (await GetTodayAsync(client)).Today;

        var id = await SetAsync(client, new SetReminderRequest(today, "bring the default to the board", "stewardship"));

        var applicantsOnly = await GetTodayAsync(client, "?areas=applicants");
        applicantsOnly.Items.Should().ContainSingle(i => i.RecordId == id).Which.Should().BeEquivalentTo(new
        {
            Area = "stewardship",
            Kind = "reminder",
            RecordType = "reminder",
            Label = "bring the default to the board",
            DueDate = (DateOnly?)null,
            WaitingSince = (DateOnly?)today,
            Actions = new[] { "mark-reminder-done" },
            Urgency = "plain",
        });
        (await ListAsync(client)).Showing.Should().Contain(r => r.Id == id);
    }

    [Fact]
    public async Task SetReminder_OnNoSection_ShowsInTheRemindersArea()
    {
        var client = factory.CreateClient().WithAuthToken();
        var today = (await GetTodayAsync(client)).Today;

        var id = await SetAsync(client, new SetReminderRequest(today, "order more lease binders", null));

        var result = await GetTodayAsync(client);
        result.Items.Should().ContainSingle(i => i.RecordId == id).Which.Area.Should().Be("reminders");
        result.AreaCounts.Should().Contain(c => c.Key == "reminders" && c.Count > 0);
    }

    [Fact]
    public async Task GetToday_ListsAReminderWhoseDayHasPassedAndThatIsNotDone()
    {
        var client = factory.CreateClient().WithAuthToken();
        var today = (await GetTodayAsync(client)).Today;
        var id = $"rem-{Guid.NewGuid():N}";
        using (var scope = factory.Services.CreateScope())
        {
            await using var session = scope.ServiceProvider.GetRequiredService<IDocumentStore>().LightweightSession(ApiFactory.TestTrustId);
            session.Store(new ReminderView { Id = id, RemindOn = today.AddDays(-4), Note = "chase the appraisal", Section = "purchases", SetBy = UserId.Generate().Value });
            await session.SaveChangesAsync();
        }

        var result = await GetTodayAsync(client);

        result.Items.Should().ContainSingle(i => i.RecordId == id)
            .Which.Should().BeEquivalentTo(new { Area = "purchases", WaitingSince = (DateOnly?)today.AddDays(-4), Urgency = "plain" });
    }

    [Fact]
    public async Task MarkReminderDone_TakesItOffTodayAndListsItAsDoneByWhoeverMarkedIt()
    {
        var client = factory.CreateClient().WithAuthToken();
        var today = (await GetTodayAsync(client)).Today;
        var id = await SetAsync(client, new SetReminderRequest(today, "send the board packet", null));
        var colleagueId = UserId.Generate().Value;
        var colleague = factory.CreateClient().WithAuthToken(colleagueId);

        (await colleague.PostAsync($"/api/reminders/{id}/done", null)).StatusCode.Should().Be(HttpStatusCode.NoContent);

        (await GetTodayAsync(client)).Items.Should().NotContain(i => i.RecordId == id);
        var list = await ListAsync(client);
        list.Showing.Should().NotContain(r => r.Id == id);
        list.Done.Should().ContainSingle(r => r.Id == id).Which.Should().Match<ReminderResponse>(r =>
            r.IsDone && r.DoneBy == colleagueId && r.DoneAt != null);
    }

    [Fact]
    public async Task MarkReminderDone_WhenAlreadyDone_ChangesNothing()
    {
        var client = factory.CreateClient().WithAuthToken();
        var today = (await GetTodayAsync(client)).Today;
        var id = await SetAsync(client, new SetReminderRequest(today, "file the insurance letter", "records"));
        (await client.PostAsync($"/api/reminders/{id}/done", null)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        var doneAt = (await ListAsync(client)).Done.Single(r => r.Id == id).DoneAt;

        (await client.PostAsync($"/api/reminders/{id}/done", null)).StatusCode.Should().Be(HttpStatusCode.NoContent);

        (await ListAsync(client)).Done.Single(r => r.Id == id).DoneAt.Should().Be(doneAt);
    }

    [Fact]
    public async Task MarkReminderDone_ForAReminderThatDoesNotExist_IsNotFound()
    {
        var client = factory.CreateClient().WithAuthToken();

        (await client.PostAsync($"/api/reminders/rem-{Guid.NewGuid():N}/done", null)).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SetReminder_ForADayBeforeToday_IsRefused()
    {
        var client = factory.CreateClient().WithAuthToken();
        var today = (await GetTodayAsync(client)).Today;

        var response = await client.PostAsJsonAsync("/api/reminders", new SetReminderRequest(today.AddDays(-1), "too late", null));

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await response.Content.ReadFromJsonAsync<ProblemDetails>())!.Detail.Should().StartWith("A reminder can't be set for");
    }

    [Fact]
    public async Task SetReminder_WithoutANote_IsRefused()
    {
        var client = factory.CreateClient().WithAuthToken();
        var today = (await GetTodayAsync(client)).Today;

        var response = await client.PostAsJsonAsync("/api/reminders", new SetReminderRequest(today, "  ", null));

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await response.Content.ReadFromJsonAsync<ProblemDetails>())!.Detail.Should().Be("Write a note saying what to do.");
    }

    private static async Task<string> SetAsync(HttpClient client, SetReminderRequest request)
    {
        var response = await client.PostAsJsonAsync("/api/reminders", request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<SetReminderResponse>())!.ReminderId;
    }

    private static async Task<ReminderListResponse> ListAsync(HttpClient client) =>
        (await client.GetFromJsonAsync<ReminderListResponse>("/api/reminders"))!;

    private static async Task<TodayResponse> GetTodayAsync(HttpClient client, string query = "") =>
        (await client.GetFromJsonAsync<TodayResponse>($"/api/today{query}"))!;
}
