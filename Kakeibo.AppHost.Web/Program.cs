using Kakeibo.AppHost.Web.Components;
using Kakeibo.AppHost.Web.Models;
using Kakeibo.AppHost.Web.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Localization;
using Serilog;
using StackExchange.Redis;
using System.Globalization;
using System.Net;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// HttpContext para pegar usuário logado
builder.Services.AddHttpContextAccessor();

//if (!builder.Environment.IsDevelopment())
//{
    // Kestrel configuration
    var certPath = builder.Configuration["Kestrel:HttpsCertificate:Path"];
    var certPassword = builder.Configuration["Kestrel:HttpsCertificate:Password"];

    builder.WebHost.ConfigureKestrel(options =>
    {
        // Listen on all IPs on HTTP port 9001
        //options.Listen(IPAddress.Any, 9001);

        // Listen specifically on 100.64.1.29 for HTTPS port 446
        options.Listen(IPAddress.Parse("100.64.1.29"), 446, listenOptions =>
        {
            listenOptions.UseHttps(certPath!, certPassword);
        });
    });
//}

// ---------------- Culture ----------------
var supportedCultures = new[]
{
    new CultureInfo("de"),
    new CultureInfo("en"),
    new CultureInfo("es"),
    new CultureInfo("fr"),
    new CultureInfo("zh-CN"),
    new CultureInfo("ja")
};

// Enable Windows Service support
builder.Host.UseWindowsService();

// ---------------- SERILOG (Corrected) ----------------
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

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("en"); // Fallback
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;

    // Accept-Language from browser/OS first
    options.RequestCultureProviders = new IRequestCultureProvider[]
    {
        new AcceptLanguageHeaderRequestCultureProvider()
    };
});

// ---------------- Redis ----------------
var redisConnectionString = builder.Configuration.GetConnectionString("redis");
if (string.IsNullOrEmpty(redisConnectionString))
{
    string strError = "Redis connection string not found.";
    Log.Fatal(strError);
    throw new InvalidOperationException(strError);
}

var redis = ConnectionMultiplexer.Connect(redisConnectionString);

// ---------------- Data Protection ----------------
builder.Services.AddDataProtection()
    .SetApplicationName("KakeiboWebFrontend")
    .PersistKeysToStackExchangeRedis(redis);

// ---------------- JWT/Cookie Hybrid Setup ----------------
builder.Services.AddSingleton<TokenService>();
builder.Services.AddTransient<JwtAuthorizationHandler>();

builder.Services.AddHttpClient("apis", c =>
{
    c.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]!);
})
.AddHttpMessageHandler<JwtAuthorizationHandler>();

// Local Blazor Server endpoints
builder.Services.AddHttpClient("local", client =>
{
    client.BaseAddress = new Uri("https://100.64.1.29:446/");
});

// ---------------- Blazor Server ----------------
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorization();

builder.Services.AddOutputCache();

var app = builder.Build();

// ---------------- Request pipeline ----------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

// app.UseCors("AllowAPI");
app.UseForwardedHeaders();
app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();
app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// =======================================================
// LOGIN (Blazor → API → Store JWT & Set Cookie)
// =======================================================
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
        Log.Information("Blazor login failed for user {Email}. Status Code: {StatusCode}", loginModel.Email, response.StatusCode);
        return Results.Json(new { error = L["Credenciais Inválidas"] }, statusCode: StatusCodes.Status401Unauthorized);
    }

    var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
    if (loginResponse == null)
    {
        Log.Error("Blazor login response deserialization failed for user {Email}.", loginModel.Email);
        return Results.Problem(L["Formato Inválido de Resposta de API."]);
    }

    tokenService.SetToken(loginResponse.Token);

    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, loginResponse.UserID),
        new Claim(ClaimTypes.Email, loginResponse.Email),
        new Claim(ClaimTypes.Name, loginResponse.Email)
    };
    claims.AddRange(loginResponse.Roles.Select(r => new Claim(ClaimTypes.Role, r)));

    return Results.Ok(new { success = true });
}).AllowAnonymous();

app.Run();
