using TheAppManager.Modules;
using VatBe.Stripe;
using VatBe.Web.Endpoints;

namespace VatBe.Web.Modules;

public class StripeModule : IAppModule
{
    public void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.AddVatBeStripe(builder.Configuration);
    }

    public void ConfigureEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapStripeWebhook();
    }
}
