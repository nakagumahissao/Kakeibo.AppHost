using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace kakeibo.api;

public static class UsersEndpoints
{
    public static void MapUsersEndpoints(this WebApplication app)
    {
        // Auth endpoints
        app.MapPost("/register", async (RegisterRequest request, UserManager<IdentityUser> userManager) =>
        {
            var user = new IdentityUser { UserName = request.Email, Email = request.Email };
            var result = await userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded) return Results.BadRequest(result.Errors);

            return Results.Ok(new { user.Id, user.Email });
        });

        app.MapPost("/auth/login", async (HttpContext ctx, LoginRequest request, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signIn, IConfiguration config) =>
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user is null) return Results.Unauthorized();

            var result = await signIn.CheckPasswordSignInAsync(user, request.Password, false);
            if (!result.Succeeded) return Results.Unauthorized();

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id),  // <--- UserID no Token
        new Claim(ClaimTypes.Email, user.Email!)
    };

            // Generate JWT...
            var token = GenerateJwt(user, config);

            // ✔ WRITE COOKIE HERE — during HTTP response, legally allowed
            ctx.Response.Cookies.Append("jwt", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            });

            // Optional: return basic JSON, no token needed
            return Results.Ok(new LoginResponse(token, user.Email!, user.Id));
        });


        // =========================
        // UPDATE USER PROFILE
        // =========================
        app.MapPut("/users/profile", async (
            UpdateUserRequest request,
            HttpContext ctx,
            UserManager<IdentityUser> userManager) =>
        {
            var userId = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier); // <<< veio do JWT
            if (userId is null) return Results.Unauthorized();

            var user = await userManager.FindByIdAsync(userId);
            if (user is null) return Results.NotFound("Usuário não encontrado");

            user.Email = request.Email;
            user.UserName = request.UserName;

            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return Results.BadRequest(result.Errors);

            return Results.Ok(new { user.Id, user.Email, user.UserName });
        }).RequireAuthorization();

        app.MapPost("/users", async (CreateUserRequest request, UserManager<IdentityUser> userManager) =>
        {
            var user = new IdentityUser
            {
                UserName = request.Email,
                Email = request.Email
            };

            var result = await userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
                return Results.BadRequest(new ApiResponse<object>(false, null, result.Errors.Select(e => e.Description).ToArray()));

            return Results.Ok(new ApiResponse<object>(true, new { user.Id, user.Email }, Array.Empty<string>()));
        });


        app.MapPost("/users/change-password", async (ChangePasswordRequest request, HttpContext ctx, UserManager<IdentityUser> userManager) =>
        {
            var userId = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null)
                return Results.Json(
                    new { message = "Usuário não autenticado." },
                    statusCode: StatusCodes.Status401Unauthorized
                );

            var user = await userManager.FindByIdAsync(userId);
            if (user is null)
                return Results.Json(
                    new { message = "Usuário não existe." },
                    statusCode: StatusCodes.Status404NotFound
                );

            var passwordCheck = await userManager.CheckPasswordAsync(user, request.CurrentPassword);
            if (!passwordCheck)
                return Results.Json(
                    new { message = "Senha atual incorreta." },
                    statusCode: StatusCodes.Status400BadRequest
                );

            var result = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

            if (!result.Succeeded)
                return Results.Json(
                    new
                    {
                        message = "Não foi possível alterar a senha.",
                        errors = result.Errors.Select(e => e.Description)
                    },
                    statusCode: StatusCodes.Status400BadRequest
                );

            return Results.Json(new { message = "Senha alterada com sucesso!" });
        }).RequireAuthorization();


        // ============================================================
        // AQUI CONTINUAM OS ADMINS (caso queira manter!)
        // Esses ainda usam {userId} pois é ADMIN gerenciando outro user
        // ============================================================

        app.MapGet("/users", async (UserManager<IdentityUser> userManager) =>
        {
            var users = userManager.Users
                .Select(u => new { u.Id, u.UserName, u.Email })
                .ToList();
            return Results.Ok(users);
        }).RequireAuthorization("Admin");

        app.MapGet("/users/{userId}", async (string userId, UserManager<IdentityUser> userManager) =>
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return Results.NotFound();
            return Results.Ok(new { user.Id, user.UserName, user.Email });
        }).RequireAuthorization("Admin");

        app.MapDelete("/users/{userId}", async (string userId, UserManager<IdentityUser> userManager) =>
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return Results.NotFound();

            var result = await userManager.DeleteAsync(user);
            if (!result.Succeeded) return Results.BadRequest(result.Errors);

            return Results.NoContent();
        }).RequireAuthorization("Admin");

        // Roles + Claims = somente admins mantêm
        // (código permanece igual ao seu)
    }

    private static string GenerateJwt(IdentityUser user, IConfiguration config)
    {
        // FIXED LINE ↓↓↓ only ONE GetBytes, no syntax errors
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
        new Claim(ClaimTypes.NameIdentifier, user.Id),
        new Claim(ClaimTypes.Email, user.Email!)
    };

        var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );

        return new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);
    }
}


// =========================
// DTOs
// =========================

public record CreateUserRequest(string Email, string Password);
public record CreateRoleRequest(string Name);
public record AddUserRoleRequest(string RoleName);
public record RemoveUserRoleRequest(string RoleName);
public record AddClaimRequest(string Type, string Value);
public record RemoveClaimRequest(string Type, string Value);
public record UpdateUserRequest(string Email, string UserName);
public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
public record ApiResponse<T>(bool Success, T? Data, string[] Errors);
public record RegisterRequest(string Email, string Password);
public record LoginRequest(string Email, string Password);
public record LoginResponse(string Token, string Email, string UserID);