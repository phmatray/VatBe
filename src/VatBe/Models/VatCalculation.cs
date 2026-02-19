namespace VatBe.Models;

/// <summary>Result of a VAT calculation.</summary>
public record VatCalculation
{
    public decimal AmountExclVat { get; init; }
    public decimal VatAmount { get; init; }
    public decimal AmountInclVat { get; init; }
    public VatRate Rate { get; init; }
    public decimal RatePercentage => (decimal)Rate;
    public VatRateCategory Category { get; init; }

    public override string ToString() =>
        $"Excl: {AmountExclVat:F2} EUR | VAT {RatePercentage}%: {VatAmount:F2} EUR | Incl: {AmountInclVat:F2} EUR";
}
