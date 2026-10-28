using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using CommonGround.API.Contracts.Documents.Requests;
using CommonGround.API.Contracts.Documents.Responses;
using CommonGround.SharedKernel.Infrastructure.API;
using FluentAssertions;

namespace CommonGround.API.IntegrationTests.Documents;

[Collection(IntegrationCollection.Name)]
public class DocumentLibraryTests(ApiFactory factory)
{
    [Fact]
    public async Task ListDocuments_WithSearch_MatchesTitleCaseInsensitively()
    {
        var client = factory.CreateClient().WithAuthToken();
        var marker = Guid.NewGuid().ToString("N");
        var matchId = await UploadAsync(client, $"Deed {marker} Elm");
        await UploadAsync(client, "Unrelated survey");

        var page = await ListAsync(client, $"search={marker.ToUpperInvariant()}");

        page.Items.Select(i => i.Id).Should().Equal(matchId);
    }

    [Fact]
    public async Task ListDocuments_ExpiringBefore_ListsSoonestExpiryFirst()
    {
        var client = factory.CreateClient().WithAuthToken();
        var marker = Guid.NewGuid().ToString("N");
        var later = await UploadAsync(client, $"Later {marker}");
        var sooner = await UploadAsync(client, $"Sooner {marker}");
        var notExpiring = await UploadAsync(client, $"Never {marker}");
        var now = DateTimeOffset.UtcNow;
        await PatchAsync(client, later, new UpdateDocumentMetadataRequest(ExpiresAt: now.AddDays(20)));
        await PatchAsync(client, sooner, new UpdateDocumentMetadataRequest(ExpiresAt: now.AddDays(-2)));
        _ = notExpiring;

        var cutoff = Uri.EscapeDataString(now.AddDays(90).ToString("O"));
        var page = await ListAsync(client, $"search={marker}&expiringBefore={cutoff}");

        page.Items.Select(i => i.Id).Should().Equal(sooner, later);
    }

    [Fact]
    public async Task ListDocuments_AfterTagging_FiltersByNormalisedTag()
    {
        var client = factory.CreateClient().WithAuthToken();
        var marker = Guid.NewGuid().ToString("N");
        var tagged = await UploadAsync(client, $"Tagged {marker}");
        await UploadAsync(client, $"Untagged {marker}");

        var tagResp = await client.PostAsJsonAsync($"/api/documents/{tagged}/tags", new TagDocumentRequest($" Deed-{marker} "));
        tagResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var page = await ListAsync(client, $"search={marker}&tag=deed-{marker}");

        page.Items.Should().ContainSingle().Which.Tags.Should().Equal($"deed-{marker}");
    }

    [Fact]
    public async Task UpdateDocumentMetadata_UnknownSensitivity_Returns400()
    {
        var client = factory.CreateClient().WithAuthToken();
        var documentId = await UploadAsync(client, "Sensitivity check");

        var response = await client.PatchAsJsonAsync(
            $"/api/documents/{documentId}", new UpdateDocumentMetadataRequest(Sensitivity: "TopSecret"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task TagDocument_BlankTag_Returns400()
    {
        var client = factory.CreateClient().WithAuthToken();
        var documentId = await UploadAsync(client, "Tag check");

        var response = await client.PostAsJsonAsync($"/api/documents/{documentId}/tags", new TagDocumentRequest("  "));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UploadDocument_UnknownRetention_Returns400()
    {
        var client = factory.CreateClient().WithAuthToken();
        using var form = new MultipartFormDataContent();
        form.Add(JsonContent.Create(new UploadDocumentMetadata("Retention check", "Internal", "Forever", null)), "metadata");
        form.Add(TextFile("x"), "file", "x.txt");

        var response = await client.PostAsync("/api/documents", form);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private static async Task<string> UploadAsync(HttpClient client, string title)
    {
        using var form = new MultipartFormDataContent();
        form.Add(JsonContent.Create(new UploadDocumentMetadata(title, "Internal", "Permanent", null)), "metadata");
        form.Add(TextFile(title), "file", "doc.txt");
        var response = await client.PostAsync("/api/documents", form);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<UploadDocumentResponse>())!.DocumentId;
    }

    private static async Task PatchAsync(HttpClient client, string documentId, UpdateDocumentMetadataRequest request)
    {
        var response = await client.PatchAsJsonAsync($"/api/documents/{documentId}", request);
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    private static async Task<PaginatedResponse<DocumentLibraryItemResponse>> ListAsync(HttpClient client, string query) =>
        (await client.GetFromJsonAsync<PaginatedResponse<DocumentLibraryItemResponse>>($"/api/documents?pageSize=100&{query}"))!;

    private static ByteArrayContent TextFile(string text)
    {
        var content = new ByteArrayContent(Encoding.UTF8.GetBytes(text));
        content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
        return content;
    }
}
