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

#pragma warning disable CS8618 // State is set by Create from the stream's first event.
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

    /// <summary>Gets the optional expiry date (a calendar date, no time zone).</summary>
    public DateOnly? ExpiresOn { get; private set; }

    /// <summary>Gets the lowercase, deduplicated set of tags applied to this document.</summary>
    public IReadOnlyCollection<string> Tags => _tags;

    /// <summary>Gets a value indicating whether this document is on legal hold.</summary>
    public bool IsOnLegalHold { get; private set; }

    /// <summary>Decides to upload a brand-new document with its first version, and optionally attach it to an entity.</summary>
    /// <returns>The events that start the document's stream.</returns>
    public static IReadOnlyList<IDomainEvent> Upload(
        DocumentId documentId,
        DocumentVersionId versionId,
        string title,
        SensitivityLevel sensitivity,
        RetentionClass retention,
        string blobKey,
        string fileName,
        string contentType,
        long sizeBytes,
        string sha256,
        UserId uploadedBy,
        (string EntityType, string EntityId, string? Role)? initialAttachment = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentNullException.ThrowIfNull(sensitivity);
        ArgumentNullException.ThrowIfNull(retention);
        ArgumentException.ThrowIfNullOrWhiteSpace(blobKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);
        ArgumentException.ThrowIfNullOrWhiteSpace(sha256);

        if (sizeBytes <= 0)
            throw BusinessRuleViolation.Document.Upload.SizeNonPositive(sizeBytes);

        var uploaded = new DocumentUploaded(
            documentId.Value,
            versionId.Value,
            title,
            sensitivity,
            retention,
            blobKey,
            fileName,
            contentType,
            sizeBytes,
            sha256,
            uploadedBy.Value);

        if (initialAttachment is not { } a)
        {
            return [uploaded];
        }

        var attached = Create(uploaded).Attach(a.EntityType, a.EntityId, a.Role, uploadedBy);
        return attached is null ? [uploaded] : [uploaded, attached];
    }

    /// <summary>Decides to attach this document to an entity.</summary>
    /// <returns>The attachment, or <see langword="null"/> when already attached with that role.</returns>
    public DocumentAttached? Attach(string entityType, string entityId, string? role, UserId attachedBy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityType);

        if (IsDeleted)
            throw BusinessRuleViolation.Document.Attach.DocumentDeleted();

        var existing = _attachments.FirstOrDefault(x =>
            x.EntityType == entityType && x.EntityId == entityId && x.Role == role);
        if (existing is not null)
            return null;

        var current = _versions[^1];
        return new DocumentAttached(
            Id.Value, entityType, entityId, role,
            Title, current.FileName, current.ContentType, current.SizeBytes,
            attachedBy.Value);
    }

    /// <summary>Decides to detach this document from an entity.</summary>
    /// <returns>The detachment, or <see langword="null"/> when not attached.</returns>
    public DocumentDetached? Detach(string entityType, string entityId, string? role, UserId detachedBy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityType);

        var existing = _attachments.FirstOrDefault(x =>
            x.EntityType == entityType && x.EntityId == entityId && x.Role == role);
        if (existing is null)
            return null;

        return new DocumentDetached(Id.Value, entityType, entityId, role, detachedBy.Value);
    }

    /// <summary>Decides to soft-delete this document. Nothing happens if it is already deleted.</summary>
    /// <remarks>
    /// Decides one <see cref="DocumentSoftDeleted"/> aggregate-stream event followed by one
    /// <see cref="DocumentSoftDeletedForAttachment"/> per active attachment so the per-entity
    /// projection can flag the row as deleted without a cross-stream lookup.
    /// </remarks>
    /// <returns>The events, or none when the document is already deleted.</returns>
    public IReadOnlyList<IDomainEvent> SoftDelete(string reason, UserId deletedBy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        if (IsDeleted)
            return [];

        if (IsOnLegalHold)
            throw BusinessRuleViolation.Document.SoftDelete.LegalHoldActive();

        return
        [
            new DocumentSoftDeleted(Id.Value, reason, deletedBy.Value),
            .. _attachments.Select(att => new DocumentSoftDeletedForAttachment(
                Id.Value, att.EntityType, att.EntityId, att.Role)),
        ];
    }

    /// <summary>Decides to add a new version to this document.</summary>
    public DocumentVersionAdded AddVersion(
        DocumentVersionId versionId,
        string blobKey,
        string fileName,
        string contentType,
        long sizeBytes,
        string sha256,
        string? note,
        UserId uploadedBy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(blobKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);
        ArgumentException.ThrowIfNullOrWhiteSpace(sha256);

        if (sizeBytes <= 0)
            throw BusinessRuleViolation.Document.AddVersion.SizeNonPositive(sizeBytes);

        if (IsDeleted)
            throw BusinessRuleViolation.Document.AddVersion.DocumentDeleted();

        var nextNumber = _versions[^1].VersionNumber + 1;

        return new DocumentVersionAdded(
            Id.Value,
            versionId.Value,
            nextNumber,
            blobKey,
            fileName,
            contentType,
            sizeBytes,
            sha256,
            note,
            uploadedBy.Value);
    }

    /// <summary>Decides to update editable metadata fields. Nulls leave the corresponding field untouched.</summary>
    /// <returns>The update, or <see langword="null"/> when nothing changes.</returns>
    public DocumentMetadataUpdated? UpdateMetadata(
        string? title,
        SensitivityLevel? sensitivity,
        DateOnly? expiresOn,
        bool clearExpiresOn,
        UserId updatedBy)
    {

        if (IsDeleted)
            throw BusinessRuleViolation.Document.UpdateMetadata.DocumentDeleted();

        if (title is not null && string.IsNullOrWhiteSpace(title))
            throw BusinessRuleViolation.Document.UpdateMetadata.TitleBlank();

        var newTitle = title ?? Title;
        var newSensitivity = sensitivity ?? Sensitivity;
        var newExpiresOn = clearExpiresOn ? (DateOnly?)null : (expiresOn ?? ExpiresOn);

        if (newTitle == Title
            && newSensitivity == Sensitivity
            && newExpiresOn == ExpiresOn)
        {
            return null;
        }

        return new DocumentMetadataUpdated(
            Id.Value, newTitle, newSensitivity, newExpiresOn, clearExpiresOn,
            updatedBy.Value);
    }

    /// <summary>Decides to add a tag (lowercased, trimmed).</summary>
    /// <returns>The tagging, or <see langword="null"/> when the document already has the tag.</returns>
    public DocumentTagged? Tag(string tag, UserId taggedBy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tag);

        if (IsDeleted)
            throw BusinessRuleViolation.Document.Tag.DocumentDeleted();

        var normalised = NormaliseTag(tag);
        if (_tags.Contains(normalised))
            return null;

        return new DocumentTagged(Id.Value, normalised, taggedBy.Value);
    }

    /// <summary>Decides to remove a tag.</summary>
    /// <returns>The untagging, or <see langword="null"/> when the document doesn't have the tag.</returns>
    public DocumentUntagged? Untag(string tag, UserId untaggedBy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tag);

        var normalised = NormaliseTag(tag);
        if (!_tags.Contains(normalised))
            return null;

        return new DocumentUntagged(Id.Value, normalised, untaggedBy.Value);
    }

    /// <summary>Normalises a tag string to lowercase, trimmed.</summary>
    public static string NormaliseTag(string tag)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tag);
        return tag.Trim().ToLowerInvariant();
    }

    /// <summary>Starts a document from its first event.</summary>
    public static Document Create(DocumentUploaded e)
    {
        ArgumentNullException.ThrowIfNull(e);

        var doc = new Document();
        doc.Apply(e);
        return doc;
    }

    private void Apply(DocumentUploaded e)
    {
        Id = DocumentId.Create(e.AggregateId);
        Title = e.Title;
        Sensitivity = e.Sensitivity;
        Retention = e.Retention;
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

    /// <summary>Applies <see cref="DocumentVersionAdded"/>.</summary>
    public void Apply(DocumentVersionAdded e)
    {
        var version = new DocumentVersion(
            DocumentVersionId.Create(e.VersionId),
            e.VersionNumber,
            e.BlobKey, e.FileName, e.ContentType, e.SizeBytes, e.Sha256, e.Note,
            UserId.Create(e.UploadedBy), e.OccurredAt);

        _versions.Add(version);
        CurrentVersionId = version.VersionId;
    }

    /// <summary>Applies <see cref="DocumentAttached"/>.</summary>
    public void Apply(DocumentAttached e)
    {
        _attachments.Add(new DocumentAttachment(
            e.EntityType, e.EntityId, e.Role,
            UserId.Create(e.AttachedBy), e.OccurredAt));
    }

    /// <summary>Applies <see cref="DocumentDetached"/>.</summary>
    public void Apply(DocumentDetached e)
    {
        _attachments.RemoveAll(x =>
            x.EntityType == e.EntityType && x.EntityId == e.EntityId && x.Role == e.Role);
    }

    /// <summary>Applies <see cref="DocumentSoftDeleted"/>.</summary>
    public void Apply(DocumentSoftDeleted _)
    {
        IsDeleted = true;
    }

    /// <summary>Applies <see cref="DocumentMetadataUpdated"/>.</summary>
    public void Apply(DocumentMetadataUpdated e)
    {
        Title = e.Title;
        Sensitivity = e.Sensitivity;
        ExpiresOn = e.ClearExpiresOn ? null : e.ExpiresOn;
    }

    /// <summary>Applies <see cref="DocumentTagged"/>.</summary>
    public void Apply(DocumentTagged e)
    {
        _tags.Add(e.Tag);
    }

    /// <summary>Applies <see cref="DocumentUntagged"/>.</summary>
    public void Apply(DocumentUntagged e)
    {
        _tags.Remove(e.Tag);
    }
}
