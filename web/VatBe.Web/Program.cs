using TheAppManager.Startup;
using VatBe.Web.Modules;

AppManager.Start(args, modules =>
{
    modules
        .Add<DataProtectionModule>()
        .Add<BlazorModule>()
        .Add<VatBeModule>()
        .Add<HealthChecksModule>();
});
