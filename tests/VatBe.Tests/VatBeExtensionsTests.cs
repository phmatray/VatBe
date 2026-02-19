using FluentAssertions;
using VatBe.Models;
using Xunit;

namespace VatBe.Tests;

public sealed class VatBeExtensionsTests
{
    [Fact]
    public void IsBelgianEnterpriseNumber_Valid_ReturnsTrue()
    {
        "0402.206.045".IsBelgianEnterpriseNumber().Should().BeTrue();
    }

    [Fact]
    public void IsBelgianEnterpriseNumber_Invalid_ReturnsFalse()
    {
        "not-a-number".IsBelgianEnterpriseNumber().Should().BeFalse();
    }

    [Fact]
    public void FormatAsEnterpriseNumber_FormatsCorrectly()
    {
        "0402206045".FormatAsEnterpriseNumber().Should().Be("0402.206.045");
    }

    [Fact]
    public void FormatAsEnterpriseNumber_InvalidInput_Throws()
    {
        var act = () => "invalid".FormatAsEnterpriseNumber();
        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void IsBelgianVatNumber_Valid_ReturnsTrue()
    {
        "BE0402.206.045".IsBelgianVatNumber().Should().BeTrue();
    }

    [Fact]
    public void IsBelgianVatNumber_WithoutPrefix_ReturnsFalse()
    {
        "0402.206.045".IsBelgianVatNumber().Should().BeFalse();
    }

    [Fact]
    public void WithBelgianVat_Fluent_CalculatesVat()
    {
        var calc = 100m.WithBelgianVat(VatRateCategory.Electronics);

        calc.VatAmount.Should().Be(21m);
        calc.AmountInclVat.Should().Be(121m);
    }

    [Fact]
    public void ExtractBelgianVat_Fluent_ExtractsVat()
    {
        var calc = 121m.ExtractBelgianVat(VatRateCategory.Electronics);

        calc.AmountExclVat.Should().Be(100m);
        calc.VatAmount.Should().Be(21m);
    }

    [Fact]
    public void GetBelgianVatRate_ReturnsCorrectRate()
    {
        VatRateCategory.BasicFood.GetBelgianVatRate().Should().Be(VatRate.Reduced);
        VatRateCategory.Electronics.GetBelgianVatRate().Should().Be(VatRate.Standard);
    }
}
