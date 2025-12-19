using Kakeibo.AppHost.Web.Components;
using Kakeibo.AppHost.Web.Localization;
using Kakeibo.AppHost.Web.Models;
using Kakeibo.AppHost.Web.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Localization;
using Serilog;
using StackExchange.Redis;
using System.Globalization;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// ---------------- Windows Service ----------------
builder.Host.UseWindowsService();

// ---------------- Kestrel HTTPS ----------------
var certPath = builder.Configuration["Kestrel:HttpsCertificate:Path"];
var certPassword = builder.Configuration["Kestrel:HttpsCertificate:Password"];

builder.WebHost.ConfigureKestrel(options =>
{
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

// ---------------- Redis Data Protection ----------------
var redisConnectionString = builder.Configuration.GetConnectionString("redis");
if (string.IsNullOrEmpty(redisConnectionString))
{
    Log.Fatal("Redis connection string not found.");
    throw new InvalidOperationException("Redis connection string not found.");
}

var redis = ConnectionMultiplexer.Connect(redisConnectionString);
builder.Services.AddDataProtection()
    .SetApplicationName("KakeiboWebFrontend")
    .PersistKeysToStackExchangeRedis(redis);

// ---------------- HTTP / JWT ----------------
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

// ---------------- CULTURE / LOCALIZATION ----------------
var supportedCultures = new[]
{
    "en", "en-US", "pt", "pt-BR",
    "ja", "ja-JP", "de", "es", "fr", "zh-CN"
};

builder.Services.AddLocalization(options =>
{
    options.ResourcesPath = "";
});

// ---------------- Blazor Server ----------------
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorization();
builder.Services.AddOutputCache();

var app = builder.Build();

// ---------------- Localization Middleware ----------------
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture("pt-BR")
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

localizationOptions.RequestCultureProviders.Insert(0,
    new AcceptLanguageHeaderRequestCultureProvider());

app.UseRequestLocalization(localizationOptions);

// ---------------- Middleware Pipeline ----------------
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
app.MapPost("/blazor-login", ProcessLoginAsync)
    .AllowAnonymous();

// ---------------- LOCAL STATIC FUNCTION FOR LOGIN ----------------
static async Task<IResult> ProcessLoginAsync(HttpContext context, LoginModel loginModel)
{
    // Resolve services
    var clientFactory = context.RequestServices.GetRequiredService<IHttpClientFactory>();
    var tokenService = context.RequestServices.GetRequiredService<TokenService>();
    var localizerFactory = context.RequestServices.GetRequiredService<IStringLocalizerFactory>();
    var L = localizerFactory.Create(typeof(SharedResources));

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
