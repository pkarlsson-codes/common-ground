namespace CommonGround.Documents.Infrastructure.BlobStorage;

/// <summary>Configuration for the file system document blob store.</summary>
public sealed class FileSystemBlobStoreOptions
{
    /// <summary>The configuration section name.</summary>
    public const string SectionName = "Documents:FileSystem";

    /// <summary>Gets or sets the directory that holds document files. A relative path is resolved against the content root.</summary>
    public string RootPath { get; set; } = "data/documents";
}
