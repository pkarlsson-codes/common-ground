using CommonGround.Documents.Domain.Documents;
using CommonGround.Documents.Domain.Documents.Events;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace CommonGround.Documents.Tests;

/// <summary>Unit tests for the <see cref="Document"/> aggregate.</summary>
public sealed class DocumentTests
{
    private static readonly UserId User = UserId.Generate();

    private static Document NewDoc(
        string title = "Title",
        (string EntityType, string EntityId, string? Role)? attach = null) =>
        Document.Upload(
            title,
            SensitivityLevel.Internal,
            RetentionClass.Permanent,
            blobKey: "documents/abc/v1.pdf",
            fileName: "v1.pdf",
            contentType: "application/pdf",
            sizeBytes: 1234,
            sha256: "deadbeef",
            uploadedBy: User,
            initialAttachment: attach);

    [Fact]
    public void Upload_creates_document_with_first_version()
    {
        var doc = NewDoc();

        doc.Title.Should().Be("Title");
        doc.Versions.Should().HaveCount(1);
        doc.Versions[0].VersionNumber.Should().Be(1);
        doc.CurrentVersionId.Should().Be(doc.Versions[0].VersionId);
        doc.IsDeleted.Should().BeFalse();
        doc.UncommittedEvents.Should().ContainItemsAssignableTo<DocumentUploaded>();
    }

    [Fact]
    public void Upload_with_zero_size_throws()
    {
        var act = () => Document.Upload(
            "t", SensitivityLevel.Internal, RetentionClass.Permanent,
            "key", "f", "application/pdf", sizeBytes: 0, sha256: "x", uploadedBy: User);

        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void Upload_with_initial_attachment_attaches_it()
    {
        var entityId = Guid.NewGuid().ToString();
        var doc = NewDoc(attach: ("Building", entityId, "primary"));

        doc.Attachments.Should().ContainSingle(a =>
            a.EntityType == "Building" && a.EntityId == entityId && a.Role == "primary");
    }

    [Fact]
    public void Attach_is_idempotent()
    {
        var doc = NewDoc();
        var entityId = Guid.NewGuid().ToString();

        doc.Attach("Building", entityId, "primary", User);
        doc.Attach("Building", entityId, "primary", User);

        doc.Attachments.Should().HaveCount(1);
    }

    [Fact]
    public void Attach_to_deleted_document_throws()
    {
        var doc = NewDoc();
        doc.SoftDelete("test", User);

        var act = () => doc.Attach("Building", Guid.NewGuid().ToString(), null, User);

        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void Detach_removes_matching_attachment_only()
    {
        var doc = NewDoc();
        var idA = Guid.NewGuid().ToString();
        var idB = Guid.NewGuid().ToString();
        doc.Attach("Building", idA, null, User);
        doc.Attach("Building", idB, null, User);

        doc.Detach("Building", idA, null, User);

        doc.Attachments.Should().ContainSingle(a => a.EntityId == idB);
    }

    [Fact]
    public void Detach_when_not_attached_is_noop()
    {
        var doc = NewDoc();
        doc.Detach("Building", Guid.NewGuid().ToString(), null, User);

        doc.Attachments.Should().BeEmpty();
    }

    [Fact]
    public void AddVersion_increments_version_number_and_advances_pointer()
    {
        var doc = NewDoc();
        var v1Id = doc.CurrentVersionId;

        doc.AddVersion(
            "documents/abc/v2.pdf", "v2.pdf", "application/pdf",
            sizeBytes: 2345, sha256: "feedface", note: "second draft", uploadedBy: User);

        doc.Versions.Should().HaveCount(2);
        doc.Versions[1].VersionNumber.Should().Be(2);
        doc.CurrentVersionId.Should().NotBe(v1Id).And.Be(doc.Versions[1].VersionId);
    }

    [Fact]
    public void AddVersion_to_deleted_document_throws()
    {
        var doc = NewDoc();
        doc.SoftDelete("reason", User);

        var act = () => doc.AddVersion("k", "f", "application/pdf", 1, "x", null, User);

        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void SoftDelete_is_idempotent()
    {
        var doc = NewDoc();
        doc.SoftDelete("first", User);
        var countAfterFirst = doc.UncommittedEvents.Count();

        doc.SoftDelete("second", User);

        doc.UncommittedEvents.Count().Should().Be(countAfterFirst);
        doc.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void SoftDelete_fans_out_one_event_per_active_attachment()
    {
        var doc = NewDoc();
        doc.Attach("Building", Guid.NewGuid().ToString(), "primary", User);
        doc.Attach("Lease", Guid.NewGuid().ToString(), null, User);
        doc.UncommittedEvents.MarkAsCommitted();

        doc.SoftDelete("reason", User);

        var events = doc.UncommittedEvents.ToList();
        events.OfType<DocumentSoftDeleted>().Should().HaveCount(1);
        events.OfType<DocumentSoftDeletedForAttachment>().Should().HaveCount(2);
    }

    [Fact]
    public void Tag_normalises_to_lowercase_and_dedupes()
    {
        var doc = NewDoc();

        doc.Tag("  Hello  ", User);
        doc.Tag("hello", User);
        doc.Tag("HELLO", User);

        doc.Tags.Should().BeEquivalentTo(["hello"]);
    }

    [Fact]
    public void Untag_removes_normalised_tag()
    {
        var doc = NewDoc();
        doc.Tag("Important", User);

        doc.Untag("IMPORTANT", User);

        doc.Tags.Should().BeEmpty();
    }

    [Fact]
    public void Untag_when_absent_is_noop()
    {
        var doc = NewDoc();
        doc.UncommittedEvents.MarkAsCommitted();

        doc.Untag("nope", User);

        doc.UncommittedEvents.Should().BeEmpty();
    }

    [Fact]
    public void UpdateMetadata_changes_only_supplied_fields()
    {
        var doc = NewDoc();
        var newExpiry = DateTimeOffset.UtcNow.AddDays(30);

        doc.UpdateMetadata("New Title", null, newExpiry, clearExpiresAt: false, User);

        doc.Title.Should().Be("New Title");
        doc.Sensitivity.Should().Be(SensitivityLevel.Internal);
        doc.ExpiresAt.Should().Be(newExpiry);
    }

    [Fact]
    public void UpdateMetadata_can_clear_expiry_explicitly()
    {
        var doc = NewDoc();
        doc.UpdateMetadata(null, null, DateTimeOffset.UtcNow, false, User);
        doc.ExpiresAt.Should().NotBeNull();

        doc.UpdateMetadata(null, null, null, clearExpiresAt: true, User);

        doc.ExpiresAt.Should().BeNull();
    }

    [Fact]
    public void UpdateMetadata_with_no_changes_is_noop()
    {
        var doc = NewDoc();
        doc.UncommittedEvents.MarkAsCommitted();

        doc.UpdateMetadata(doc.Title, doc.Sensitivity, null, false, User);

        doc.UncommittedEvents.Should().BeEmpty();
    }

    [Fact]
    public void UpdateMetadata_blank_title_throws()
    {
        var doc = NewDoc();

        var act = () => doc.UpdateMetadata("   ", null, null, false, User);

        act.Should().Throw<BusinessRuleViolationException>();
    }
}
