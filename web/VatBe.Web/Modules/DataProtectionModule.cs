using Microsoft.AspNetCore.DataProtection;
using TheAppManager.Modules;

namespace VatBe.Web.Modules;

public class DataProtectionModule : IAppModule
{
    public void ConfigureServices(WebApplicationBuilder builder)
    {
        var dataProtectionPath = builder.Configuration.GetValue<string>("DataProtection:KeyPath");
        if (!string.IsNullOrWhiteSpace(dataProtectionPath))
        {
            var keyDir = new DirectoryInfo(dataProtectionPath);
            if (!keyDir.Exists) keyDir.Create();

            builder.Services.AddDataProtection()
                .PersistKeysToFileSystem(keyDir)
                .SetApplicationName("VatBe");
        }
    }
}
