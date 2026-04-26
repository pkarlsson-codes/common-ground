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

        var entityId = Guid.NewGuid();

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
        var listResp = await client.GetAsync($"/api/entities/Building/{entityId:N}/documents?page.number=1&page.size=10");
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
}
