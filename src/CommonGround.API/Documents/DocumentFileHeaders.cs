using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace CommonGround.API.Documents;

/// <summary>Response headers for a document file the API serves itself (the file system store).</summary>
/// <remarks>
/// The file reaches the browser from the app's own origin, so uploaded HTML or SVG must never run there.
/// Types a browser shows without running script open in the tab; everything else downloads.
/// The gateway copies these headers onto its response.
/// </remarks>
public static class DocumentFileHeaders
{
    private static readonly HashSet<string> InlineTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "image/png",
        "image/jpeg",
        "image/gif",
        "image/webp",
        "text/plain",
    };

    /// <summary>Sets Content-Disposition, nosniff and a sandbox CSP for <paramref name="fileName"/>.</summary>
    /// <param name="headers">The response headers to set.</param>
    /// <param name="fileName">The file name the user uploaded.</param>
    /// <param name="contentType">The content type recorded at upload.</param>
    public static void Apply(IHeaderDictionary headers, string fileName, string contentType)
    {
        ArgumentNullException.ThrowIfNull(headers);

        var disposition = new ContentDispositionHeaderValue(InlineTypes.Contains(contentType) ? "inline" : "attachment");
        disposition.SetHttpFileName(fileName);
        headers.ContentDisposition = disposition.ToString();
        headers.XContentTypeOptions = "nosniff";
        headers.ContentSecurityPolicy = "sandbox";
    }
}
