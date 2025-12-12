using kakeibo.api.Data;
using kakeibo.api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Globalization;
using System.Net;
using System.Text;

namespace kakeibo.api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // ---------------------------------------------------------
        // 1. KESTREL HTTPS CONFIG
        // ---------------------------------------------------------
        var certPath = builder.Configuration["Kestrel:HttpsCertificate:Path"];
        var certPassword = builder.Configuration["Kestrel:HttpsCertificate:Password"];

        builder.WebHost.ConfigureKestrel(options =>
        {
            // HTTPS on port 447
            options.Listen(IPAddress.Parse("100.64.1.29"), 447, listenOptions =>
            {
                listenOptions.UseHttps(certPath!, certPassword);
            });
        });

        // ---------------------------------------------------------
        // 2. LOCALIZATION
        // ---------------------------------------------------------
        builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

        builder.Services.AddRequestLocalization(options =>
        {
            var supportedCultures = new[]
            {
                new CultureInfo("pt-BR"),
                new CultureInfo("de"),
                new CultureInfo("en"),
                new CultureInfo("es"),
                new CultureInfo("fr"),
                new CultureInfo("zh-CN"),
                new CultureInfo("ja")
            };

            options.SetDefaultCulture("pt-BR")
                   .AddSupportedCultures(supportedCultures.Select(c => c.Name).ToArray())
                   .AddSupportedUICultures(supportedCultures.Select(c => c.Name).ToArray());
        });

        builder.Services.AddMvc()
            .AddViewLocalization()
            .AddDataAnnotationsLocalization();

        // ---------------------------------------------------------
        // 3. WINDOWS SERVICE SUPPORT
        // ---------------------------------------------------------
        builder.Host.UseWindowsService();

        // ---------------------------------------------------------
        // 4. SERILOG
        // ---------------------------------------------------------
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        builder.Host.UseSerilog();

        Log.Information("Starting Kakeibo Minimal API Service...");

        // ---------------------------------------------------------
        // 5. CORS – allow only your Blazor server origin
        // ---------------------------------------------------------
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowBlazorWithCredentials", policy =>
            {
                policy.WithOrigins("https://100.64.1.29:446")
                      .AllowCredentials()
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            });
        });

        // ---------------------------------------------------------
        // 6. SETTINGS & SERVICES
        // ---------------------------------------------------------
        builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
        builder.Services.AddSingleton<IEmailService, EmailService>();

        builder.Services.Configure<ClientSettings>(builder.Configuration.GetSection("ClientSettings"));
        builder.Services.AddSingleton<IClientSettings, ClientSettingsService>();

        // ---------------------------------------------------------
        // 7. DATABASE + IDENTITY
        // ---------------------------------------------------------
        var connectionString = builder.Configuration.GetConnectionString("DatabaseConnection");

        builder.Services.AddDbContext<KakeiboDBContext>(options =>
            options.UseSqlServer(connectionString));

        builder.Services.AddIdentity<IdentityUser, IdentityRole>()
            .AddEntityFrameworkStores<KakeiboDBContext>()
            .AddDefaultTokenProviders();

        // ---------------------------------------------------------
        // 8. JWT AUTHENTICATION
        // ---------------------------------------------------------
        var jwtKey = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!);

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = "JwtBearer";
            options.DefaultChallengeScheme = "JwtBearer";
        })
        .AddJwtBearer("JwtBearer", options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(jwtKey),
                ClockSkew = TimeSpan.Zero
            };
        });

        builder.Services.AddAuthorization();

        // ---------------------------------------------------------
        // 9. OPENAPI / SWAGGER
        // ---------------------------------------------------------
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddOpenApi();

        // ---------------------------------------------------------
        // 10. BUILD APP
        // ---------------------------------------------------------
        var app = builder.Build();

        // ---------------------------------------------------------
        // 11. MIDDLEWARE PIPELINE
        // ---------------------------------------------------------
        app.UseHttpsRedirection();

        app.UseRequestLocalization();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseCors("AllowBlazorWithCredentials");

        // ---- Enable Swagger ALWAYS (your API is internal only) ----
        app.MapOpenApi();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/openapi/v1.json", "Kakeibo API v1");
            options.RoutePrefix = "swagger"; // URL = /swagger
        });

        // ---------------------------------------------------------
        // 12. ENDPOINTS
        // ---------------------------------------------------------
        app.MapUsersEndpoints();
        app.MapTiposDeDespesaEndpoints();
        app.MapTiposDeEntradasEndpoints();
        app.MapSaidasEndpoints();
        app.MapResultadosEndpoints();
        app.MapPlanoAnualEndpoints();
        app.MapEntradasEndpoints();
        app.MapDespesasEndpoints();

        // ---------------------------------------------------------
        // 13. RUN
        // ---------------------------------------------------------
        app.Run();
    }
}
