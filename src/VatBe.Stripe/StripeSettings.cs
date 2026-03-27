namespace VatBe.Stripe;

/// <summary>
/// Stripe configuration — bind from "Stripe" section in appsettings.
/// </summary>
public sealed class StripeSettings
{
    public const string SectionName = "Stripe";

    /// <summary>Stripe publishable key (pk_live_... or pk_test_...)</summary>
    public string PublishableKey { get; set; } = string.Empty;

    /// <summary>Stripe secret key (sk_live_... or sk_test_...) — server-side only, never exposed to client</summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>Webhook signing secret (whsec_...) — used to verify incoming Stripe events</summary>
    public string WebhookSecret { get; set; } = string.Empty;

    /// <summary>Base URL of the web app, used to build success/cancel redirect URLs</summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>Pre-configured Stripe Price IDs for each plan tier</summary>
    public StripePrices Prices { get; set; } = new();
}

/// <summary>
/// Stripe Price IDs for each subscription tier.
/// These are the IDs from your Stripe dashboard (price_...).
/// </summary>
public sealed class StripePrices
{
    /// <summary>Monthly price ID for the Pro tier</summary>
    public string ProMonthly { get; set; } = string.Empty;

    /// <summary>Annual price ID for the Pro tier</summary>
    public string ProAnnual { get; set; } = string.Empty;

    /// <summary>Monthly price ID for the Enterprise tier</summary>
    public string EnterpriseMonthly { get; set; } = string.Empty;

    /// <summary>Annual price ID for the Enterprise tier</summary>
    public string EnterpriseAnnual { get; set; } = string.Empty;
}
