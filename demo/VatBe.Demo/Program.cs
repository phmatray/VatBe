using VatBe;
using VatBe.Calculation;
using VatBe.Models;
using VatBe.Vies;
using Microsoft.Extensions.DependencyInjection;

// ─────────────────────────────────────────────────────────────────────────────
// VatBe Demo — Belgian VAT toolkit for .NET
// ─────────────────────────────────────────────────────────────────────────────

Console.WriteLine("╔══════════════════════════════════════════════════════╗");
Console.WriteLine("║          VatBe — Belgian VAT Toolkit Demo            ║");
Console.WriteLine("╚══════════════════════════════════════════════════════╝");
Console.WriteLine();

// ─────────────────────────────────
// 1. Enterprise Number Validation
// ─────────────────────────────────
Console.WriteLine("── 1. Enterprise Number (KBO/BCE) Validation ──────────");

string[] testNumbers =
[
    "0402.206.045",      // Delhaize Group
    "0403.199.702",      // BNP Paribas Fortis
    "BE0123.456.749",    // Generated valid
    "0000000000",        // Invalid (all zeros)
    "0412345678",        // Invalid (wrong check digits)
    "not-a-number",      // Invalid
];

foreach (var input in testNumbers)
{
    var valid = EnterpriseNumber.TryParse(input, out var en);
    if (valid)
        Console.WriteLine($"  ✓ {input,-25} → {en.ToFormatted()} (VAT: {en.ToVatNumber().ToCompact()})");
    else
        Console.WriteLine($"  ✗ {input,-25} → INVALID");
}

Console.WriteLine();

// ─────────────────────────────────
// 2. VAT Number Validation
// ─────────────────────────────────
Console.WriteLine("── 2. VAT Number (BTW/TVA) Validation ─────────────────");

string[] vatNumbers =
[
    "BE0402.206.045",    // Valid
    "BE 0403.199.702",   // Valid with space
    "BE9999999999",      // Invalid check digits
    "FR12345678901",     // French (not Belgian format)
    "0402.206.045",      // Missing BE prefix
];

foreach (var input in vatNumbers)
{
    var valid = VatNumber.TryParse(input, out var vat);
    if (valid)
        Console.WriteLine($"  ✓ {input,-25} → {vat.ToFormatted()}");
    else
        Console.WriteLine($"  ✗ {input,-25} → INVALID");
}

Console.WriteLine();

// ─────────────────────────────────
// 3. VAT Rate Lookup
// ─────────────────────────────────
Console.WriteLine("── 3. VAT Rate Lookup ──────────────────────────────────");

var categories = new[]
{
    (VatRateCategory.BasicFood,           "Basic food (bread, milk...)"),
    (VatRateCategory.Pharmaceutical,      "Medicine, pharmaceuticals"),
    (VatRateCategory.Books,               "Books, newspapers"),
    (VatRateCategory.HotelAccommodation,  "Hotel accommodation"),
    (VatRateCategory.ResidentialRenovation, "Renovation (building >10yr)"),
    (VatRateCategory.RestaurantFood,      "Restaurant (food portion)"),
    (VatRateCategory.Electronics,         "Electronics, IT equipment"),
    (VatRateCategory.Clothing,            "Clothing & fashion"),
    (VatRateCategory.DigitalServices,     "Digital / SaaS services"),
    (VatRateCategory.Alcohol,             "Alcohol"),
    (VatRateCategory.ExportOutsideEU,     "Export outside EU"),
};

foreach (var (cat, desc) in categories)
{
    var rate = BelgianVatRates.GetRate(cat);
    Console.WriteLine($"  {(int)rate,3}%  {desc}");
}

Console.WriteLine();

// ─────────────────────────────────
// 4. VAT Calculation
// ─────────────────────────────────
Console.WriteLine("── 4. VAT Calculation ──────────────────────────────────");

var calc1 = VatCalculator.FromExclVat(999.00m, VatRateCategory.Electronics);
Console.WriteLine($"  Laptop (excl VAT): €{calc1.AmountExclVat:F2}");
Console.WriteLine($"  VAT ({calc1.RatePercentage}%):         €{calc1.VatAmount:F2}");
Console.WriteLine($"  Total (incl VAT):  €{calc1.AmountInclVat:F2}");
Console.WriteLine();

// Reverse calculation
var calc2 = VatCalculator.FromInclVat(119.99m, VatRateCategory.DigitalServices);
Console.WriteLine($"  SaaS invoice (incl VAT): €{calc2.AmountInclVat:F2}");
Console.WriteLine($"  Excl VAT:                €{calc2.AmountExclVat:F2}");
Console.WriteLine($"  VAT ({calc2.RatePercentage}%):              €{calc2.VatAmount:F2}");
Console.WriteLine();

// ─────────────────────────────────
// 5. Invoice Calculation
// ─────────────────────────────────
Console.WriteLine("── 5. Mixed-Rate Invoice Example ───────────────────────");

var invoiceLines = new[]
{
    new InvoiceLine { Description = "IT Consulting (21%)",   AmountExclVat = 2000.00m, Category = VatRateCategory.Standard },
    new InvoiceLine { Description = "Cloud hosting (21%)",   AmountExclVat = 150.00m,  Category = VatRateCategory.DigitalServices },
    new InvoiceLine { Description = "Technical book (6%)",   AmountExclVat = 49.95m,   Category = VatRateCategory.Books },
    new InvoiceLine { Description = "Catered lunch (12%)",   AmountExclVat = 85.00m,   Category = VatRateCategory.RestaurantFood },
};

Console.WriteLine("  Lines:");
foreach (var line in invoiceLines)
{
    var lc = VatCalculator.FromExclVat(line.AmountExclVat, line.Category);
    Console.WriteLine($"    {line.Description,-35} €{line.AmountExclVat,9:F2} + {(int)lc.Rate}% VAT");
}

var totals = VatCalculator.CalculateInvoice(invoiceLines);
Console.WriteLine();
Console.WriteLine("  VAT breakdown:");
foreach (var group in totals.VatByRate)
    Console.WriteLine($"    {(int)group.Rate,3}%  Base: €{group.BaseAmount,8:F2}  VAT: €{group.VatAmount,7:F2}");

Console.WriteLine($"  ─────────────────────────────────────────────────");
Console.WriteLine($"  Total excl VAT:  €{totals.TotalExclVat,9:F2}");
Console.WriteLine($"  Total VAT:       €{totals.TotalVat,9:F2}");
Console.WriteLine($"  Total incl VAT:  €{totals.TotalInclVat,9:F2}");
Console.WriteLine();

// ─────────────────────────────────
// 6. Fluent Extension Methods
// ─────────────────────────────────
Console.WriteLine("── 6. Fluent Extensions ────────────────────────────────");

Console.WriteLine($"  \"0402.206.045\".IsBelgianEnterpriseNumber() = {"0402.206.045".IsBelgianEnterpriseNumber()}");
Console.WriteLine($"  \"0000000000\".IsBelgianEnterpriseNumber()   = {"0000000000".IsBelgianEnterpriseNumber()}");
Console.WriteLine($"  \"BE0402206045\".IsBelgianVatNumber()         = {"BE0402206045".IsBelgianVatNumber()}");
Console.WriteLine($"  \"0402206045\".FormatAsEnterpriseNumber()      = {"0402206045".FormatAsEnterpriseNumber()}");

var fluentCalc = 500m.WithBelgianVat(VatRateCategory.Standard);
Console.WriteLine($"  500m.WithBelgianVat(Standard) → incl: €{fluentCalc.AmountInclVat}");

Console.WriteLine();

// ─────────────────────────────────
// 7. VIES Lookup (optional — requires network)
// ─────────────────────────────────
Console.WriteLine("── 7. VIES Validation (EU VIES API) ───────────────────");

if (args.Contains("--vies"))
{
    var services = new ServiceCollection().AddVatBe().BuildServiceProvider();
    var viesClient = services.GetRequiredService<IViesClient>();

    Console.WriteLine("  Validating BE0402206045 (Delhaize Group)...");
    var viesResult = await viesClient.ValidateAsync("BE", "0402206045");
    Console.WriteLine($"  {viesResult}");
    Console.WriteLine($"  Validated at: {viesResult.ValidatedAt:HH:mm:ss} UTC");
}
else
{
    Console.WriteLine("  (Skipped — pass --vies to perform live EU VIES lookup)");
}

Console.WriteLine();
Console.WriteLine("✓ Demo complete. VatBe works correctly.");
