using Shouldly;
using VatBe.Models;
using Xunit;

namespace VatBe.Tests;

public sealed class VatBeExtensionsTests
{
    [Fact]
    public void IsBelgianEnterpriseNumber_Valid_ReturnsTrue()
    {
        "0402.206.045".IsBelgianEnterpriseNumber().ShouldBeTrue();
    }

    [Fact]
    public void IsBelgianEnterpriseNumber_Invalid_ReturnsFalse()
    {
        "not-a-number".IsBelgianEnterpriseNumber().ShouldBeFalse();
    }

    [Fact]
    public void FormatAsEnterpriseNumber_FormatsCorrectly()
    {
        "0402206045".FormatAsEnterpriseNumber().ShouldBe("0402.206.045");
    }

    [Fact]
    public void FormatAsEnterpriseNumber_InvalidInput_Throws()
    {
        var act = () => "invalid".FormatAsEnterpriseNumber();
        Should.Throw<FormatException>(act);
    }

    [Fact]
    public void IsBelgianVatNumber_Valid_ReturnsTrue()
    {
        "BE0402.206.045".IsBelgianVatNumber().ShouldBeTrue();
    }

    [Fact]
    public void IsBelgianVatNumber_WithoutPrefix_ReturnsFalse()
    {
        "0402.206.045".IsBelgianVatNumber().ShouldBeFalse();
    }

    [Fact]
    public void WithBelgianVat_Fluent_CalculatesVat()
    {
        var calc = 100m.WithBelgianVat(VatRateCategory.Electronics);

        calc.VatAmount.ShouldBe(21m);
        calc.AmountInclVat.ShouldBe(121m);
    }

    [Fact]
    public void ExtractBelgianVat_Fluent_ExtractsVat()
    {
        var calc = 121m.ExtractBelgianVat(VatRateCategory.Electronics);

        calc.AmountExclVat.ShouldBe(100m);
        calc.VatAmount.ShouldBe(21m);
    }

    [Fact]
    public void GetBelgianVatRate_ReturnsCorrectRate()
    {
        VatRateCategory.BasicFood.GetBelgianVatRate().ShouldBe(VatRate.Reduced);
        VatRateCategory.Electronics.GetBelgianVatRate().ShouldBe(VatRate.Standard);
    }
}
