using System.Text.Json;
using CommonGround.SharedKernel.Infrastructure.Seeding;
using FluentAssertions;

namespace CommonGround.SharedKernel.Tests.Infrastructure.Seeding;

public class SeedDateTests
{
    public class TestBed
    {
        public DateOnly Today { get; } = new(2026, 11, 3);
    }

    public class TheResolveMethod
    {
        private readonly TestBed _testBed = new();

        [Theory]
        [InlineData("today", "2026-11-03")]
        [InlineData("today-30d", "2026-10-04")]
        [InlineData("today+14d", "2026-11-17")]
        [InlineData("today-2w", "2026-10-20")]
        [InlineData("today+1w", "2026-11-10")]
        [InlineData("today-6m", "2026-05-03")]
        [InlineData("today+3m", "2027-02-03")]
        public void GivenRelativeToken_WhenResolved_ThenOffsetsFromToday(string value, string expected)
        {
            var sut = SeedDate.Parse(value);

            sut.Resolve(_testBed.Today).Should().Be(DateOnly.Parse(expected));
        }

        [Fact]
        public void GivenIsoDate_WhenResolved_ThenIgnoresToday()
        {
            var sut = SeedDate.Parse("2021-09-15");

            sut.Resolve(_testBed.Today).Should().Be(new DateOnly(2021, 9, 15));
        }

        [Fact]
        public void GivenMonthOffsetFromMonthEnd_WhenResolved_ThenClampsToShorterMonth()
        {
            var sut = SeedDate.Parse("today-1m");

            sut.Resolve(new DateOnly(2026, 3, 31)).Should().Be(new DateOnly(2026, 2, 28));
        }
    }

    public class TheParseMethod
    {
        [Theory]
        [InlineData("")]
        [InlineData("tomorrow")]
        [InlineData("today-30")]
        [InlineData("today-30y")]
        [InlineData("today - 30d")]
        [InlineData("Today")]
        [InlineData("2026-13-01")]
        [InlineData("09/15/2021")]
        public void GivenUnrecognisedValue_WhenParsed_ThenThrowsFormatExceptionNamingTheValue(string value)
        {
            var act = () => SeedDate.Parse(value);

            act.Should().Throw<FormatException>().WithMessage($"*'{value}'*");
        }
    }

    public class TheJsonConverter
    {
        private sealed record Entry(SeedDate Date, SeedDate? Optional);

        [Fact]
        public void GivenTokenInJson_WhenDeserialized_ThenReadsSeedDate()
        {
            var entry = JsonSerializer.Deserialize<Entry>("""{ "Date": "today-1d", "Optional": null }""")!;

            entry.Date.Resolve(new DateOnly(2026, 11, 3)).Should().Be(new DateOnly(2026, 11, 2));
            entry.Optional.Should().BeNull();
        }

        [Fact]
        public void GivenBadTokenInJson_WhenDeserialized_ThenThrowsJsonException()
        {
            var act = () => JsonSerializer.Deserialize<Entry>("""{ "Date": "next week" }""");

            act.Should().Throw<JsonException>().WithMessage("*'next week'*");
        }
    }
}
