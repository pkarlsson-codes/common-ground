using CommonGround.SharedKernel.Infrastructure.Seeding;
using CommonGround.TrustManagement.Infrastructure.Seeding;
using FluentAssertions;
using Xunit;

namespace CommonGround.TrustManagement.Tests.Infrastructure.Seeding;

public class TrustSeedLookupTests
{
    public class TheBuildIdMethod
    {
        [Fact]
        public void GivenTheDevelopmentRun_WhenBuilt_ThenKeepsTheIdFromBeforeRuns()
        {
            TrustSeedLookup.BuildId(SeedRun.Development, "dev-default").Should().Be("trust-seed:dev-default");
        }

        [Fact]
        public void GivenTwoOtherRuns_WhenBuilt_ThenEachRunHasItsOwnId()
        {
            var first = TrustSeedLookup.BuildId(new SeedRun("sandbox-1"), "dev-default");
            var second = TrustSeedLookup.BuildId(new SeedRun("sandbox-2"), "dev-default");

            first.Should().Be("trust-seed:sandbox-1:dev-default");
            second.Should().NotBe(first);
        }
    }
}
