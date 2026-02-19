using FluentAssertions;
using VatBe.Calculation;
using VatBe.Models;
using Xunit;

namespace VatBe.Tests;

public sealed class VatCalculatorTests
{
    // --- FromExclVat ---

    [Fact]
    public void FromExclVat_StandardRate_CalculatesCorrectly()
    {
        var result = VatCalculator.FromExclVat(100m, VatRateCategory.Electronics);

        result.Rate.Should().Be(VatRate.Standard);
        result.AmountExclVat.Should().Be(100m);
        result.VatAmount.Should().Be(21m);
        result.AmountInclVat.Should().Be(121m);
        result.RatePercentage.Should().Be(21m);
    }

    [Fact]
    public void FromExclVat_ReducedRate_CalculatesCorrectly()
    {
        var result = VatCalculator.FromExclVat(100m, VatRateCategory.BasicFood);

        result.Rate.Should().Be(VatRate.Reduced);
        result.AmountExclVat.Should().Be(100m);
        result.VatAmount.Should().Be(6m);
        result.AmountInclVat.Should().Be(106m);
    }

    [Fact]
    public void FromExclVat_IntermediateRate_CalculatesCorrectly()
    {
        var result = VatCalculator.FromExclVat(100m, VatRateCategory.RestaurantFood);

        result.Rate.Should().Be(VatRate.Intermediate);
        result.VatAmount.Should().Be(12m);
        result.AmountInclVat.Should().Be(112m);
    }

    [Fact]
    public void FromExclVat_ZeroRate_CalculatesCorrectly()
    {
        var result = VatCalculator.FromExclVat(100m, VatRateCategory.ExportOutsideEU);

        result.Rate.Should().Be(VatRate.Zero);
        result.VatAmount.Should().Be(0m);
        result.AmountInclVat.Should().Be(100m);
    }

    [Fact]
    public void FromExclVat_RoundingApplied()
    {
        // 33.33 * 21% = 6.9993 → rounds to 7.00
        var result = VatCalculator.FromExclVat(33.33m, VatRateCategory.Standard);

        result.VatAmount.Should().Be(7.00m);
        result.AmountInclVat.Should().Be(40.33m);
    }

    [Fact]
    public void FromExclVat_NegativeAmount_Throws()
    {
        var act = () => VatCalculator.FromExclVat(-10m, VatRateCategory.Standard);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void FromExclVat_ZeroAmount_ReturnsZeros()
    {
        var result = VatCalculator.FromExclVat(0m, VatRateCategory.Standard);

        result.AmountExclVat.Should().Be(0m);
        result.VatAmount.Should().Be(0m);
        result.AmountInclVat.Should().Be(0m);
    }

    // --- FromInclVat ---

    [Fact]
    public void FromInclVat_StandardRate_CalculatesCorrectly()
    {
        // 121 incl 21% → excl = 100, vat = 21
        var result = VatCalculator.FromInclVat(121m, VatRateCategory.Electronics);

        result.AmountInclVat.Should().Be(121m);
        result.AmountExclVat.Should().Be(100m);
        result.VatAmount.Should().Be(21m);
    }

    [Fact]
    public void FromInclVat_ReducedRate_CalculatesCorrectly()
    {
        // 106 incl 6% → excl = 100, vat = 6
        var result = VatCalculator.FromInclVat(106m, VatRateCategory.BasicFood);

        result.AmountExclVat.Should().Be(100m);
        result.VatAmount.Should().Be(6m);
    }

    [Fact]
    public void FromInclVat_NegativeAmount_Throws()
    {
        var act = () => VatCalculator.FromInclVat(-10m, VatRateCategory.Standard);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    // --- FromExclVatWithRate ---

    [Fact]
    public void FromExclVatWithRate_UsesExplicitRate()
    {
        var result = VatCalculator.FromExclVatWithRate(200m, VatRate.Reduced);

        result.Rate.Should().Be(VatRate.Reduced);
        result.VatAmount.Should().Be(12m);
        result.AmountInclVat.Should().Be(212m);
    }

    // --- CalculateInvoice ---

    [Fact]
    public void CalculateInvoice_MultipleLines_SumsCorrectly()
    {
        var lines = new[]
        {
            new InvoiceLine { Description = "Laptop",   AmountExclVat = 1000m, Category = VatRateCategory.Electronics },
            new InvoiceLine { Description = "Bread",    AmountExclVat = 5m,    Category = VatRateCategory.BasicFood },
            new InvoiceLine { Description = "Meal",     AmountExclVat = 20m,   Category = VatRateCategory.RestaurantFood },
        };

        var totals = VatCalculator.CalculateInvoice(lines);

        totals.TotalExclVat.Should().Be(1025m);
        totals.TotalVat.Should().Be(1000m * 0.21m + 5m * 0.06m + 20m * 0.12m);
        totals.VatByRate.Should().HaveCount(3);
    }

    [Fact]
    public void CalculateInvoice_VatGroupedByRate()
    {
        var lines = new[]
        {
            new InvoiceLine { Description = "Item A", AmountExclVat = 100m, Category = VatRateCategory.Electronics },
            new InvoiceLine { Description = "Item B", AmountExclVat = 200m, Category = VatRateCategory.Clothing },   // also 21%
        };

        var totals = VatCalculator.CalculateInvoice(lines);

        totals.VatByRate.Should().HaveCount(1); // both grouped under 21%
        totals.VatByRate[0].Rate.Should().Be(VatRate.Standard);
        totals.VatByRate[0].BaseAmount.Should().Be(300m);
        totals.VatByRate[0].VatAmount.Should().Be(63m);
    }
}
