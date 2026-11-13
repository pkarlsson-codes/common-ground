using CommonGround.SharedKernel.Domain;
using FluentAssertions;

namespace CommonGround.SharedKernel.Tests.Domain;

public class IncomeBandTests
{
    public class TheCreateMethod
    {
        [Fact]
        public void GivenNoBounds_WhenCreated_ThenThrows()
        {
            var act = () => IncomeBand.Create(null, null);

            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void GivenALowerLevelAboveTheUpper_WhenCreated_ThenThrows()
        {
            var act = () => IncomeBand.Create(80, 50);

            act.Should().Throw<ArgumentException>();
        }
    }
}
