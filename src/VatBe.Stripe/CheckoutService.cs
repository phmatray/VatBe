using global::Stripe;
using global::Stripe.Checkout;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace VatBe.Stripe;

/// <inheritdoc />
public sealed class CheckoutService : ICheckoutService
{
    private readonly StripeSettings _settings;
    private readonly ILogger<CheckoutService> _logger;

    public CheckoutService(IOptions<StripeSettings> settings, ILogger<CheckoutService> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        StripeConfiguration.ApiKey = _settings.SecretKey;
    }

    /// <inheritdoc />
    public async Task<string> CreateSessionUrlAsync(
        string priceId,
        string? customerEmail = null,
        CancellationToken cancellationToken = default)
    {
        var options = new SessionCreateOptions
        {
            Mode = "subscription",
            LineItems =
            [
                new SessionLineItemOptions
                {
                    Price = priceId,
                    Quantity = 1,
                }
            ],
            SuccessUrl = $"{_settings.BaseUrl}/checkout/success?session_id={{CHECKOUT_SESSION_ID}}",
            CancelUrl  = $"{_settings.BaseUrl}/pricing",
            CustomerEmail = customerEmail,
            // Collect billing address for Belgian VAT invoicing
            BillingAddressCollection = "required",
            // Allow promo codes
            AllowPromotionCodes = true,
            // Automatic tax collection (requires Stripe Tax to be enabled on your account)
            AutomaticTax = new SessionAutomaticTaxOptions { Enabled = false },
        };

        var service = new SessionService();
        Session session;

        try
        {
            session = await service.CreateAsync(options, cancellationToken: cancellationToken);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe checkout session creation failed for priceId={PriceId}", priceId);
            throw;
        }

        _logger.LogInformation("Stripe checkout session created: {SessionId} for priceId={PriceId}", session.Id, priceId);
        return session.Url;
    }
}
