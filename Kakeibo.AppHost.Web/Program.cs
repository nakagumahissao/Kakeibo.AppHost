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

var builder = WebApplication.CreateBuilder(args);

// HttpContext
builder.Services.AddHttpContextAccessor();

// Circuit Handler
builder.Services.AddSingleton<CircuitHandler, CultureCircuitHandler>();

// ---------------- Kestrel ----------------
var certPath = builder.Configuration["Kestrel:HttpsCertificate:Path"];
var certPassword = builder.Configuration["Kestrel:HttpsCertificate:Password"];

builder.WebHost.ConfigureKestrel(options =>
{
    options.Listen(IPAddress.Parse("100.64.1.29"), 446, listenOptions =>
    {
        listenOptions.UseHttps(certPath!, certPassword);
    });
});

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

builder.Services.AddLocalization(options =>
{
    options.ResourcesPath = "Resources";
});

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("pt-BR");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;

    // --- TEMPORARY FIX: Use ONLY the AcceptLanguageHeader provider ---
    options.RequestCultureProviders = new IRequestCultureProvider[]
    {
        // This MUST be the only provider for now to ensure it runs without interference.
        new AcceptLanguageHeaderRequestCultureProvider()
        // DO NOT INCLUDE QueryString or Cookie for this test!
    };

    //options.RequestCultureProviders = new IRequestCultureProvider[]
    //{
    //    new QueryStringRequestCultureProvider(),
    //    new CookieRequestCultureProvider(),
    //    new AcceptLanguageHeaderRequestCultureProvider()
    //};
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
app.MapPost("/blazor-login", async (
    LoginModel loginModel,
    IHttpClientFactory clientFactory,
    TokenService tokenService,
    IStringLocalizer<SharedResources> L
) =>
{
    Log.Information("Blazor login attempt for user {Email}.", loginModel.Email);

    var client = clientFactory.CreateClient("apis");
    var response = await client.PostAsJsonAsync("auth/login", loginModel);

    if (!response.IsSuccessStatusCode)
    {
        return Results.Json(
            new { error = L["CredenciaisInválidas"] },
            statusCode: StatusCodes.Status401Unauthorized
        );
    }

    var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
    if (loginResponse == null)
    {
        return Results.Problem(L["FormatoInválidoRespostaApi"]);
    }

    tokenService.SetToken(loginResponse.Token);
    return Results.Ok(new { success = true });

}).AllowAnonymous();

app.Run();
