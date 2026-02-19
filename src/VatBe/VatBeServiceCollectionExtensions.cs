using Microsoft.Extensions.DependencyInjection;
using VatBe.Vies;

namespace VatBe;

/// <summary>
/// DI registration for VatBe services.
/// </summary>
public static class VatBeServiceCollectionExtensions
{
    /// <summary>
    /// Register VatBe services. Adds <see cref="IViesClient"/> with a typed <see cref="HttpClient"/>.
    /// </summary>
    public static IServiceCollection AddVatBe(this IServiceCollection services)
    {
        services.AddHttpClient<IViesClient, ViesClient>(client =>
        {
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        return services;
    }
}
