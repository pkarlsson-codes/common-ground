namespace CommonGround.API.Clients;

/// <summary>The API's answer to a download: a URL to redirect to, or the response that carries the file.</summary>
public sealed class DocumentDownloadResult : IDisposable
{
    private DocumentDownloadResult(Uri? redirectUrl, HttpResponseMessage? fileResponse)
    {
        RedirectUrl = redirectUrl;
        FileResponse = fileResponse;
    }

    /// <summary>Gets the presigned URL to send the browser to, or null when <see cref="FileResponse"/> is set.</summary>
    public Uri? RedirectUrl { get; }

    /// <summary>Gets the API response whose body and headers are the file, or null when <see cref="RedirectUrl"/> is set.</summary>
    public HttpResponseMessage? FileResponse { get; }

    /// <summary>Creates a redirect result.</summary>
    /// <param name="url">The presigned URL.</param>
    /// <returns>The result.</returns>
    public static DocumentDownloadResult RedirectTo(Uri url) => new(url, null);

    /// <summary>Creates a result that owns <paramref name="response"/>.</summary>
    /// <param name="response">The API response that carries the file.</param>
    /// <returns>The result.</returns>
    public static DocumentDownloadResult FileIn(HttpResponseMessage response) => new(null, response);

    /// <inheritdoc/>
    public void Dispose() => FileResponse?.Dispose();
}
