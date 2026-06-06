using CommonGround.Documents.Application.Errors;
using CommonGround.Documents.Domain.Documents.Events;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Documents.Domain.Documents;

/// <summary>
/// A document tracked by the CLT — owns its versions, attachments, sensitivity, and retention metadata.
/// </summary>
public sealed class Document : AggregateRoot<DocumentId>
{
    private readonly List<DocumentVersion> _versions = [];
    private readonly List<DocumentAttachment> _attachments = [];
    private readonly SortedSet<string> _tags = new(StringComparer.Ordinal);

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

    /// <summary>Gets the optional expiry timestamp.</summary>
    public DateTimeOffset? ExpiresAt { get; private set; }

    /// <summary>Gets the lowercase, deduplicated set of tags applied to this document.</summary>
    public IReadOnlyCollection<string> Tags => _tags;

    /// <summary>Gets a value indicating whether this document is on legal hold.</summary>
    public bool IsOnLegalHold { get; private set; }

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
        (string EntityType, string EntityId, string? Role)? initialAttachment = null,
        DocumentId? documentId = null,
        DocumentVersionId? versionId = null)
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
            throw BusinessRuleViolation.Document.Upload.SizeNonPositive(sizeBytes);

        var doc = new Document();
        var actualVersionId = versionId ?? DocumentVersionId.Generate();
        var actualDocumentId = documentId ?? DocumentId.Generate();

        doc.ApplyChange(new DocumentUploaded(
            actualDocumentId.Value,
            actualVersionId.Value,
            title,
            sensitivity.DisplayName,
            retention.DisplayName,
            blobKey,
            fileName,
            contentType,
            sizeBytes,
            sha256,
            uploadedBy.Value));

        if (initialAttachment is { } a)
        {
            doc.Attach(a.EntityType, a.EntityId, a.Role, uploadedBy);
        }

        return doc;
    }

    /// <summary>Attaches this document to an entity. Idempotent on (EntityType, EntityId, Role).</summary>
    public void Attach(string entityType, string entityId, string? role, UserId attachedBy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityType);
        ArgumentNullException.ThrowIfNull(attachedBy);

        if (IsDeleted)
            throw BusinessRuleViolation.Document.Attach.DocumentDeleted();

        var existing = _attachments.FirstOrDefault(x =>
            x.EntityType == entityType && x.EntityId == entityId && x.Role == role);
        if (existing is not null)
            return;

        var current = _versions[^1];
        ApplyChange(new DocumentAttached(
            Id.Value, entityType, entityId, role,
            Title, current.FileName, current.ContentType, current.SizeBytes,
            attachedBy.Value));
    }

    /// <summary>Detaches this document from an entity. Idempotent if not attached.</summary>
    public void Detach(string entityType, string entityId, string? role, UserId detachedBy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityType);
        ArgumentNullException.ThrowIfNull(detachedBy);

        var existing = _attachments.FirstOrDefault(x =>
            x.EntityType == entityType && x.EntityId == entityId && x.Role == role);
        if (existing is null)
            return;

        ApplyChange(new DocumentDetached(
            Id.Value, entityType, entityId, role, detachedBy.Value));
    }

    /// <summary>Soft-deletes this document. Idempotent if already deleted.</summary>
    /// <remarks>
    /// Raises one <see cref="DocumentSoftDeleted"/> aggregate-stream event followed by one
    /// <see cref="DocumentSoftDeletedForAttachment"/> per active attachment so the per-entity
    /// projection can flag the row as deleted without a cross-stream lookup.
    /// </remarks>
    public void SoftDelete(string reason, UserId deletedBy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        ArgumentNullException.ThrowIfNull(deletedBy);

        if (IsDeleted)
            return;

        if (IsOnLegalHold)
            throw BusinessRuleViolation.Document.SoftDelete.LegalHoldActive();

        ApplyChange(new DocumentSoftDeleted(Id.Value, reason, deletedBy.Value));

        // Snapshot before raising fanout events — the apply paths don't mutate _attachments here.
        foreach (var att in _attachments.ToArray())
        {
            ApplyChange(new DocumentSoftDeletedForAttachment(
                Id.Value, att.EntityType, att.EntityId, att.Role));
        }
    }

    /// <summary>Adds a new version to this document.</summary>
    public void AddVersion(
        string blobKey,
        string fileName,
        string contentType,
        long sizeBytes,
        string sha256,
        string? note,
        UserId uploadedBy,
        DocumentVersionId? versionId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(blobKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);
        ArgumentException.ThrowIfNullOrWhiteSpace(sha256);
        ArgumentNullException.ThrowIfNull(uploadedBy);

        if (sizeBytes <= 0)
            throw BusinessRuleViolation.Document.AddVersion.SizeNonPositive(sizeBytes);

        if (IsDeleted)
            throw BusinessRuleViolation.Document.AddVersion.DocumentDeleted();

        var actualVersionId = versionId ?? DocumentVersionId.Generate();
        var nextNumber = _versions[^1].VersionNumber + 1;

        ApplyChange(new DocumentVersionAdded(
            Id.Value,
            actualVersionId.Value,
            nextNumber,
            blobKey,
            fileName,
            contentType,
            sizeBytes,
            sha256,
            note,
            uploadedBy.Value));
    }

    /// <summary>Updates editable metadata fields. Nulls leave the corresponding field untouched.</summary>
    public void UpdateMetadata(
        string? title,
        SensitivityLevel? sensitivity,
        DateTimeOffset? expiresAt,
        bool clearExpiresAt,
        UserId updatedBy)
    {
        ArgumentNullException.ThrowIfNull(updatedBy);

        if (IsDeleted)
            throw BusinessRuleViolation.Document.UpdateMetadata.DocumentDeleted();

        if (title is not null && string.IsNullOrWhiteSpace(title))
            throw BusinessRuleViolation.Document.UpdateMetadata.TitleBlank();

        var newTitle = title ?? Title;
        var newSensitivity = sensitivity?.DisplayName ?? Sensitivity.DisplayName;
        var newExpiresAt = clearExpiresAt ? (DateTimeOffset?)null : (expiresAt ?? ExpiresAt);

        if (newTitle == Title
            && newSensitivity == Sensitivity.DisplayName
            && newExpiresAt == ExpiresAt)
        {
            return;
        }

        ApplyChange(new DocumentMetadataUpdated(
            Id.Value, newTitle, newSensitivity, newExpiresAt, clearExpiresAt,
            updatedBy.Value));
    }

    /// <summary>Adds a tag (lowercased, trimmed). Idempotent.</summary>
    public void Tag(string tag, UserId taggedBy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tag);
        ArgumentNullException.ThrowIfNull(taggedBy);

        if (IsDeleted)
            throw BusinessRuleViolation.Document.Tag.DocumentDeleted();

        var normalised = NormaliseTag(tag);
        if (_tags.Contains(normalised))
            return;

        ApplyChange(new DocumentTagged(
            Id.Value, normalised, taggedBy.Value));
    }

    /// <summary>Removes a tag. Idempotent.</summary>
    public void Untag(string tag, UserId untaggedBy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tag);
        ArgumentNullException.ThrowIfNull(untaggedBy);

        var normalised = NormaliseTag(tag);
        if (!_tags.Contains(normalised))
            return;

        ApplyChange(new DocumentUntagged(
            Id.Value, normalised, untaggedBy.Value));
    }

    /// <summary>Normalises a tag string to lowercase, trimmed.</summary>
    public static string NormaliseTag(string tag)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tag);
        return tag.Trim().ToLowerInvariant();
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
            case DocumentSoftDeletedForAttachment _: break; // fanout-only, no aggregate state change
            case DocumentMetadataUpdated e: Apply(e); break;
            case DocumentTagged e: Apply(e); break;
            case DocumentUntagged e: Apply(e); break;
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

    private void Apply(DocumentMetadataUpdated e)
    {
        Title = e.Title;
        Sensitivity = SensitivityLevel.FromDisplayName(e.Sensitivity);
        ExpiresAt = e.ClearExpiresAt ? null : e.ExpiresAt;
    }

    private void Apply(DocumentTagged e)
    {
        _tags.Add(e.Tag);
    }

    private void Apply(DocumentUntagged e)
    {
        _tags.Remove(e.Tag);
    }
}
