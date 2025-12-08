using kakeibo.api.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace kakeibo.api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddServiceDefaults();

        // Connection string
        var connectionString = builder.Configuration.GetConnectionString("DatabaseConnection");

        // Register DbContext with Identity
        builder.Services.AddDbContext<KakeiboDBContext>(options =>
            options.UseSqlServer(connectionString));

        builder.Services.AddIdentity<IdentityUser, IdentityRole>()
            .AddEntityFrameworkStores<KakeiboDBContext>()
            .AddDefaultTokenProviders();

        // JWT configuration
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
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(jwtKey),
                ClockSkew = TimeSpan.Zero
            };
        });

        builder.Services.AddAuthorization();

        // OpenAPI / Swagger
        builder.Services.AddOpenApi();

        var app = builder.Build();

        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();

        // Map your endpoints
        app.MapUsersEndpoints();
        app.MapDefaultEndpoints();
        app.MapTiposDeDespesaEndpoints();
        app.MapTiposDeEntradasEndpoints();
        app.MapSaidasEndpoints();
        app.MapResultadosEndpoints();
        app.MapPlanoAnualEndpoints();
        app.MapEntradasEndpoints();
        app.MapDespesasEndpoints();

        

        // OpenAPI
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.Run();
    }
}
