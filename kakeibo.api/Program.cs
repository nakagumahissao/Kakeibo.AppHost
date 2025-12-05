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

        // Auth endpoints
        app.MapPost("/register", async (RegisterRequest request, UserManager<IdentityUser> userManager) =>
        {
            var user = new IdentityUser { UserName = request.Email, Email = request.Email };
            var result = await userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded) return Results.BadRequest(result.Errors);

            return Results.Ok(new { user.Id, user.Email });
        });

        app.MapPost("/login", async (LoginRequest request, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IConfiguration config) =>
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user == null) return Results.Unauthorized();

            var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (!result.Succeeded) return Results.Unauthorized();

            // Generate JWT
            var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(config["Jwt:Key"]!);

            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, user.Id) };
            if (!string.IsNullOrEmpty(user.Email))
                claims.Add(new Claim(ClaimTypes.Email, user.Email));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(8),
                Issuer = config["Jwt:Issuer"],
                Audience = config["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwt = tokenHandler.WriteToken(token);

            return Results.Ok(new { token = jwt });
        });

        // OpenAPI
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.Run();
    }
}

// DTOs
public record RegisterRequest(string Email, string Password);
public record LoginRequest(string Email, string Password);
