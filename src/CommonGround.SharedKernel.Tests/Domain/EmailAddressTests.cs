using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;

namespace CommonGround.SharedKernel.Tests.Domain;

public class EmailAddressTests
{
    [Theory]
    [InlineData("jordan@example.com")]
    [InlineData("jordan.brooks+clt@mail.example.org")]
    [InlineData("jordan@localhost")]
    public void Create_AddressShapedInput_KeepsIt(string input)
    {
        var email = EmailAddress.Create(input);

        Assert.Equal(input, email.Value);
    }

    [Fact]
    public void Create_SurroundingWhitespace_IsTrimmed()
    {
        var email = EmailAddress.Create("  jordan@example.com ");

        Assert.Equal("jordan@example.com", email.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_NoInput_ThrowsRequired(string? input)
    {
        var exception = Assert.Throws<BusinessRuleViolationException>(() => EmailAddress.Create(input));

        Assert.Equal("shared.email.required", exception.Code);
    }

    [Theory]
    [InlineData("jordan.example.com")]
    [InlineData("@example.com")]
    [InlineData("jordan@")]
    [InlineData("jordan@@example.com")]
    [InlineData("jordan@home@example.com")]
    [InlineData("jordan brooks@example.com")]
    public void Create_InputNotShapedLikeAnAddress_ThrowsInvalidFormat(string input)
    {
        var exception = Assert.Throws<BusinessRuleViolationException>(() => EmailAddress.Create(input));

        Assert.Equal("shared.email.invalid_format", exception.Code);
    }

    [Fact]
    public void Create_LongerThanSmtpAllows_ThrowsInvalidFormat()
    {
        var input = new string('j', EmailAddress.MaxLength - "@example.com".Length + 1) + "@example.com";

        var exception = Assert.Throws<BusinessRuleViolationException>(() => EmailAddress.Create(input));

        Assert.Equal("shared.email.invalid_format", exception.Code);
    }
}
