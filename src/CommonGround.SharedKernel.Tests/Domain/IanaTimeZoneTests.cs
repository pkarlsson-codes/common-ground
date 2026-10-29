using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using FluentAssertions;

namespace CommonGround.SharedKernel.Tests.Domain;

public class IanaTimeZoneTests
{
    public class TheCreateMethod
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public void GivenNoValue_WhenCreated_ThenThrowsRequired(string? value)
        {
            var act = () => IanaTimeZone.Create(value);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("shared.time_zone.required");
        }

        [Theory]
        [InlineData("Mars/Olympus_Mons")]
        [InlineData("Mountain Standard Time")]
        public void GivenNonIanaValue_WhenCreated_ThenThrowsUnknown(string value)
        {
            var act = () => IanaTimeZone.Create(value);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("shared.time_zone.unknown");
        }

        [Fact]
        public void GivenIanaId_WhenCreated_ThenKeepsId()
        {
            var sut = IanaTimeZone.Create("America/Denver");

            sut.Value.Should().Be("America/Denver");
        }
    }

    public class TheToDateMethod
    {
        [Fact]
        public void GivenEveningInDenver_WhenConverted_ThenReturnsDenverDateNotUtcDate()
        {
            var sut = IanaTimeZone.Create("America/Denver");
            var eveningInDenver = new DateTimeOffset(2026, 10, 29, 3, 0, 0, TimeSpan.Zero);

            sut.ToDate(eveningInDenver).Should().Be(new DateOnly(2026, 10, 28));
        }

        [Fact]
        public void GivenMorningInAuckland_WhenConverted_ThenReturnsAucklandDate()
        {
            var sut = IanaTimeZone.Create("Pacific/Auckland");
            var lateUtcEvening = new DateTimeOffset(2026, 10, 28, 20, 0, 0, TimeSpan.Zero);

            sut.ToDate(lateUtcEvening).Should().Be(new DateOnly(2026, 10, 29));
        }
    }
}
