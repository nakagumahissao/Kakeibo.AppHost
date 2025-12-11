using Kakeibo.AppHost.Web.Components;
using Kakeibo.AppHost.Web.Models;
using Kakeibo.AppHost.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using System.Security.Claims;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// ---------------- Serviços base ----------------
builder.AddServiceDefaults();
// builder.AddRedisOutputCache("redis");

// HttpContext para pegar usuário logado
builder.Services.AddHttpContextAccessor();

var redisConnectionString = builder.Configuration.GetConnectionString("redis")
    ?? throw new InvalidOperationException("Redis connection string 'redis' not found.");

var redis = ConnectionMultiplexer.Connect(redisConnectionString);

// This stabilizes the key used to encrypt the authentication cookie.
builder.Services.AddDataProtection()
    .SetApplicationName("KakeiboWebFrontend")
    .PersistKeysToStackExchangeRedis(
        redis
    );

// ---------------- JWT/Cookie Hybrid Setup ----------------

// 1. Register the services needed for JWT handling
builder.Services.AddSingleton<TokenService>();
builder.Services.AddTransient<JwtAuthorizationHandler>();

// Removed the incorrect ForwardedIdentityHttpClientHandler registration
// and the redundant IAuthenticationService registration.

// 2. Update the HttpClient registration for "apis"
builder.Services.AddHttpClient("apis", c =>
{
    c.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]!);
})
// Add the JwtAuthorizationHandler to attach the Bearer token
.AddHttpMessageHandler<JwtAuthorizationHandler>();

// ---------------- Blazor Server ----------------
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

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
// 1. Register the base output cache services
builder.Services.AddOutputCache();

// 2. Register the Redis store implementation using the existing connection string
builder.Services.AddStackExchangeRedisOutputCache(options =>
{
    // Uses the connection string successfully retrieved above
    options.Configuration = redisConnectionString;
    options.InstanceName = "KakeiboOutputCache"; // Use a distinct instance name
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
    HttpContext httpContext,
    TokenService tokenService
) =>
{
    var client = clientFactory.CreateClient("apis");

    // 1. Call the API to validate credentials
    var response = await client.PostAsJsonAsync("auth/login", loginModel);

    if (!response.IsSuccessStatusCode)
        return Results.Json(new { error = "Credenciais inválidas" }, statusCode: StatusCodes.Status401Unauthorized);

    var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
    if (loginResponse == null)
        return Results.Problem("Invalid API response format.");

    // Store JWT in server-side TokenService (for Blazor Server circuit)
    tokenService.SetToken(loginResponse.Token);

    // 2. Create claims and sign in using cookie auth (visible to browser)
    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, loginResponse.UserID),
        new Claim(ClaimTypes.Email, loginResponse.Email),
        new Claim(ClaimTypes.Name, loginResponse.Email)
    };
    claims.AddRange(loginResponse.Roles.Select(r => new Claim(ClaimTypes.Role, r)));

    return Results.Ok(new { success = true }); // client will navigate manually
}).AllowAnonymous();

app.MapDefaultEndpoints();
app.Run();