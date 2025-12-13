using Kakeibo.AppHost.Web.Components;
using Kakeibo.AppHost.Web.Models;
using Kakeibo.AppHost.Web.Services;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Localization;
using Serilog;
using StackExchange.Redis;
using System.Globalization;
using System.Net;
using Kakeibo.AppHost.Web;
using Kakeibo.AppHost.Web.Resources;
using Microsoft.AspNetCore.Http.HttpResults;

var builder = WebApplication.CreateBuilder(args);

// ... (Kestrel, Culture, Windows Service, Serilog, Redis, Data Protection, JWT/HTTP, Blazor Setup remains unchanged) ...

// ---------------- CULTURE (FIXED) ----------------
var supportedCultures = new[]
{
    new CultureInfo("pt"),
    new CultureInfo("pt-BR"),
    new CultureInfo("ja"),
    new CultureInfo("ja-JP"),
    new CultureInfo("en"),
    new CultureInfo("en-US"),
    new CultureInfo("de"),
    new CultureInfo("es"),
    new CultureInfo("fr"),
    new CultureInfo("zh-CN")
};

// --- FINAL LOCALIZATION FIX (Simplified to avoid Type Conflict) ---
// We remove the explicit factory/localizer registrations that caused the compiler conflict.
builder.Services.AddLocalization(options =>
{
    // We rely on the implicit naming convention again, but the fix must be done 
    // by ensuring the RootNamespace is correct in the .csproj file.
    options.ResourcesPath = "Resources";
});

// ... (Configuration of RequestLocalizationOptions remains unchanged) ...
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("pt-BR");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;

    options.RequestCultureProviders = new IRequestCultureProvider[]
    {
        new AcceptLanguageHeaderRequestCultureProvider()
    };
});

// ---------------- Windows Service ----------------
builder.Host.UseWindowsService();

// ---------------- SERILOG ----------------
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithMachineName()
    .Enrich.WithProcessId()
    .Enrich.WithThreadId()
    .CreateLogger();

Log.Information("Starting Kakeibo Web Site Service...");
builder.Host.UseSerilog();

// ---------------- Redis ----------------
var redisConnectionString = builder.Configuration.GetConnectionString("redis");
if (string.IsNullOrEmpty(redisConnectionString))
{
    Log.Fatal("Redis connection string not found.");
    throw new InvalidOperationException("Redis connection string not found.");
}

var redis = ConnectionMultiplexer.Connect(redisConnectionString);

// ---------------- Data Protection ----------------
builder.Services.AddDataProtection()
    .SetApplicationName("KakeiboWebFrontend")
    .PersistKeysToStackExchangeRedis(redis);

// ---------------- JWT / HTTP ----------------
builder.Services.AddSingleton<TokenService>();
builder.Services.AddTransient<JwtAuthorizationHandler>();

builder.Services.AddHttpClient("apis", c =>
{
    c.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]!);
})
.AddHttpMessageHandler<JwtAuthorizationHandler>();

builder.Services.AddHttpClient("local", client =>
{
    client.BaseAddress = new Uri("https://100.64.1.29:446/");
});

// ---------------- Blazor ----------------
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorization();
builder.Services.AddOutputCache();

var app = builder.Build();

// Localization
var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("pt-BR"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
};

localizationOptions.RequestCultureProviders.Insert(0,
    new AcceptLanguageHeaderRequestCultureProvider());

app.UseRequestLocalization(localizationOptions);

// ---------------- Pipeline ----------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseForwardedHeaders();
app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();
app.UseAuthorization();

// ---------------- Blazor Routing ----------------
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// ---------------- LOGIN ENDPOINT ----------------
// We use the static local function, passing only HttpContext to manually resolve services.
app.MapPost("/blazor-login", ProcessLoginAsync)
    .AllowAnonymous();


// ---------------- LOCAL STATIC FUNCTION FOR LOGIN ----------------
// We keep the service locator pattern as it is the only way to avoid DI conflicts in this project.
static async Task<IResult> ProcessLoginAsync(HttpContext context, LoginModel loginModel)
{
    // Manually resolve the necessary services from the request's scope
    var clientFactory = context.RequestServices.GetRequiredService<IHttpClientFactory>();
    var tokenService = context.RequestServices.GetRequiredService<TokenService>();
    var localizerFactory = context.RequestServices.GetRequiredService<IStringLocalizerFactory>();

    // Manually create the typed localizer instance using the factory
    var L = localizerFactory.Create(typeof(SharedResources)); // This relies on the ResourcesPath setting

    // The rest of the logic remains the same
    Log.Information("Blazor login attempt for user {Email}.", loginModel.Email);

    var client = clientFactory.CreateClient("apis");
    var response = await client.PostAsJsonAsync("auth/login", loginModel);

    if (!response.IsSuccessStatusCode)
    {
        return Results.Json(
            new { error = L["CredenciaisInválidas"].Value },
            statusCode: StatusCodes.Status401Unauthorized
        );
    }

    var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
    if (loginResponse == null)
    {
        return Results.Problem(L["FormatoInválidoRespostaApi"].Value);
    }

    tokenService.SetToken(loginResponse.Token);
    return Results.Ok(new { success = true });
}

app.Run();