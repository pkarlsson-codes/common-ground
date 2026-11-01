using CommonGround.PropertyRegistry.Domain.Homes;
using FluentAssertions;
using Xunit;

namespace CommonGround.PropertyRegistry.Tests.Domain.Homes;

public class HomeLabelTests
{
    public class TheFormatMethod
    {
        [Fact]
        public void GivenUnitDesignation_WhenFormatted_ThenAppendsItToTheAddress()
        {
            HomeLabel.Format("14 Alder Lane", "Unit B").Should().Be("14 Alder Lane, Unit B");
        }

        [Fact]
        public void GivenNoUnitDesignation_WhenFormatted_ThenIsTheAddress()
        {
            HomeLabel.Format("14 Alder Lane", null).Should().Be("14 Alder Lane");
        }
    }
}
