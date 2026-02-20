using Microsoft.AspNetCore.DataProtection;
using VatBe;
using VatBe.Web.Components;

var builder = WebApplication.CreateBuilder(args);

// ─────────────────────────────────────────────────────────────────────────────
// Data Protection — shared keys for multi-pod Blazor Server deployment
// ─────────────────────────────────────────────────────────────────────────────
var dataProtectionPath = builder.Configuration.GetValue<string>("DataProtection:KeyPath");
if (!string.IsNullOrWhiteSpace(dataProtectionPath))
{
    var keyDir = new DirectoryInfo(dataProtectionPath);
    if (!keyDir.Exists) keyDir.Create();
    
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(keyDir)
        .SetApplicationName("VatBe");
}

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// VatBe: IViesClient + typed HttpClient with Polly resilience
builder.Services.AddVatBe();

builder.Services.AddHealthChecks();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapHealthChecks("/health");

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
