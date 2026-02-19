using VatBe.Calculation;
using VatBe.Models;

namespace VatBe;

/// <summary>
/// Fluent extension methods for common VatBe operations.
/// </summary>
public static class VatBeExtensions
{
    // --- EnterpriseNumber extensions ---

    /// <summary>Try to parse a Belgian enterprise number (KBO/BCE) from a string.</summary>
    public static bool IsBelgianEnterpriseNumber(this string value) =>
        EnterpriseNumber.IsValid(value);

    /// <summary>Format a Belgian enterprise number (KBO/BCE) with dots: 0XXX.XXX.XXX</summary>
    public static string FormatAsEnterpriseNumber(this string value)
    {
        if (!EnterpriseNumber.TryParse(value, out var en))
            throw new FormatException($"'{value}' is not a valid Belgian enterprise number.");
        return en.ToFormatted();
    }

    // --- VatNumber extensions ---

    /// <summary>Try to parse a Belgian VAT number (BTW/TVA) from a string.</summary>
    public static bool IsBelgianVatNumber(this string value) =>
        VatNumber.IsValid(value);

    // --- VatCalculator extensions ---

    /// <summary>Calculate VAT-inclusive amount from a VAT-exclusive amount.</summary>
    public static VatCalculation WithBelgianVat(this decimal amountExclVat, VatRateCategory category) =>
        VatCalculator.FromExclVat(amountExclVat, category);

    /// <summary>Extract VAT from a VAT-inclusive amount.</summary>
    public static VatCalculation ExtractBelgianVat(this decimal amountInclVat, VatRateCategory category) =>
        VatCalculator.FromInclVat(amountInclVat, category);

    /// <summary>Get the Belgian VAT rate for a category.</summary>
    public static VatRate GetBelgianVatRate(this VatRateCategory category) =>
        BelgianVatRates.GetRate(category);
}
