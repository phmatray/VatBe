using FluentAssertions;
using VatBe.Models;
using Xunit;

namespace VatBe.Tests;

public sealed class VatNumberTests
{
    [Theory]
    [InlineData("BE0402.206.045")]
    [InlineData("BE0402206045")]
    [InlineData("BE 0402.206.045")]
    [InlineData("be0402206045")]
    public void TryParse_ValidBelgianVatNumbers_ReturnsTrue(string input)
    {
        var result = VatNumber.TryParse(input, out var vat);

        result.Should().BeTrue();
        vat.CountryCode.Should().Be("BE");
        vat.EnterpriseNumber.Digits.Should().Be("0402206045");
    }

    [Theory]
    [InlineData(null)]                    // null
    [InlineData("")]                      // empty
    [InlineData("0402206045")]            // missing BE prefix → not a VAT number
    [InlineData("0402.206.045")]          // missing BE prefix
    [InlineData("FR12345678901")]         // French VAT (not Belgian format)
    [InlineData("BE0000000000")]          // all zeros
    [InlineData("BEINVALID")]             // non-numeric
    public void TryParse_InvalidVatNumbers_ReturnsFalse(string? input)
    {
        var result = VatNumber.TryParse(input, out _);
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WorksCorrectly()
    {
        VatNumber.IsValid("BE0402.206.045").Should().BeTrue();
        VatNumber.IsValid("0402.206.045").Should().BeFalse(); // no BE prefix
        VatNumber.IsValid(null).Should().BeFalse();
    }

    [Fact]
    public void ToCompact_ReturnsNoSpaceOrDots()
    {
        var vat = VatNumber.Parse("BE0402.206.045");
        vat.ToCompact().Should().Be("BE0402206045");
    }

    [Fact]
    public void ToFormatted_ReturnsReadableFormat()
    {
        var vat = VatNumber.Parse("BE0402206045");
        vat.ToFormatted().Should().Be("BE 0402.206.045");
    }

    [Fact]
    public void ToViesFormat_IsCompact()
    {
        var vat = VatNumber.Parse("BE0402206045");
        vat.ToViesFormat().Should().Be("BE0402206045");
    }

    [Fact]
    public void ToString_ReturnsFormattedVersion()
    {
        var vat = VatNumber.Parse("BE0402206045");
        vat.ToString().Should().Be("BE 0402.206.045");
    }

    [Fact]
    public void Parse_InvalidInput_ThrowsFormatException()
    {
        var act = () => VatNumber.Parse("not-a-vat-number");
        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void EnterpriseNumber_ToVatNumber_And_Back_AreConsistent()
    {
        var en = EnterpriseNumber.Parse("0402206045");
        var vat = en.ToVatNumber();
        var back = vat.EnterpriseNumber;

        back.Should().Be(en);
        vat.CountryCode.Should().Be("BE");
    }
}
