using Kakeibo.AppHost.Web.Components;
using Kakeibo.AppHost.Web.Models;
using Kakeibo.AppHost.Web.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Localization;
using StackExchange.Redis;
using System.Globalization;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// ---------------- Serviços base ----------------
builder.AddServiceDefaults();
// builder.AddRedisOutputCache("redis");

// HttpContext para pegar usuário logado
builder.Services.AddHttpContextAccessor();

// ---------------- Redis ----------------
var redisConnectionString = builder.Configuration.GetConnectionString("redis");
if (string.IsNullOrEmpty(redisConnectionString))
{
    // Mensagem de erro fixa, não podemos usar L[] aqui
    throw new InvalidOperationException("Redis connection string not found.");
}

var redis = ConnectionMultiplexer.Connect(redisConnectionString);

// ---------------- Culture ----------------
var supportedCultures = new[]
{
    new CultureInfo("en"),
    new CultureInfo("pt-BR"),
    new CultureInfo("ja")
};

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

// ---------------- Blazor Server ----------------
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorWithCredentials", policy =>
    {
        policy.WithOrigins("https://localhost:7030")
              .AllowCredentials()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorization();

// ---------------- Output Caching Setup ----------------
builder.Services.AddOutputCache();
builder.Services.AddStackExchangeRedisOutputCache(options =>
{
    options.Configuration = redisConnectionString;
    options.InstanceName = "KakeiboOutputCache";
});

var app = builder.Build();

// ---------------- Request pipeline ----------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseCors("AllowBlazorWithCredentials");
app.UseForwardedHeaders();
app.UseHttpsRedirection();
app.UseAntiforgery();
app.UseOutputCache();
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
    var client = clientFactory.CreateClient("apis");

    var response = await client.PostAsJsonAsync("auth/login", loginModel);

    if (!response.IsSuccessStatusCode)
        return Results.Json(new { error = L["Invalid credentials"] }, statusCode: StatusCodes.Status401Unauthorized);

    var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
    if (loginResponse == null)
        return Results.Problem(L["Invalid API response format."]);

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

// =======================================================
// Mensagens fixas adicionais podem ser adaptadas nos endpoints futuros
// usando IStringLocalizer<SharedResources> injetado via DI
// =======================================================

app.MapDefaultEndpoints();
app.Run();
