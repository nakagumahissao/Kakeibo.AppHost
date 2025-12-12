using kakeibo.api.Data;
using kakeibo.api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace kakeibo.api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddServiceDefaults();

        // Culture
        builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

        builder.Services.AddRequestLocalization(options =>
        {
            var supported = new[] { "en", "pt-BR", "ja" };
            options.SetDefaultCulture("pt-BR")
                .AddSupportedCultures(supported)
                .AddSupportedUICultures(supported);
        });

        builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

        builder.Services.AddMvc()
            .AddViewLocalization()
            .AddDataAnnotationsLocalization();
        // Culture End

        // EMail
        builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
        builder.Services.AddSingleton<IEmailService, EmailService>();

        // Client Settings
        builder.Services.Configure<ClientSettings>(builder.Configuration.GetSection("ClientSettings"));
        builder.Services.AddSingleton<IClientSettings, ClientSettingsService>();

        // 1. SERVICE CONFIGURATION PHASE (MUST BE BEFORE builder.Build())
        // ----------------------------------------------------------------

        // Connection string
        var connectionString = builder.Configuration.GetConnectionString("DatabaseConnection");

        // Register DbContext with Identity
        builder.Services.AddDbContext<KakeiboDBContext>(options =>
            options.UseSqlServer(connectionString));

        builder.Services.AddIdentity<IdentityUser, IdentityRole>()
            .AddEntityFrameworkStores<KakeiboDBContext>()
            .AddDefaultTokenProviders();

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
                // ... (your existing TokenValidationParameters)
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(jwtKey),
                ClockSkew = TimeSpan.Zero
            };
        });

        // Add Authorization service once
        builder.Services.AddAuthorization();

        // OpenAPI / Swagger
        builder.Services.AddOpenApi();

        // ----------------------------------------------------------------
        // 2. BUILD THE APPLICATION
        // ----------------------------------------------------------------
        var app = builder.Build();

        // 3. MIDDLEWARE CONFIGURATION PHASE (MUST BE AFTER builder.Build())
        // ----------------------------------------------------------------
        app.UseHttpsRedirection();

        // Use Authentication and Authorization middleware once
        app.UseAuthentication();
        app.UseAuthorization();

        // OpenAPI
        if (app.Environment.IsDevelopment())
        {
            // Swagger UI should generally be run first for easy debugging
            app.MapOpenApi();
        }

        // 4. ENDPOINT MAPPING PHASE
        // ----------------------------------------------------------------
        app.MapUsersEndpoints();
        app.MapDefaultEndpoints();
        app.MapTiposDeDespesaEndpoints();
        app.MapTiposDeEntradasEndpoints();
        app.MapSaidasEndpoints();
        app.MapResultadosEndpoints();
        app.MapPlanoAnualEndpoints();
        app.MapEntradasEndpoints();
        app.MapDespesasEndpoints();

        // 5. RUN THE APPLICATION
        // ----------------------------------------------------------------
        app.Run();
    }
}