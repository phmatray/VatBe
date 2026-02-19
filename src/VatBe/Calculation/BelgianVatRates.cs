using VatBe.Models;

namespace VatBe.Calculation;

/// <summary>
/// Maps <see cref="VatRateCategory"/> to the applicable Belgian <see cref="VatRate"/>.
/// Source: SPF Finances / FOD Financiën — applicable as of 2026.
/// </summary>
public static class BelgianVatRates
{
    private static readonly Dictionary<VatRateCategory, VatRate> _rates = new()
    {
        // 0%
        [VatRateCategory.UsedGoods]                  = VatRate.Zero,
        [VatRateCategory.ExportOutsideEU]            = VatRate.Zero,

        // 6%
        [VatRateCategory.BasicFood]                  = VatRate.Reduced,
        [VatRateCategory.Pharmaceutical]             = VatRate.Reduced,
        [VatRateCategory.Books]                      = VatRate.Reduced,
        [VatRateCategory.Newspapers]                 = VatRate.Reduced,
        [VatRateCategory.AgriculturalProducts]       = VatRate.Reduced,
        [VatRateCategory.WaterSupply]                = VatRate.Reduced,
        [VatRateCategory.HotelAccommodation]         = VatRate.Reduced,
        [VatRateCategory.PublicTransport]            = VatRate.Reduced,
        [VatRateCategory.SocialHousingConstruction]  = VatRate.Reduced,
        [VatRateCategory.SocialHousingRenovation]    = VatRate.Reduced,
        [VatRateCategory.ResidentialRenovation]      = VatRate.Reduced,

        // 12%
        [VatRateCategory.RestaurantFood]             = VatRate.Intermediate,
        [VatRateCategory.Coal]                       = VatRate.Intermediate,
        [VatRateCategory.SocialHousing]              = VatRate.Intermediate,
        [VatRateCategory.Margarine]                  = VatRate.Intermediate,
        [VatRateCategory.Phytopharmaceuticals]       = VatRate.Intermediate,

        // 21%
        [VatRateCategory.Standard]                   = VatRate.Standard,
        [VatRateCategory.DigitalServices]            = VatRate.Standard,
        [VatRateCategory.Clothing]                   = VatRate.Standard,
        [VatRateCategory.Electronics]                = VatRate.Standard,
        [VatRateCategory.Vehicles]                   = VatRate.Standard,
        [VatRateCategory.RealEstateNew]              = VatRate.Standard,
        [VatRateCategory.Alcohol]                    = VatRate.Standard,
        [VatRateCategory.Tobacco]                    = VatRate.Standard,
        [VatRateCategory.RestaurantDrinks]           = VatRate.Standard,
    };

    /// <summary>Get the VAT rate for a given category.</summary>
    public static VatRate GetRate(VatRateCategory category)
    {
        if (_rates.TryGetValue(category, out var rate))
            return rate;

        // Default to standard rate if category not found
        return VatRate.Standard;
    }

    /// <summary>Get the decimal rate percentage (e.g. 0.21m for Standard).</summary>
    public static decimal GetRateDecimal(VatRateCategory category) =>
        (decimal)GetRate(category) / 100m;

    /// <summary>Get all categories for a given rate.</summary>
    public static IEnumerable<VatRateCategory> GetCategoriesForRate(VatRate rate) =>
        _rates
            .Where(kvp => kvp.Value == rate)
            .Select(kvp => kvp.Key);

    /// <summary>All supported categories and their rates.</summary>
    public static IReadOnlyDictionary<VatRateCategory, VatRate> AllRates =>
        _rates.AsReadOnly();
}
