using System.Text.Json;
using CommonGround.SharedKernel.Domain;
using FluentAssertions;

namespace CommonGround.SharedKernel.Tests.Domain;

/// <summary>Covers the identifier plumbing every <see cref="IEntityId{TSelf}"/> shares, through <see cref="UserId"/>.</summary>
public class EntityIdTests
{
    public class TheCreateMethod
    {
        [Theory]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData("trust-01jq")]
        [InlineData("user01jq")]
        public void GivenValueWithoutThePrefix_WhenCreated_ThenThrows(string value)
        {
            var act = () => UserId.Create(value);

            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void GivenSameValue_WhenCreatedTwice_ThenIdsAreEqual()
        {
            var sut = UserId.Create("user-01jq");

            sut.Should().Be(UserId.Create("user-01jq"));
        }
    }

    public class TheGenerateMethod
    {
        [Fact]
        public void GivenNothing_WhenGenerated_ThenValueIsPrefixedAndUnique()
        {
            var sut = UserId.Generate();

            sut.Value.Should().StartWith("user-").And.NotBe(UserId.Generate().Value);
        }
    }

    public class TheJsonConverter
    {
        [Fact]
        public void GivenId_WhenRoundTripped_ThenSerializesAsPlainStringAndReadsBack()
        {
            var sut = UserId.Create("user-01jq");

            var json = JsonSerializer.Serialize(new { Id = sut });

            json.Should().Be("""{"Id":"user-01jq"}""");
            JsonSerializer.Deserialize<UserId>("\"user-01jq\"").Should().Be(sut);
        }
    }
}
