using CommonGround.SharedKernel.Infrastructure.Seeding;
using FluentAssertions;

namespace CommonGround.SharedKernel.Tests.Infrastructure.Seeding;

public class SeedRunTests
{
    public class TheConstructor
    {
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("sandbox:1")]
        public void GivenBlankKeyOrKeyWithColon_WhenCreated_ThenThrows(string key)
        {
            var act = () => new SeedRun(key);

            act.Should().Throw<ArgumentException>();
        }
    }

    public class TheIsDevelopmentProperty
    {
        [Fact]
        public void GivenARunWithTheDevelopmentKey_ThenIsDevelopment()
        {
            new SeedRun("dev").IsDevelopment.Should().BeTrue();
        }

        [Fact]
        public void GivenARunWithAnotherKey_ThenIsNotDevelopment()
        {
            new SeedRun("sandbox-1").IsDevelopment.Should().BeFalse();
        }
    }
}
