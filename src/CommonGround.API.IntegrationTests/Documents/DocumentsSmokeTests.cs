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

        // 4. Download — the store serves the file, so the API streams it
        var dl = await client.GetAsync($"/api/documents/{upload.DocumentId}/download");
        dl.StatusCode.Should().Be(HttpStatusCode.OK);
        (await dl.Content.ReadAsStringAsync()).Should().Be("hello document");
        dl.Content.Headers.ContentType!.MediaType.Should().Be("text/plain");
        dl.Content.Headers.ContentDisposition!.DispositionType.Should().Be("inline");
        dl.Content.Headers.ContentDisposition.FileName.Should().Be("deed.txt");
        dl.Headers.GetValues("X-Content-Type-Options").Should().Equal("nosniff");
        dl.Headers.GetValues("Content-Security-Policy").Should().Equal("sandbox");
    }

    [Fact]
    public async Task Download_OfHtml_IsAnAttachment()
    {
        var client = factory.CreateClient().WithAuthToken();

        using var form = new MultipartFormDataContent();
        form.Add(JsonContent.Create(new UploadDocumentMetadata("Page", "Internal", "Permanent", null)), "metadata");
        var html = new ByteArrayContent(Encoding.UTF8.GetBytes("<script>alert(1)</script>"));
        html.Headers.ContentType = new MediaTypeHeaderValue("text/html");
        form.Add(html, "file", "page.html");
        var upload = await (await client.PostAsync("/api/documents", form)).Content.ReadFromJsonAsync<UploadDocumentResponse>();

        var dl = await client.GetAsync($"/api/documents/{upload!.DocumentId}/download");

        dl.StatusCode.Should().Be(HttpStatusCode.OK);
        dl.Content.Headers.ContentDisposition!.DispositionType.Should().Be("attachment");
    }

    [Fact]
    public async Task Download_WithVersionId_ServesThatVersion()
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

        var current = await client.GetAsync($"/api/documents/{documentId}/download");
        var first = await client.GetAsync($"/api/documents/{documentId}/download?versionId={firstVersionId}");
        var unknown = await client.GetAsync($"/api/documents/{documentId}/download?versionId=ver-missing");

        (await current.Content.ReadAsStringAsync()).Should().Be("second");
        (await first.Content.ReadAsStringAsync()).Should().Be("first");
        unknown.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private static ByteArrayContent TextFile(string text)
    {
        var content = new ByteArrayContent(Encoding.UTF8.GetBytes(text));
        content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
        return content;
    }
}
