using CommonGround.SharedKernel.Domain.Exceptions;
using FluentAssertions;

namespace CommonGround.SharedKernel.Tests.Domain.Exceptions;

public class BusinessRuleViolationExceptionTests
{
    public class TheConstructor
    {
        [Fact]
        public void GivenCodeOnly_WhenConstructed_ThenExposesCode()
        {
            var sut = new BusinessRuleViolationException("module.aggregate.rule");

            sut.Code.Should().Be("module.aggregate.rule");
        }
    }
}
