using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;

namespace CommonGround.SharedKernel.Tests.Domain;

public class PersonNameTests
{
    [Fact]
    public void Create_Name_KeepsItTrimmed()
    {
        var name = PersonName.Create("  Jordan Brooks ");

        Assert.Equal("Jordan Brooks", name.Value);
    }

    [Fact]
    public void Create_AtMaxLength_KeepsIt()
    {
        var input = new string('J', PersonName.MaxLength);

        Assert.Equal(input, PersonName.Create(input).Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_NoInput_ThrowsRequired(string? input)
    {
        var exception = Assert.Throws<BusinessRuleViolationException>(() => PersonName.Create(input));

        Assert.Equal("shared.person_name.required", exception.Code);
    }

    [Fact]
    public void Create_LongerThanMaxLength_ThrowsTooLong()
    {
        var exception = Assert.Throws<BusinessRuleViolationException>(
            () => PersonName.Create(new string('J', PersonName.MaxLength + 1)));

        Assert.Equal("shared.person_name.too_long", exception.Code);
    }
}
