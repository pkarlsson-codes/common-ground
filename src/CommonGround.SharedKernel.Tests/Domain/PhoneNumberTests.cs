using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;

namespace CommonGround.SharedKernel.Tests.Domain;

public class PhoneNumberTests
{
    [Theory]
    [InlineData("(202) 555-1234")]
    [InlineData("202-555-1234")]
    [InlineData("202.555.1234")]
    [InlineData("2025551234")]
    [InlineData("1-202-555-1234")]
    public void Constructor_UsLocalFormats_NormalizesToE164(string input)
    {
        var phone = new PhoneNumber(input);

        Assert.Equal("+12025551234", phone.Value);
    }

    [Fact]
    public void Constructor_E164Input_PassesThrough()
    {
        var phone = new PhoneNumber("+12025551234");

        Assert.Equal("+12025551234", phone.Value);
    }

    [Fact]
    public void Constructor_InternationalInput_NormalizesToE164()
    {
        var phone = new PhoneNumber("+44 20 7946 0958");

        Assert.Equal("+442079460958", phone.Value);
    }

    [Theory]
    [InlineData("garbage")]
    [InlineData("123")]
    [InlineData("+1")]
    [InlineData("555-0100-99999999")]
    public void Constructor_InvalidInput_ThrowsBusinessRuleViolation(string input)
    {
        var exception = Assert.Throws<BusinessRuleViolationException>(() => new PhoneNumber(input));

        Assert.Equal("shared.phone.invalid_format", exception.Code);
    }

    [Fact]
    public void Constructor_WhitespaceInput_ThrowsBusinessRuleViolation()
    {
        var exception = Assert.Throws<BusinessRuleViolationException>(() => new PhoneNumber("   "));

        Assert.Equal("shared.phone.required", exception.Code);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void FromOptional_NoInput_ReturnsNull(string? input)
    {
        Assert.Null(PhoneNumber.FromOptional(input));
    }

    [Fact]
    public void FromOptional_ValidInput_ReturnsParsedPhoneNumber()
    {
        var phone = PhoneNumber.FromOptional("(202) 555-1234");

        Assert.Equal("+12025551234", phone?.Value);
    }

    [Fact]
    public void Equality_SameCanonicalValue_AreEqual()
    {
        var a = new PhoneNumber("(202) 555-1234");
        var b = new PhoneNumber("+12025551234");

        Assert.Equal(a, b);
    }

    [Fact]
    public void ToString_ReturnsE164Value()
    {
        Assert.Equal("+12025551234", new PhoneNumber("2025551234").ToString());
    }
}
