using System.Text.Json;
using CommonGround.SharedKernel.Domain;
using FluentAssertions;

namespace CommonGround.SharedKernel.Tests.Domain;

public class EnumerationJsonConverterTests
{
    private static readonly JsonSerializerOptions Options = new() { Converters = { new EnumerationJsonConverter() } };

    public class TheWriteMethod
    {
        [Fact]
        public void GivenEnumeration_WhenSerialized_ThenWritesItsDisplayName()
        {
            var json = JsonSerializer.Serialize(new Holder(Shade.DeepBlue, [Shade.Red]), Options);

            json.Should().Be("""{"Shade":"DeepBlue","Shades":["Red"]}""");
        }
    }

    public class TheReadMethod
    {
        [Fact]
        public void GivenDisplayName_WhenDeserialized_ThenReadsTheSameInstance()
        {
            var sut = JsonSerializer.Deserialize<Holder>("""{"Shade":"DeepBlue","Shades":["Red"]}""", Options);

            sut!.Shade.Should().BeSameAs(Shade.DeepBlue);
            sut.Shades.Should().ContainSingle().Which.Should().BeSameAs(Shade.Red);
        }

        [Theory]
        [InlineData("\"Green\"")]
        [InlineData("2")]
        public void GivenUnknownDisplayNameOrValue_WhenDeserialized_ThenThrows(string json)
        {
            var act = () => JsonSerializer.Deserialize<Shade>(json, Options);

            act.Should().Throw<JsonException>();
        }
    }

    private sealed record Holder(Shade Shade, IReadOnlyList<Shade> Shades);

    private sealed class Shade : Enumeration<Shade>
    {
        public static readonly Shade Red = new(1, "Red");
        public static readonly Shade DeepBlue = new(2, "DeepBlue");

        private Shade(int value, string displayName) : base(value, displayName) { }
    }
}
