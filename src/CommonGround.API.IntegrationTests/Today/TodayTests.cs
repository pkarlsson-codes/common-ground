using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using CommonGround.API.Contracts.Documents.Requests;
using CommonGround.API.Contracts.Documents.Responses;
using CommonGround.API.Contracts.HomeBuyers.Requests;
using CommonGround.API.Contracts.HomeBuyers.Responses;
using CommonGround.API.Contracts.Today.Responses;
using FluentAssertions;

namespace CommonGround.API.IntegrationTests.Today;

/// <summary>
/// Covers the Today queue: items appear when a record starts waiting on staff, leave when staff act on it,
/// and can be narrowed to some areas without changing the counts.
/// </summary>
[Collection(IntegrationCollection.Name)]
public class TodayTests(ApiFactory factory)
{
    [Fact]
    public async Task GetToday_ListsASubmittedApplicationWithItsDecisionsUntilItIsApproved()
    {
        var client = factory.CreateClient().WithAuthToken();
        var applicationId = await SubmitAsync(client, await RegisterHouseholdAsync(client, "today-submitted@example.com"));

        var before = await GetTodayAsync(client);

        before.Items.Should().ContainSingle(i => i.RecordId == applicationId)
            .Which.Should().BeEquivalentTo(new { Area = "applicants", Kind = "application-awaiting-decision", Label = "Today Applicant", Actions = new[] { "approve-application", "deny-application" } });

        (await client.PostAsJsonAsync($"/api/homeowner-applications/{applicationId}/approve",
            new ApproveApplicationRequest(before.Today, null)))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        var after = await GetTodayAsync(client);

        after.Items.Should().NotContain(i => i.RecordId == applicationId);
    }

    [Fact]
    public async Task GetToday_ListsADocumentOnlyOnceItExpiresWithinThirtyDays()
    {
        var client = factory.CreateClient().WithAuthToken();
        var today = (await GetTodayAsync(client)).Today;
        var soon = await UploadAsync(client, "Today expiring soon");
        var later = await UploadAsync(client, "Today expiring later");
        await PatchAsync(client, soon, new UpdateDocumentMetadataRequest(ExpiresOn: today.AddDays(3)));
        await PatchAsync(client, later, new UpdateDocumentMetadataRequest(ExpiresOn: today.AddDays(31)));

        var result = await GetTodayAsync(client);

        result.Items.Should().ContainSingle(i => i.RecordId == soon)
            .Which.Should().BeEquivalentTo(new { Area = "records", DueDate = (DateOnly?)today.AddDays(3), Urgency = "soon" });
        result.Items.Should().NotContain(i => i.RecordId == later);
    }

    [Fact]
    public async Task GetToday_WithAreas_ListsOnlyThoseAreasButCountsEveryArea()
    {
        var client = factory.CreateClient().WithAuthToken();
        await SubmitAsync(client, await RegisterHouseholdAsync(client, "today-filtered@example.com"));
        var everything = await GetTodayAsync(client);

        var applicants = await GetTodayAsync(client, "?areas=applicants&areas=records");

        applicants.Items.Should().NotBeEmpty().And.OnlyContain(i => i.Area == "applicants" || i.Area == "records");
        applicants.Items.Count.Should().Be(everything.Items.Count(i => i.Area == "applicants" || i.Area == "records"));
        applicants.AreaCounts.Should().BeEquivalentTo(everything.AreaCounts);
        applicants.TotalCount.Should().Be(everything.Items.Count);
    }

    private static async Task<TodayResponse> GetTodayAsync(HttpClient client, string query = "") =>
        (await client.GetFromJsonAsync<TodayResponse>($"/api/today{query}"))!;

    private static async Task<string> RegisterHouseholdAsync(HttpClient client, string email)
    {
        var response = await client.PostAsJsonAsync("/api/households",
            new RegisterHouseholdRequest(
                [new HouseholdMemberRequest("Today Applicant", email, "(202) 555-0100", true)],
                false,
                65000m));
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<RegisterHouseholdResponse>())!.HouseholdId;
    }

    private static async Task<string> SubmitAsync(HttpClient client, string householdId)
    {
        var response = await client.PostAsJsonAsync("/api/homeowner-applications", new SubmitApplicationRequest(householdId));
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<SubmitApplicationResponse>())!.ApplicationId;
    }

    private static async Task<string> UploadAsync(HttpClient client, string title)
    {
        using var form = new MultipartFormDataContent();
        form.Add(JsonContent.Create(new UploadDocumentMetadata(title, "Internal", "Permanent", null)), "metadata");
        using var file = new ByteArrayContent(Encoding.UTF8.GetBytes(title));
        file.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
        form.Add(file, "file", "doc.txt");
        var response = await client.PostAsync("/api/documents", form);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<UploadDocumentResponse>())!.DocumentId;
    }

    private static async Task PatchAsync(HttpClient client, string documentId, UpdateDocumentMetadataRequest request)
    {
        var response = await client.PatchAsJsonAsync($"/api/documents/{documentId}", request);
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
