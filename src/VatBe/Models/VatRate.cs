namespace VatBe.Models;

/// <summary>Belgian VAT rates (as of 2026).</summary>
public enum VatRate
{
    /// <summary>0% — Used goods, certain exports, etc.</summary>
    Zero = 0,

    /// <summary>6% — Food, medicine, books, agricultural products, social housing renovation.</summary>
    Reduced = 6,

    /// <summary>12% — Restaurant meals (food portion), coal, margarine, phytopharmaceuticals.</summary>
    Intermediate = 12,

    /// <summary>21% — Standard rate for everything not in reduced categories.</summary>
    Standard = 21,
}

/// <summary>VAT rate category mapping for common goods/services.</summary>
public enum VatRateCategory
{
    // 0%
    UsedGoods,
    ExportOutsideEU,

    // 6%
    BasicFood,
    Pharmaceutical,
    Books,
    Newspapers,
    AgriculturalProducts,
    WaterSupply,
    HotelAccommodation,
    PublicTransport,
    SocialHousingConstruction,
    SocialHousingRenovation,
    ResidentialRenovation,   // buildings > 10 years

    // 12%
    RestaurantFood,          // food component of restaurant bill (drinks stay at 21%)
    Coal,
    SocialHousing,           // certain new social housing
    Margarine,
    Phytopharmaceuticals,

    // 21%
    Standard,                // catch-all
    DigitalServices,
    Clothing,
    Electronics,
    Vehicles,
    RealEstateNew,           // new buildings
    Alcohol,
    Tobacco,
    RestaurantDrinks,
}
