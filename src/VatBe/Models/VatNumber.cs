using System.Text.RegularExpressions;

namespace VatBe.Models;

/// <summary>
/// Belgian VAT number (BTW/TVA number).
/// Format: BE0XXXXXXXXX — same enterprise number prefixed with BE.
/// </summary>
public readonly record struct VatNumber
{
    /// <summary>Country code — always "BE" for Belgian VAT numbers.</summary>
    public string CountryCode => "BE";

    /// <summary>The underlying 10-digit enterprise number.</summary>
    public EnterpriseNumber EnterpriseNumber { get; }

    private VatNumber(EnterpriseNumber enterpriseNumber) =>
        EnterpriseNumber = enterpriseNumber;

    internal static VatNumber FromEnterpriseNumber(EnterpriseNumber en) => new(en);

    /// <summary>
    /// Parse a Belgian VAT number. Accepts multiple formats:
    /// BE0123456749, BE 0123.456.749, BE0123.456.749
    /// </summary>
    public static bool TryParse(string? input, out VatNumber result)
    {
        result = default;

        if (string.IsNullOrWhiteSpace(input))
            return false;

        var s = input.Trim();

        // Must start with BE (or be)
        if (!s.StartsWith("BE", StringComparison.OrdinalIgnoreCase))
            return false;

        // Delegate the rest to EnterpriseNumber (which handles BE prefix too)
        if (!EnterpriseNumber.TryParse(s, out var en))
            return false;

        result = new VatNumber(en);
        return true;
    }

    public static VatNumber Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new FormatException($"'{input}' is not a valid Belgian VAT number.");
        return result;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>Compact format: BE0XXXXXXXXX</summary>
    public string ToCompact() => $"BE{EnterpriseNumber.Digits}";

    /// <summary>Formatted: BE 0XXX.XXX.XXX</summary>
    public string ToFormatted() => $"BE {EnterpriseNumber.ToFormatted()}";

    /// <summary>EU VIES format (no spaces): BE0XXXXXXXXX</summary>
    public string ToViesFormat() => ToCompact();

    public override string ToString() => ToFormatted();
}
