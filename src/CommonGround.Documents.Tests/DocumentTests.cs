using CommonGround.Documents.Domain.Documents;
using CommonGround.Documents.Domain.Documents.Events;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.Testing;
using FluentAssertions;
using Xunit;

namespace CommonGround.Documents.Tests;

public class DocumentTests
{
    public class TestBed
    {
        public UserId User { get; } = UserId.Generate();

        public DocumentId DocumentId { get; } = DocumentId.Generate();

        public IReadOnlyList<object> Upload((string EntityType, string EntityId, string? Role)? attach = null) =>
            Document.Upload(
                DocumentId,
                DocumentVersionId.Generate(),
                "Title",
                SensitivityLevel.Internal,
                RetentionClass.Permanent,
                blobKey: "documents/abc/v1.pdf",
                fileName: "v1.pdf",
                contentType: "application/pdf",
                sizeBytes: 1234,
                sha256: "deadbeef",
                uploadedBy: User,
                initialAttachment: attach);

        public Document UploadedDocument() => EventReplay.Replay<Document>(Upload());

        public Document AttachedDocument(params string[] entityIds) =>
            UploadedDocument().Evolve(entityIds.Select(id => (object)Attached(id)))!;

        public Document DeletedDocument() => UploadedDocument().Evolve(new DocumentSoftDeleted(DocumentId.Value, "gone", User.Value))!;

        public Document TaggedDocument(string tag) => UploadedDocument().Evolve(new DocumentTagged(DocumentId.Value, tag, User.Value))!;

        public DocumentAttached Attached(string entityId, string? role = null) =>
            new(DocumentId.Value, "Building", entityId, role, "Title", "v1.pdf", "application/pdf", 1234, User.Value);
    }

    public class TheUploadMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenZeroSize_WhenUploaded_ThenThrowsBusinessRuleViolation()
        {
            var act = () => Document.Upload(
                _testBed.DocumentId, DocumentVersionId.Generate(), "t", SensitivityLevel.Internal, RetentionClass.Permanent,
                "key", "f", "application/pdf", sizeBytes: 0, sha256: "x", uploadedBy: _testBed.User);

            act.Should().Throw<BusinessRuleViolationException>();
        }

        [Fact]
        public void GivenValidInputs_WhenUploaded_ThenStartsWithTheFirstVersion()
        {
            var events = _testBed.Upload();

            var sut = EventReplay.Replay<Document>(events);
            sut.Versions.Should().ContainSingle().Which.VersionNumber.Should().Be(1);
            sut.CurrentVersionId.Should().Be(sut.Versions[0].VersionId);
        }

        [Fact]
        public void GivenInitialAttachment_WhenUploaded_ThenAlsoAttaches()
        {
            var events = _testBed.Upload(("Building", "building-1", "primary"));

            events.Should().SatisfyRespectively(
                e => e.Should().BeOfType<DocumentUploaded>(),
                e => e.Should().BeEquivalentTo(new { EntityType = "Building", EntityId = "building-1", Role = "primary" }));
        }
    }

    public class TheAttachMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenAlreadyAttachedWithTheSameRole_WhenAttached_ThenDecidesNothing()
        {
            var sut = _testBed.AttachedDocument("building-1");

            var attached = sut.Attach("Building", "building-1", null, _testBed.User);

            attached.Should().BeNull();
        }

        [Fact]
        public void GivenDeletedDocument_WhenAttached_ThenThrowsBusinessRuleViolation()
        {
            var sut = _testBed.DeletedDocument();

            var act = () => sut.Attach("Building", "building-1", null, _testBed.User);

            act.Should().Throw<BusinessRuleViolationException>();
        }
    }

    public class TheDetachMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenTwoAttachments_WhenOneIsDetached_ThenOnlyThatOneIsRemoved()
        {
            var sut = _testBed.AttachedDocument("building-a", "building-b");

            var detached = sut.Detach("Building", "building-a", null, _testBed.User);

            sut.Evolve(detached!)!.Attachments.Should().ContainSingle(a => a.EntityId == "building-b");
        }

        [Fact]
        public void GivenNotAttached_WhenDetached_ThenDecidesNothing()
        {
            var sut = _testBed.UploadedDocument();

            var detached = sut.Detach("Building", "building-1", null, _testBed.User);

            detached.Should().BeNull();
        }
    }

    public class TheAddVersionMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenUploadedDocument_WhenVersionAdded_ThenNumbersItNextAndMakesItCurrent()
        {
            var sut = _testBed.UploadedDocument();
            var versionId = DocumentVersionId.Generate();

            var added = sut.AddVersion(versionId, "documents/abc/v2.pdf", "v2.pdf", "application/pdf", 2345, "feedface", "second draft", _testBed.User);

            added.VersionNumber.Should().Be(2);
            sut.Evolve(added)!.CurrentVersionId.Should().Be(versionId);
        }

        [Fact]
        public void GivenDeletedDocument_WhenVersionAdded_ThenThrowsBusinessRuleViolation()
        {
            var sut = _testBed.DeletedDocument();

            var act = () => sut.AddVersion(DocumentVersionId.Generate(), "k", "f", "application/pdf", 1, "x", null, _testBed.User);

            act.Should().Throw<BusinessRuleViolationException>();
        }
    }

    public class TheSoftDeleteMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenDeletedDocument_WhenSoftDeleted_ThenDecidesNothing()
        {
            var sut = _testBed.DeletedDocument();

            var events = sut.SoftDelete("again", _testBed.User);

            events.Should().BeEmpty();
        }

        [Fact]
        public void GivenTwoAttachments_WhenSoftDeleted_ThenFansOutOneEventPerAttachment()
        {
            var sut = _testBed.AttachedDocument("building-a", "building-b");

            var events = sut.SoftDelete("reason", _testBed.User);

            events.OfType<DocumentSoftDeleted>().Should().ContainSingle();
            events.OfType<DocumentSoftDeletedForAttachment>().Should().HaveCount(2);
        }
    }

    public class TheTagMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenUntaggedDocument_WhenTagged_ThenNormalisesTheTag()
        {
            var sut = _testBed.UploadedDocument();

            var tagged = sut.Tag("  Hello  ", _testBed.User);

            tagged!.Tag.Should().Be("hello");
        }

        [Fact]
        public void GivenTaggedDocument_WhenTaggedInAnotherCase_ThenDecidesNothing()
        {
            var sut = _testBed.TaggedDocument("hello");

            var tagged = sut.Tag("HELLO", _testBed.User);

            tagged.Should().BeNull();
        }
    }

    public class TheUntagMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenTaggedDocument_WhenUntaggedInAnotherCase_ThenRemovesTheTag()
        {
            var sut = _testBed.TaggedDocument("important");

            var untagged = sut.Untag("IMPORTANT", _testBed.User);

            sut.Evolve(untagged!)!.Tags.Should().BeEmpty();
        }

        [Fact]
        public void GivenTagAbsent_WhenUntagged_ThenDecidesNothing()
        {
            var sut = _testBed.UploadedDocument();

            var untagged = sut.Untag("nope", _testBed.User);

            untagged.Should().BeNull();
        }
    }

    public class TheUpdateMetadataMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenOnlySomeFields_WhenUpdated_ThenKeepsTheOthers()
        {
            var sut = _testBed.UploadedDocument();
            var newExpiry = new DateOnly(2026, 11, 27);

            var updated = sut.UpdateMetadata("New Title", null, newExpiry, clearExpiresOn: false, _testBed.User);

            sut.Evolve(updated!).Should().BeEquivalentTo(new
            {
                Title = "New Title",
                Sensitivity = SensitivityLevel.Internal,
                ExpiresOn = newExpiry,
            });
        }

        [Fact]
        public void GivenExpiry_WhenClearedExplicitly_ThenHasNoExpiry()
        {
            var sut = _testBed.UploadedDocument();
            sut = sut.Evolve(sut.UpdateMetadata(null, null, new DateOnly(2026, 10, 28), false, _testBed.User)!)!;

            var updated = sut.UpdateMetadata(null, null, null, clearExpiresOn: true, _testBed.User);

            sut.Evolve(updated!)!.ExpiresOn.Should().BeNull();
        }

        [Fact]
        public void GivenNoChanges_WhenUpdated_ThenDecidesNothing()
        {
            var sut = _testBed.UploadedDocument();

            var updated = sut.UpdateMetadata(sut.Title, sut.Sensitivity, null, false, _testBed.User);

            updated.Should().BeNull();
        }

        [Fact]
        public void GivenBlankTitle_WhenUpdated_ThenThrowsBusinessRuleViolation()
        {
            var sut = _testBed.UploadedDocument();

            var act = () => sut.UpdateMetadata("   ", null, null, false, _testBed.User);

            act.Should().Throw<BusinessRuleViolationException>();
        }
    }
}
