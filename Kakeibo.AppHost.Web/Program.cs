using Kakeibo.AppHost.Web.Components;
using Kakeibo.AppHost.Web.Models;
using Kakeibo.AppHost.Web;
using Kakeibo.AppHost.Web.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Localization;
using Serilog;
using StackExchange.Redis;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// ---------------- Windows Service ----------------
builder.Host.UseWindowsService();

var certPath = builder.Configuration["Kestrel:HttpsCertificate:Path"];
var certPassword = builder.Configuration["Kestrel:HttpsCertificate:Password"];

builder.WebHost.ConfigureKestrel(options =>
{
    // HTTPS on port 446
    options.Listen(IPAddress.Parse("100.64.1.29"), 446, listenOptions =>
    {
        listenOptions.UseHttps(certPath!, certPassword);
    });
});

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

// ---------------- CULTURE ----------------
var supportedCultures = new[]
{
    "en",
    "en-US",
    "pt",
    "pt-BR",
    "ja",
    "ja-JP",
    "de",
    "es",
    "fr",
    "zh-CN"
};

builder.Services.AddLocalization(options =>
{
    options.ResourcesPath = "Localization";
});

// ---------------- Blazor ----------------
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorization();
builder.Services.AddOutputCache();

var app = builder.Build();

// Localization
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(supportedCultures[2])
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

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

    var L = localizerFactory.Create(typeof(SharedResources));

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