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
        [FromServices] ISubscriptionService subscriptionService,
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

        try
        {
            switch (stripeEvent.Type)
            {
                case EventTypes.CheckoutSessionCompleted:
                    var session = stripeEvent.Data.Object as global::Stripe.Checkout.Session;
                    if (session is not null)
                    {
                        await subscriptionService.ProvisionSubscriberAsync(
                            session.CustomerId ?? string.Empty,
                            session.CustomerEmail ?? string.Empty,
                            session.Id);
                    }
                    break;

                case EventTypes.CustomerSubscriptionCreated:
                case EventTypes.CustomerSubscriptionUpdated:
                    var subscription = stripeEvent.Data.Object as Subscription;
                    if (subscription is not null)
                    {
                        var planNickname = subscription.Items?.Data?.FirstOrDefault()?.Plan?.Nickname;
                        await subscriptionService.SyncSubscriptionAsync(
                            subscription.Id,
                            subscription.CustomerId,
                            subscription.Status,
                            planNickname);
                    }
                    break;

                case EventTypes.CustomerSubscriptionDeleted:
                    var cancelledSub = stripeEvent.Data.Object as Subscription;
                    if (cancelledSub is not null)
                    {
                        await subscriptionService.RevokeSubscriberAsync(
                            cancelledSub.Id,
                            cancelledSub.CustomerId);
                    }
                    break;

                case EventTypes.InvoicePaymentFailed:
                    var invoice = stripeEvent.Data.Object as Invoice;
                    if (invoice is not null)
                    {
                        await subscriptionService.HandlePaymentFailureAsync(
                            invoice.Id,
                            invoice.CustomerId,
                            (int)(invoice.AttemptCount ?? 1));
                    }
                    break;

                default:
                    logger.LogDebug("Unhandled Stripe event type: {EventType}", stripeEvent.Type);
                    break;
            }
        }
        catch (Exception ex)
        {
            // Log but return 200 — Stripe will retry on non-2xx, so we should only return errors
            // for signature failures (above). For processing errors, log and investigate separately.
            logger.LogError(ex, "Error processing Stripe event {EventType} ({EventId})",
                stripeEvent.Type, stripeEvent.Id);
        }

        return Results.Ok(new { received = true });
    }
}
