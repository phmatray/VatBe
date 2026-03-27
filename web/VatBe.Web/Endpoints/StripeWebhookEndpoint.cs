using global::Stripe;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using VatBe.Stripe;

namespace VatBe.Web.Endpoints;

/// <summary>
/// Handles incoming Stripe webhook events.
/// Register via app.MapStripeWebhook() extension.
/// </summary>
public static class StripeWebhookEndpoint
{
    public static IEndpointRouteBuilder MapStripeWebhook(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/stripe/webhook", HandleAsync)
            .WithName("StripeWebhook")
            .WithTags("Stripe")
            .DisableAntiforgery(); // Stripe uses its own signature verification

        return endpoints;
    }

    private static async Task<IResult> HandleAsync(
        HttpContext context,
        [FromServices] IOptions<StripeSettings> settings,
        [FromServices] ILogger<Program> logger)
    {
        var json = await new StreamReader(context.Request.Body).ReadToEndAsync();
        var webhookSecret = settings.Value.WebhookSecret;

        Event stripeEvent;

        try
        {
            stripeEvent = EventUtility.ConstructEvent(
                json,
                context.Request.Headers["Stripe-Signature"],
                webhookSecret,
                throwOnApiVersionMismatch: false);
        }
        catch (StripeException ex)
        {
            logger.LogWarning(ex, "Stripe webhook signature verification failed");
            return Results.BadRequest(new { error = "Webhook signature verification failed." });
        }

        logger.LogInformation("Received Stripe event: {EventType} ({EventId})", stripeEvent.Type, stripeEvent.Id);

        switch (stripeEvent.Type)
        {
            case EventTypes.CheckoutSessionCompleted:
                // TODO Sprint 2: provision API key, send welcome email
                var session = stripeEvent.Data.Object as global::Stripe.Checkout.Session;
                logger.LogInformation(
                    "Checkout completed: session={SessionId} customer={CustomerId} email={Email}",
                    session?.Id, session?.CustomerId, session?.CustomerEmail);
                break;

            case EventTypes.CustomerSubscriptionCreated:
            case EventTypes.CustomerSubscriptionUpdated:
                // TODO Sprint 2: sync subscription status to DB
                var subscription = stripeEvent.Data.Object as Subscription;
                logger.LogInformation(
                    "Subscription {EventType}: id={SubscriptionId} status={Status}",
                    stripeEvent.Type, subscription?.Id, subscription?.Status);
                break;

            case EventTypes.CustomerSubscriptionDeleted:
                // TODO Sprint 2: revoke API key, send offboarding email
                var cancelledSub = stripeEvent.Data.Object as Subscription;
                logger.LogInformation(
                    "Subscription cancelled: id={SubscriptionId}", cancelledSub?.Id);
                break;

            case EventTypes.InvoicePaymentFailed:
                // TODO Sprint 2: send dunning email, grace period logic
                var invoice = stripeEvent.Data.Object as Invoice;
                logger.LogWarning(
                    "Invoice payment failed: invoiceId={InvoiceId} customer={CustomerId}",
                    invoice?.Id, invoice?.CustomerId);
                break;

            default:
                logger.LogDebug("Unhandled Stripe event type: {EventType}", stripeEvent.Type);
                break;
        }

        return Results.Ok(new { received = true });
    }
}
