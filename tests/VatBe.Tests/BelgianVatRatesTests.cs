using FluentAssertions;
using VatBe.Calculation;
using VatBe.Models;
using Xunit;

namespace VatBe.Tests;

public sealed class BelgianVatRatesTests
{
    [Theory]
    [InlineData(VatRateCategory.UsedGoods, VatRate.Zero)]
    [InlineData(VatRateCategory.ExportOutsideEU, VatRate.Zero)]
    [InlineData(VatRateCategory.BasicFood, VatRate.Reduced)]
    [InlineData(VatRateCategory.Pharmaceutical, VatRate.Reduced)]
    [InlineData(VatRateCategory.Books, VatRate.Reduced)]
    [InlineData(VatRateCategory.HotelAccommodation, VatRate.Reduced)]
    [InlineData(VatRateCategory.ResidentialRenovation, VatRate.Reduced)]
    [InlineData(VatRateCategory.RestaurantFood, VatRate.Intermediate)]
    [InlineData(VatRateCategory.Coal, VatRate.Intermediate)]
    [InlineData(VatRateCategory.Standard, VatRate.Standard)]
    [InlineData(VatRateCategory.Electronics, VatRate.Standard)]
    [InlineData(VatRateCategory.DigitalServices, VatRate.Standard)]
    [InlineData(VatRateCategory.Alcohol, VatRate.Standard)]
    public void GetRate_ReturnsCorrectRateForCategory(VatRateCategory category, VatRate expectedRate)
    {
        BelgianVatRates.GetRate(category).Should().Be(expectedRate);
    }

    [Theory]
    [InlineData(VatRateCategory.Standard, 0.21)]
    [InlineData(VatRateCategory.BasicFood, 0.06)]
    [InlineData(VatRateCategory.RestaurantFood, 0.12)]
    [InlineData(VatRateCategory.ExportOutsideEU, 0.00)]
    public void GetRateDecimal_ReturnsCorrectDecimalFraction(VatRateCategory category, double expected)
    {
        var rate = BelgianVatRates.GetRateDecimal(category);
        rate.Should().BeApproximately((decimal)expected, 0.0001m);
    }

    [Fact]
    public void GetCategoriesForRate_Standard_ReturnsMultipleCategories()
    {
        var categories = BelgianVatRates.GetCategoriesForRate(VatRate.Standard).ToList();
        categories.Should().Contain(VatRateCategory.Electronics);
        categories.Should().Contain(VatRateCategory.Clothing);
        categories.Should().Contain(VatRateCategory.Standard);
    }

    [Fact]
    public void GetCategoriesForRate_Zero_ReturnsOnlyZeroCategories()
    {
        var categories = BelgianVatRates.GetCategoriesForRate(VatRate.Zero).ToList();
        categories.Should().OnlyContain(c =>
            c == VatRateCategory.UsedGoods || c == VatRateCategory.ExportOutsideEU);
    }

    [Fact]
    public void AllRates_CoversAllCategoriesInEnum()
    {
        var allDefined = Enum.GetValues<VatRateCategory>();
        var covered = BelgianVatRates.AllRates.Keys;
        covered.Should().Contain(allDefined, "every VatRateCategory must have a rate defined");
    }

    [Fact]
    public void RateValues_AreValidBelgianRates()
    {
        var validRates = new[] { 0, 6, 12, 21 };
        foreach (var rate in BelgianVatRates.AllRates.Values)
        {
            validRates.Should().Contain((int)rate);
        }
    }
}
