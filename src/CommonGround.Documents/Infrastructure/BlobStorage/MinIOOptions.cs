namespace CommonGround.Documents.Infrastructure.BlobStorage;

/// <summary>Configuration for the MinIO-backed document blob store.</summary>
public sealed class MinIOOptions
{
    /// <summary>The configuration section name.</summary>
    public const string SectionName = "MinIO";

    /// <summary>Gets or sets the MinIO endpoint host:port (no scheme, e.g. "localhost:9000").</summary>
    public string Endpoint { get; set; } = "localhost:9000";

    /// <summary>Gets or sets the access key.</summary>
    public string AccessKey { get; set; } = "minioadmin";

    /// <summary>Gets or sets the secret key.</summary>
    public string SecretKey { get; set; } = "minioadmin";

    /// <summary>Gets or sets a value indicating whether HTTPS is used.</summary>
    public bool UseSsl { get; set; }

    /// <summary>Gets or sets the bucket where documents are stored.</summary>
    public string Bucket { get; set; } = "commonground-documents";
}
