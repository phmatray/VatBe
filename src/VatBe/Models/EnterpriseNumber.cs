using System.Text.RegularExpressions;

namespace VatBe.Models;

/// <summary>
/// Belgian enterprise number (KBO/BCE number).
/// Format: 0XXX.XXX.XXX — always 10 digits, validated with Modulus 97.
/// </summary>
public readonly record struct EnterpriseNumber
{
    /// <summary>Raw 10-digit string (no dots or spaces).</summary>
    public string Digits { get; }

    private EnterpriseNumber(string digits) => Digits = digits;

    /// <summary>
    /// Parse and validate a Belgian enterprise number from various input formats.
    /// Accepts: 0123456749, 0123.456.749, BE0123456749, BE 0123.456.749
    /// </summary>
    public static bool TryParse(string? input, out EnterpriseNumber result)
    {
        result = default;

        if (string.IsNullOrWhiteSpace(input))
            return false;

        // Normalize: strip BE/be prefix, dots, spaces, dashes
        var clean = NormalizeInput(input);

        if (!IsValidDigits(clean))
            return false;

        result = new EnterpriseNumber(clean);
        return true;
    }

    /// <summary>
    /// Parse or throw. Prefer TryParse for user input.
    /// </summary>
    public static EnterpriseNumber Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new FormatException($"'{input}' is not a valid Belgian enterprise number.");
        return result;
    }

    /// <summary>Returns true if the input is a syntactically valid Belgian enterprise number.</summary>
    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>Formatted with dots: 0XXX.XXX.XXX</summary>
    public string ToFormatted() =>
        $"{Digits[0]}{Digits[1..4]}.{Digits[4..7]}.{Digits[7..]}";

    /// <summary>Belgian VAT number from this enterprise number (BE0XXXXXXXXX)</summary>
    public VatNumber ToVatNumber() => VatNumber.FromEnterpriseNumber(this);

    public override string ToString() => ToFormatted();

    // --- Private helpers ---

    private static string NormalizeInput(string input)
    {
        // Remove BE/be prefix (with optional space)
        var s = input.Trim();
        if (s.StartsWith("BE", StringComparison.OrdinalIgnoreCase))
            s = s[2..].TrimStart();

        // Remove dots, spaces, dashes, slashes
        return Regex.Replace(s, @"[\s.\-/]", "");
    }

    private static bool IsValidDigits(string digits)
    {
        // Must be exactly 10 numeric digits
        if (digits.Length != 10 || !digits.All(char.IsDigit))
            return false;

        // Must start with 0 (post-2003 all start with 0; old ones with 0 too after prefix)
        // The KBO started numbering at 0200.000.000 around 2003; before that: no leading 0
        // We accept both: 0XXXXXXXXX (new) and 1XXXXXXXXX (old, pre-2003)
        // But the check digit algorithm is the same regardless.

        // Modulus 97 validation:
        // The 8-digit body (first 8 digits) mod 97 should give remainder R
        // Check digits = 97 - R (zero-padded to 2 digits)
        var body = long.Parse(digits[..8]);
        var checkDigits = int.Parse(digits[8..]);
        var expected = 97 - (int)(body % 97);

        return expected == checkDigits;
    }
}
