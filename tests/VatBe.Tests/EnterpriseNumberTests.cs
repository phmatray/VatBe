using FluentAssertions;
using VatBe.Models;
using Xunit;

namespace VatBe.Tests;

public sealed class EnterpriseNumberTests
{
    // --- Valid numbers ---

    [Theory]
    [InlineData("0402.206.045")]        // Delhaize Group
    [InlineData("0402206045")]          // No dots
    [InlineData("BE0402.206.045")]      // With BE prefix
    [InlineData("BE 0402.206.045")]     // With BE and space
    [InlineData("be0402206045")]        // Lowercase BE
    [InlineData("BE0402206045")]        // BE compact
    public void TryParse_DelhaizeGroup_ReturnsTrue(string input)
    {
        var result = EnterpriseNumber.TryParse(input, out var en);

        result.Should().BeTrue();
        en.Digits.Should().Be("0402206045");
    }

    [Theory]
    [InlineData("0403.199.702")]        // BNP Paribas Fortis
    [InlineData("0123.456.749")]        // Generated valid
    [InlineData("0200.000.043")]        // Generated valid
    [InlineData("0687.485.619")]        // Generated valid
    [InlineData("0500.000.059")]        // Generated valid
    [InlineData("0999.999.922")]        // Generated valid (high)
    public void TryParse_ValidNumbers_ReturnsTrue(string input)
    {
        var result = EnterpriseNumber.TryParse(input, out _);
        result.Should().BeTrue();
    }

    // --- Invalid numbers ---

    [Theory]
    [InlineData(null)]                  // null
    [InlineData("")]                    // empty
    [InlineData("   ")]                 // whitespace
    [InlineData("123")]                 // too short
    [InlineData("12345678901")]         // too long (11 digits)
    [InlineData("ABCDEFGHIJ")]          // non-numeric
    [InlineData("0402206040")]          // wrong check digits (Delhaize with wrong suffix)
    [InlineData("0000000000")]          // all zeros (mod 97 = 0, check = 97, but stored as 00)
    [InlineData("0123456789")]          // wrong check digits (ends in 89, should be 49)
    public void TryParse_InvalidNumbers_ReturnsFalse(string? input)
    {
        var result = EnterpriseNumber.TryParse(input, out _);
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_DelegatesToTryParse()
    {
        EnterpriseNumber.IsValid("0402.206.045").Should().BeTrue();
        EnterpriseNumber.IsValid("0000000000").Should().BeFalse();
        EnterpriseNumber.IsValid(null).Should().BeFalse();
    }

    [Fact]
    public void ToFormatted_ReturnsDotsFormat()
    {
        var en = EnterpriseNumber.Parse("0402206045");
        en.ToFormatted().Should().Be("0402.206.045");
    }

    [Fact]
    public void ToString_ReturnsFormattedVersion()
    {
        var en = EnterpriseNumber.Parse("0402206045");
        en.ToString().Should().Be("0402.206.045");
    }

    [Fact]
    public void ToVatNumber_ReturnsCorrectVatNumber()
    {
        var en = EnterpriseNumber.Parse("0402206045");
        var vat = en.ToVatNumber();

        vat.CountryCode.Should().Be("BE");
        vat.EnterpriseNumber.Should().Be(en);
        vat.ToFormatted().Should().Be("BE 0402.206.045");
    }

    [Fact]
    public void Parse_InvalidInput_ThrowsFormatException()
    {
        var act = () => EnterpriseNumber.Parse("not-a-number");
        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void TwoEqualNumbers_AreEqual()
    {
        var a = EnterpriseNumber.Parse("0402.206.045");
        var b = EnterpriseNumber.Parse("BE0402206045");

        a.Should().Be(b);
    }

    [Fact]
    public void Digits_AreTenCharactersLong()
    {
        var en = EnterpriseNumber.Parse("0123.456.749");
        en.Digits.Should().HaveLength(10);
    }
}
