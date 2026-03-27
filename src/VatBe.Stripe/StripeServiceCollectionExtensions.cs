using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace VatBe.Stripe;

/// <summary>
/// DI registration for VatBe Stripe billing services.
/// </summary>
public static class StripeServiceCollectionExtensions
{
    /// <summary>
    /// Registers Stripe billing services.
    /// Expects a "Stripe" section in configuration with <see cref="StripeSettings"/>.
    /// </summary>
    public static IServiceCollection AddVatBeStripe(
        this IServiceCollection services,
        Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        services.Configure<StripeSettings>(configuration.GetSection(StripeSettings.SectionName));
        services.TryAddScoped<ICheckoutService, CheckoutService>();
        return services;
    }
}
