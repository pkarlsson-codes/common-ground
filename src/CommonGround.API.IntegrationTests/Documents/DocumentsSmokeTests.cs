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
public class DocumentsSmokeTests(ApiFactory factory)
{
    [Fact]
    public async Task Upload_Attach_List_Download_RoundTrip()
    {
        var client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        }).WithAuthToken();

        var entityId = Guid.NewGuid().ToString();

        // 1. Multipart upload
        using var form = new MultipartFormDataContent();
        var meta = new UploadDocumentMetadata("Test deed", "Internal", "Permanent", null);
        form.Add(JsonContent.Create(meta), "metadata");
        var fileBytes = Encoding.UTF8.GetBytes("hello document");
        var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
        form.Add(fileContent, "file", "deed.txt");

        var uploadResp = await client.PostAsync("/api/documents", form);
        uploadResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var upload = await uploadResp.Content.ReadFromJsonAsync<UploadDocumentResponse>();
        upload!.DocumentId.Should().StartWith("doc-");

        // 2. Attach to a fake entity
        var attachResp = await client.PostAsJsonAsync(
            $"/api/documents/{upload.DocumentId}/attachments",
            new AttachDocumentRequest("Building", entityId, "primary"));
        attachResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // 3. List per-entity
        var listResp = await client.GetAsync($"/api/entities/Building/{entityId}/documents?page.number=1&page.size=10");
        listResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var list = await listResp.Content.ReadFromJsonAsync<PaginatedResponse<EntityDocumentItemResponse>>();
        list!.Items.Should().ContainSingle(i => i.DocumentId == upload.DocumentId)
            .Which.Title.Should().Be("Test deed");

        // 4. Download — expect 302 to the (fake) presigned URL
        var dl = await client.GetAsync($"/api/documents/{upload.DocumentId}/download");
        dl.StatusCode.Should().Be(HttpStatusCode.Redirect);
        dl.Headers.Location.Should().NotBeNull();
        dl.Headers.Location!.ToString().Should().StartWith("https://fake-minio.local/");
    }

    [Fact]
    public async Task Download_WithVersionId_RedirectsToThatVersion()
    {
        var client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        }).WithAuthToken();

        using var form = new MultipartFormDataContent();
        form.Add(JsonContent.Create(new UploadDocumentMetadata("Versioned deed", "Internal", "Permanent", null)), "metadata");
        form.Add(TextFile("first"), "file", "deed-v1.txt");
        var uploadResp = await client.PostAsync("/api/documents", form);
        uploadResp.EnsureSuccessStatusCode();
        var upload = await uploadResp.Content.ReadFromJsonAsync<UploadDocumentResponse>();
        var documentId = upload!.DocumentId;

        var firstVersionId = (await client.GetFromJsonAsync<DocumentResponse>($"/api/documents/{documentId}"))!.CurrentVersionId;

        using var versionForm = new MultipartFormDataContent();
        versionForm.Add(TextFile("second"), "file", "deed-v2.txt");
        var versionResp = await client.PostAsync($"/api/documents/{documentId}/versions", versionForm);
        versionResp.EnsureSuccessStatusCode();
        var secondVersionId = (await versionResp.Content.ReadFromJsonAsync<AddDocumentVersionResponse>())!.VersionId;

        var current = await client.GetAsync($"/api/documents/{documentId}/download");
        var first = await client.GetAsync($"/api/documents/{documentId}/download?versionId={firstVersionId}");
        var unknown = await client.GetAsync($"/api/documents/{documentId}/download?versionId=ver-missing");

        current.Headers.Location!.ToString().Should().Contain(Uri.EscapeDataString(secondVersionId));
        first.StatusCode.Should().Be(HttpStatusCode.Redirect);
        first.Headers.Location!.ToString().Should().Contain(Uri.EscapeDataString(firstVersionId));
        unknown.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private static ByteArrayContent TextFile(string text)
    {
        var content = new ByteArrayContent(Encoding.UTF8.GetBytes(text));
        content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
        return content;
    }
}
