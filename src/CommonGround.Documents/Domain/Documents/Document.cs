using CommonGround.Documents.Domain.Documents.Events;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;
using CommonGround.SharedKernel.Domain.Exceptions;

namespace CommonGround.Documents.Domain.Documents;

/// <summary>
/// A document tracked by the CLT — owns its versions, attachments, sensitivity, and retention metadata.
/// </summary>
public sealed class Document : AggregateRoot<DocumentId>
{
    private readonly List<DocumentVersion> _versions = [];
    private readonly List<DocumentAttachment> _attachments = [];

#pragma warning disable CS8618
    private Document() { }
#pragma warning restore CS8618

    /// <summary>Gets the human-readable title of this document.</summary>
    public string Title { get; private set; }

    /// <summary>Gets the sensitivity classification.</summary>
    public SensitivityLevel Sensitivity { get; private set; }

    /// <summary>Gets the retention class.</summary>
    public RetentionClass Retention { get; private set; }

    /// <summary>Gets the ID of the most recent version.</summary>
    public DocumentVersionId CurrentVersionId { get; private set; }

    /// <summary>Gets all versions in upload order (oldest first).</summary>
    public IReadOnlyList<DocumentVersion> Versions => _versions;

    /// <summary>Gets all current attachments. Detached attachments are removed from this collection.</summary>
    public IReadOnlyList<DocumentAttachment> Attachments => _attachments;

    /// <summary>Gets the user who uploaded the original version.</summary>
    public UserId UploadedBy { get; private set; }

    /// <summary>Gets when the original version was uploaded.</summary>
    public DateTimeOffset UploadedAt { get; private set; }

    /// <summary>Gets a value indicating whether this document is soft-deleted.</summary>
    public bool IsDeleted { get; private set; }

    /// <summary>Uploads a brand-new document with its first version, and optionally attaches it to an entity.</summary>
    public static Document Upload(
        string title,
        SensitivityLevel sensitivity,
        RetentionClass retention,
        string blobKey,
        string fileName,
        string contentType,
        long sizeBytes,
        string sha256,
        UserId uploadedBy,
        (string EntityType, Guid EntityId, string? Role)? initialAttachment = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentNullException.ThrowIfNull(sensitivity);
        ArgumentNullException.ThrowIfNull(retention);
        ArgumentException.ThrowIfNullOrWhiteSpace(blobKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);
        ArgumentException.ThrowIfNullOrWhiteSpace(sha256);
        ArgumentNullException.ThrowIfNull(uploadedBy);

        if (sizeBytes <= 0)
            throw new BusinessRuleViolationException("Uploaded blob must have non-zero size.");

        var doc = new Document();
        var now = DateTimeOffset.UtcNow;
        var versionId = DocumentVersionId.Generate();

        doc.ApplyChange(new DocumentUploaded(
            DocumentId.Generate().Value,
            versionId.Value,
            title,
            sensitivity.DisplayName,
            retention.DisplayName,
            blobKey,
            fileName,
            contentType,
            sizeBytes,
            sha256,
            uploadedBy.Value,
            now));

        if (initialAttachment is { } a)
        {
            doc.Attach(a.EntityType, a.EntityId, a.Role, uploadedBy);
        }

        return doc;
    }

    /// <summary>Attaches this document to an entity. Idempotent on (EntityType, EntityId, Role).</summary>
    public void Attach(string entityType, Guid entityId, string? role, UserId attachedBy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityType);
        ArgumentNullException.ThrowIfNull(attachedBy);

        if (IsDeleted)
            throw new BusinessRuleViolationException("Cannot attach a deleted document.");

        var existing = _attachments.FirstOrDefault(x =>
            x.EntityType == entityType && x.EntityId == entityId && x.Role == role);
        if (existing is not null)
            return;

        ApplyChange(new DocumentAttached(
            Id.Value, entityType, entityId, role, attachedBy.Value, DateTimeOffset.UtcNow));
    }

    /// <summary>Detaches this document from an entity. Idempotent if not attached.</summary>
    public void Detach(string entityType, Guid entityId, string? role, UserId detachedBy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityType);
        ArgumentNullException.ThrowIfNull(detachedBy);

        var existing = _attachments.FirstOrDefault(x =>
            x.EntityType == entityType && x.EntityId == entityId && x.Role == role);
        if (existing is null)
            return;

        ApplyChange(new DocumentDetached(
            Id.Value, entityType, entityId, role, detachedBy.Value, DateTimeOffset.UtcNow));
    }

    /// <summary>Soft-deletes this document. Idempotent if already deleted.</summary>
    public void SoftDelete(string reason, UserId deletedBy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        ArgumentNullException.ThrowIfNull(deletedBy);

        if (IsDeleted)
            return;

        ApplyChange(new DocumentSoftDeleted(Id.Value, reason, deletedBy.Value, DateTimeOffset.UtcNow));
    }

    /// <inheritdoc/>
    protected override void ApplyEvent(IDomainEvent domainEvent)
    {
        switch (domainEvent)
        {
            case DocumentUploaded e: Apply(e); break;
            case DocumentVersionAdded e: Apply(e); break;
            case DocumentAttached e: Apply(e); break;
            case DocumentDetached e: Apply(e); break;
            case DocumentSoftDeleted e: Apply(e); break;
            default: throw new InvalidOperationException($"Unknown event: {domainEvent?.GetType().Name}");
        }
    }

    private void Apply(DocumentUploaded e)
    {
        Id = DocumentId.Create(e.AggregateId);
        Title = e.Title;
        Sensitivity = SensitivityLevel.FromDisplayName(e.Sensitivity);
        Retention = RetentionClass.FromDisplayName(e.Retention);
        UploadedBy = UserId.Create(e.UploadedBy);
        UploadedAt = e.OccurredAt;
        IsDeleted = false;

        var version = new DocumentVersion(
            DocumentVersionId.Create(e.VersionId),
            VersionNumber: 1,
            e.BlobKey, e.FileName, e.ContentType, e.SizeBytes, e.Sha256, Note: null,
            UserId.Create(e.UploadedBy), e.OccurredAt);

        _versions.Add(version);
        CurrentVersionId = version.VersionId;
    }

    private void Apply(DocumentVersionAdded e)
    {
        var version = new DocumentVersion(
            DocumentVersionId.Create(e.VersionId),
            e.VersionNumber,
            e.BlobKey, e.FileName, e.ContentType, e.SizeBytes, e.Sha256, e.Note,
            UserId.Create(e.UploadedBy), e.OccurredAt);

        _versions.Add(version);
        CurrentVersionId = version.VersionId;
    }

    private void Apply(DocumentAttached e)
    {
        _attachments.Add(new DocumentAttachment(
            e.EntityType, e.EntityId, e.Role,
            UserId.Create(e.AttachedBy), e.OccurredAt));
    }

    private void Apply(DocumentDetached e)
    {
        _attachments.RemoveAll(x =>
            x.EntityType == e.EntityType && x.EntityId == e.EntityId && x.Role == e.Role);
    }

    private void Apply(DocumentSoftDeleted _)
    {
        IsDeleted = true;
    }
}
