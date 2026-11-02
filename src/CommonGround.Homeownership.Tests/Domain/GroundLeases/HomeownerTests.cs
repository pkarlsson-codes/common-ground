using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.SharedKernel.Domain.Exceptions;
using FluentAssertions;

namespace CommonGround.Homeownership.Tests.Domain.GroundLeases;

public class HomeownerTests
{
    [Fact]
    public void Constructor_E164Phone_IsPreserved()
    {
        var homeowner = Homeowner.From("Alice Johnson", "alice@example.com", "+12025551234");

        homeowner.Phone!.Value.Should().Be("+12025551234");
    }

    [Fact]
    public void Constructor_UsLocalPhone_NormalizesToE164()
    {
        var homeowner = Homeowner.From("Alice Johnson", "alice@example.com", "(202) 555-1234");

        homeowner.Phone!.Value.Should().Be("+12025551234");
    }

    [Fact]
    public void Constructor_NoPhone_LeavesPhoneNull()
    {
        var homeowner = Homeowner.From("Alice Johnson", "alice@example.com");

        homeowner.Phone.Should().BeNull();
    }

    [Fact]
    public void Constructor_InvalidPhone_Throws()
    {
        var act = () => Homeowner.From("Alice Johnson", "alice@example.com", "not-a-phone");

        act.Should().Throw<BusinessRuleViolationException>()
            .Which.Code.Should().Be("shared.phone.invalid_format");
    }

    [Fact]
    public void Equality_SamePhoneInDifferentInputFormats_AreEqual()
    {
        var a = Homeowner.From("Alice Johnson", "alice@example.com", "+12025551234");
        var b = Homeowner.From("Alice Johnson", "alice@example.com", "202-555-1234");

        a.Should().Be(b);
    }
}
