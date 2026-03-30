using Microsoft.Extensions.Logging;

namespace VatBe.Stripe;

/// <inheritdoc />
/// <remarks>
/// This implementation logs all lifecycle events and provides the scaffolding
/// for Sprint 2 integrations: API key provisioning, email notifications, and DB sync.
/// Each method contains a clearly marked TODO for the concrete integration work.
/// </remarks>
public sealed class SubscriptionService : ISubscriptionService
{
    private readonly ILogger<SubscriptionService> _logger;

    public SubscriptionService(ILogger<SubscriptionService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public Task ProvisionSubscriberAsync(
        string customerId,
        string customerEmail,
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        // TODO Sprint 2a: Generate and persist an API key for the customer
        //   1. Generate a cryptographically secure API key (e.g. Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)))
        //   2. Hash the key (SHA-256) and store <hash, customerId, email, createdAt> in DB
        //   3. Return plain-text key to the customer (only once — never stored in plain text)

        // TODO Sprint 2b: Send welcome email with the API key and onboarding instructions
        //   Use SendGrid / Resend / SMTP — template: "welcome-subscriber"

        _logger.LogInformation(
            "Subscriber provisioned: customerId={CustomerId} email={Email} sessionId={SessionId}",
            customerId, customerEmail, sessionId);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task SyncSubscriptionAsync(
        string subscriptionId,
        string customerId,
        string status,
        string? planNickname,
        CancellationToken cancellationToken = default)
    {
        // TODO Sprint 2a: Upsert subscription record in DB
        //   Table: Subscriptions (subscriptionId PK, customerId, status, planNickname, updatedAt)
        //   Consider EF Core or Dapper for persistence

        // TODO Sprint 2b: Adjust API key permissions based on status
        //   - "active" | "trialing" → key enabled
        //   - "past_due"            → key enabled with a grace-period warning flag
        //   - "unpaid" | "canceled" → key disabled (handled by RevokeSubscriberAsync for "canceled")

        _logger.LogInformation(
            "Subscription synced: subscriptionId={SubscriptionId} customerId={CustomerId} status={Status} plan={Plan}",
            subscriptionId, customerId, status, planNickname ?? "unknown");

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task RevokeSubscriberAsync(
        string subscriptionId,
        string customerId,
        CancellationToken cancellationToken = default)
    {
        // TODO Sprint 2a: Disable the API key for this customer
        //   Update ApiKeys table: set IsActive = false WHERE customerId = @customerId

        // TODO Sprint 2b: Send offboarding email
        //   Template: "subscription-cancelled" — include data export link

        // TODO Sprint 2c: Optionally trigger a 30-day data retention cleanup job

        _logger.LogInformation(
            "Subscriber revoked: subscriptionId={SubscriptionId} customerId={CustomerId}",
            subscriptionId, customerId);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task HandlePaymentFailureAsync(
        string invoiceId,
        string customerId,
        int attemptCount,
        CancellationToken cancellationToken = default)
    {
        // TODO Sprint 2a: Mark subscription as past_due in DB

        // TODO Sprint 2b: Dunning email sequence based on attempt count
        //   Attempt 1 → friendly reminder
        //   Attempt 2 → urgent warning — API access will be suspended
        //   Attempt 3 → API key suspended, final notice before cancellation

        // TODO Sprint 2c: After max retries, call Stripe API to cancel subscription
        //   (Stripe can also handle this via Smart Retries — configure in dashboard)

        var severity = attemptCount switch
        {
            1 => LogLevel.Information,
            2 => LogLevel.Warning,
            _ => LogLevel.Error
        };

        _logger.Log(severity,
            "Payment failure: invoiceId={InvoiceId} customerId={CustomerId} attempt={Attempt}",
            invoiceId, customerId, attemptCount);

        return Task.CompletedTask;
    }
}
