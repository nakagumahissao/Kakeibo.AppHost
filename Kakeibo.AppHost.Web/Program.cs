using Kakeibo.AppHost.Web;
using Kakeibo.AppHost.Web.Components;
using Kakeibo.AppHost.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Kakeibo.AppHost.Web.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Default Aspire & integrations
builder.AddServiceDefaults();
builder.AddRedisOutputCache("cache");

// Token service apenas se quiser leitura do cookie para API Header
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<TokenService>();

// correct HttpClient factory for API access
builder.Services.AddHttpClient("apis", c =>
{
    c.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]!);
});

// UI Blazor Server
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Authentication via Cookie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.LoginPath = "/login";     // página login
        options.LogoutPath = "/logout";   // endpoint logout
        options.AccessDeniedPath = "/denied";
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();
app.UseOutputCache();
app.MapStaticAssets();

// Middleware necessário para autenticação funcionar
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapPost("/blazor-login", async (LoginModel loginModel, IHttpClientFactory clientFactory, IHttpContextAccessor httpContext) =>
{
    var client = clientFactory.CreateClient("apis");

    // Chama API para validar login
    var response = await client.PostAsJsonAsync("/auth/login", loginModel);

    if (!response.IsSuccessStatusCode)
        return Results.Unauthorized();

    var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();

    // Criar Claims e SignInAsync no Blazor Server
    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, loginResponse!.UserId),
        new Claim(ClaimTypes.Email, loginResponse.Email),
        new Claim(ClaimTypes.Name, loginResponse.Email)
    };

    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var principal = new ClaimsPrincipal(identity);

    await httpContext.HttpContext!.SignInAsync(
        CookieAuthenticationDefaults.AuthenticationScheme,
        principal,
        new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTime.UtcNow.AddHours(12)
        });

    // Retorna JWT para chamadas API
    return Results.Ok(new { token = loginResponse.Token });
})
.AllowAnonymous();

app.MapDefaultEndpoints();

app.Run();
