using CommonGround.Documents.Application.ReadModels;
using CommonGround.Documents.Application.WorkItems;
using FluentAssertions;
using Xunit;

namespace CommonGround.Documents.Tests.Application.WorkItems;

public class DocumentsWorkItemsTests
{
    public class TestBed
    {
        public DateOnly Today { get; } = new(2026, 11, 2);

        public DocumentLibraryItemView Document(DateOnly? expiresOn) =>
            new() { Id = "document-1", Title = "Homeowner insurance", ExpiresOn = expiresOn };
    }

    public class TheForDocumentMethod
    {
        private readonly TestBed _testBed = new();

        [Theory]
        [InlineData(-1, "Expired")]
        [InlineData(0, "Expiring")]
        [InlineData(30, "Expiring")]
        public void GivenExpiryWithinTheWindowOrPast_WhenDerived_ThenIsDueOnTheExpiryDate(int days, string detail)
        {
            var expiresOn = _testBed.Today.AddDays(days);

            var item = DocumentsWorkItems.ForDocument(_testBed.Document(expiresOn), _testBed.Today);

            item.Should().BeEquivalentTo(new { Label = "Homeowner insurance", Detail = detail, DueDate = (DateOnly?)expiresOn });
        }

        [Fact]
        public void GivenExpiryBeyondTheWindow_WhenDerived_ThenNothingIsWaiting()
        {
            var item = DocumentsWorkItems.ForDocument(_testBed.Document(_testBed.Today.AddDays(31)), _testBed.Today);

            item.Should().BeNull();
        }

        [Fact]
        public void GivenDeletedDocument_WhenDerived_ThenNothingIsWaiting()
        {
            var document = _testBed.Document(_testBed.Today);
            document.IsDeleted = true;

            var item = DocumentsWorkItems.ForDocument(document, _testBed.Today);

            item.Should().BeNull();
        }
    }
}
