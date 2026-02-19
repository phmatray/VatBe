using VatBe.Models;

namespace VatBe.Calculation;

/// <summary>
/// Belgian VAT calculator. Supports excl→incl and incl→excl directions.
/// All amounts are in EUR. Rounding follows Belgian fiscal rules (2 decimal places, round half up).
/// </summary>
public static class VatCalculator
{
    /// <summary>
    /// Calculate VAT from an amount excluding VAT.
    /// </summary>
    public static VatCalculation FromExclVat(
        decimal amountExclVat,
        VatRateCategory category)
    {
        if (amountExclVat < 0)
            throw new ArgumentOutOfRangeException(nameof(amountExclVat), "Amount must be non-negative.");

        var rate = BelgianVatRates.GetRate(category);
        var rateDecimal = (decimal)rate / 100m;
        var vatAmount = RoundBelgian(amountExclVat * rateDecimal);
        var amountInclVat = amountExclVat + vatAmount;

        return new VatCalculation
        {
            AmountExclVat = amountExclVat,
            VatAmount = vatAmount,
            AmountInclVat = amountInclVat,
            Rate = rate,
            Category = category,
        };
    }

    /// <summary>
    /// Calculate VAT from an amount including VAT (reverse calculation).
    /// </summary>
    public static VatCalculation FromInclVat(
        decimal amountInclVat,
        VatRateCategory category)
    {
        if (amountInclVat < 0)
            throw new ArgumentOutOfRangeException(nameof(amountInclVat), "Amount must be non-negative.");

        var rate = BelgianVatRates.GetRate(category);
        var rateDecimal = (decimal)rate / 100m;
        var divisor = 1m + rateDecimal;
        var amountExclVat = RoundBelgian(amountInclVat / divisor);
        var vatAmount = amountInclVat - amountExclVat;

        return new VatCalculation
        {
            AmountExclVat = amountExclVat,
            VatAmount = RoundBelgian(vatAmount),
            AmountInclVat = amountInclVat,
            Rate = rate,
            Category = category,
        };
    }

    /// <summary>
    /// Calculate VAT using an explicit rate (for non-standard scenarios).
    /// </summary>
    public static VatCalculation FromExclVatWithRate(
        decimal amountExclVat,
        VatRate rate)
    {
        if (amountExclVat < 0)
            throw new ArgumentOutOfRangeException(nameof(amountExclVat), "Amount must be non-negative.");

        var rateDecimal = (decimal)rate / 100m;
        var vatAmount = RoundBelgian(amountExclVat * rateDecimal);

        return new VatCalculation
        {
            AmountExclVat = amountExclVat,
            VatAmount = vatAmount,
            AmountInclVat = amountExclVat + vatAmount,
            Rate = rate,
            Category = VatRateCategory.Standard,
        };
    }

    /// <summary>
    /// Calculate total from a list of line items (each with their own category).
    /// </summary>
    public static InvoiceTotals CalculateInvoice(IEnumerable<InvoiceLine> lines)
    {
        var calculations = lines
            .Select(l => (Line: l, Calc: FromExclVat(l.AmountExclVat, l.Category)))
            .ToList();

        var byRate = calculations
            .GroupBy(x => x.Calc.Rate)
            .Select(g => new VatGroup
            {
                Rate = g.Key,
                BaseAmount = g.Sum(x => x.Calc.AmountExclVat),
                VatAmount = g.Sum(x => x.Calc.VatAmount),
            })
            .OrderBy(g => g.Rate)
            .ToList();

        return new InvoiceTotals
        {
            TotalExclVat = calculations.Sum(x => x.Calc.AmountExclVat),
            TotalVat = calculations.Sum(x => x.Calc.VatAmount),
            TotalInclVat = calculations.Sum(x => x.Calc.AmountInclVat),
            VatByRate = byRate,
        };
    }

    private static decimal RoundBelgian(decimal amount) =>
        Math.Round(amount, 2, MidpointRounding.AwayFromZero);
}

/// <summary>A line item on an invoice for VAT calculation.</summary>
public record InvoiceLine
{
    public required string Description { get; init; }
    public required decimal AmountExclVat { get; init; }
    public required VatRateCategory Category { get; init; }
}

/// <summary>Aggregated VAT by rate (for invoice footer).</summary>
public record VatGroup
{
    public VatRate Rate { get; init; }
    public decimal BaseAmount { get; init; }
    public decimal VatAmount { get; init; }
}

/// <summary>Complete invoice totals.</summary>
public record InvoiceTotals
{
    public decimal TotalExclVat { get; init; }
    public decimal TotalVat { get; init; }
    public decimal TotalInclVat { get; init; }
    public IReadOnlyList<VatGroup> VatByRate { get; init; } = [];
}
