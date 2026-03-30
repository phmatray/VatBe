namespace VatBe.Stripe;

/// <summary>
/// Manages subscriber lifecycle events driven by Stripe webhook events.
/// Handles provisioning, updates, cancellations, and dunning for VatBe API subscriptions.
/// </summary>
public interface ISubscriptionService
{
    /// <summary>
    /// Called when a Stripe Checkout session completes successfully.
    /// Provisions an API key for the customer and sends a welcome email.
    /// </summary>
    /// <param name="customerId">Stripe customer ID (cus_...)</param>
    /// <param name="customerEmail">Customer email address</param>
    /// <param name="sessionId">Stripe Checkout session ID (cs_...)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task ProvisionSubscriberAsync(
        string customerId,
        string customerEmail,
        string sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Called when a subscription is created or updated.
    /// Syncs subscription status (active, past_due, trialing, etc.) to the local DB.
    /// </summary>
    /// <param name="subscriptionId">Stripe subscription ID (sub_...)</param>
    /// <param name="customerId">Stripe customer ID</param>
    /// <param name="status">Stripe subscription status string</param>
    /// <param name="planNickname">Human-readable plan name (Pro Monthly, etc.)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SyncSubscriptionAsync(
        string subscriptionId,
        string customerId,
        string status,
        string? planNickname,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Called when a subscription is cancelled / deleted.
    /// Revokes the customer's API key and triggers offboarding communications.
    /// </summary>
    /// <param name="subscriptionId">Stripe subscription ID</param>
    /// <param name="customerId">Stripe customer ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RevokeSubscriberAsync(
        string subscriptionId,
        string customerId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Called when a Stripe invoice payment fails.
    /// Marks the subscription as past_due and initiates dunning (grace period) logic.
    /// </summary>
    /// <param name="invoiceId">Stripe invoice ID (in_...)</param>
    /// <param name="customerId">Stripe customer ID</param>
    /// <param name="attemptCount">Number of payment attempts so far</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task HandlePaymentFailureAsync(
        string invoiceId,
        string customerId,
        int attemptCount,
        CancellationToken cancellationToken = default);
}
