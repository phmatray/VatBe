using TheAppManager.Modules;

namespace VatBe.Web.Modules;

public class VatBeModule : IAppModule
{
    public void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.AddVatBe();
    }
}
