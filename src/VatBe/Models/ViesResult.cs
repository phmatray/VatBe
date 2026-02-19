namespace VatBe.Models;

/// <summary>
/// Result from the EU VIES VAT validation service.
/// </summary>
public record ViesResult
{
    /// <summary>Whether the VAT number is currently valid in VIES.</summary>
    public bool IsValid { get; init; }

    /// <summary>The queried country code (e.g. "BE").</summary>
    public string CountryCode { get; init; } = string.Empty;

    /// <summary>The queried VAT number (without country code).</summary>
    public string VatNumber { get; init; } = string.Empty;

    /// <summary>Registered company name (if returned by VIES).</summary>
    public string? TraderName { get; init; }

    /// <summary>Registered company address (if returned by VIES).</summary>
    public string? TraderAddress { get; init; }

    /// <summary>Date of validation (UTC).</summary>
    public DateTimeOffset ValidatedAt { get; init; }

    /// <summary>The raw request identifier from VIES.</summary>
    public string? RequestIdentifier { get; init; }

    /// <summary>Error message if VIES returned an error (service unavailable, etc.).</summary>
    public string? Error { get; init; }

    public override string ToString() =>
        IsValid
            ? $"✓ VALID — {CountryCode}{VatNumber} | {TraderName} | {TraderAddress}"
            : $"✗ INVALID — {CountryCode}{VatNumber}{(Error != null ? $" ({Error})" : "")}";
}
