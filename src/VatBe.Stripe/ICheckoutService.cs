namespace VatBe.Stripe;

/// <summary>
/// Creates Stripe Checkout sessions for VatBe subscription plans.
/// </summary>
public interface ICheckoutService
{
    /// <summary>
    /// Creates a new Stripe Checkout session for the given price ID.
    /// Returns the URL to redirect the user to.
    /// </summary>
    /// <param name="priceId">Stripe Price ID (price_...)</param>
    /// <param name="customerEmail">Pre-fill email in Stripe Checkout (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<string> CreateSessionUrlAsync(
        string priceId,
        string? customerEmail = null,
        CancellationToken cancellationToken = default);
}
