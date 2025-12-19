using kakeibo.api.Resources;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Security.Claims;
using System.Text;

namespace kakeibo.api;

public static class UsersEndpoints
{
    public static void MapUsersEndpoints(this WebApplication app)
    {
        // =========================
        // CONFIRM PASSWORD RESET
        // =========================
        app.MapPost("/auth/confirm-reset-password", async (
            [FromBody] ConfirmResetPasswordRequest request,
            UserManager<IdentityUser> userManager,
            IStringLocalizer<SharedResources> L
        ) =>
        {
            if (string.IsNullOrWhiteSpace(request.UserId) ||
                string.IsNullOrWhiteSpace(request.NewPassword) ||
                string.IsNullOrWhiteSpace(request.Token))
            {
                return Results.Json(
                    new { error = L["UserId, token e nova senha são obrigatórios."] },
                    statusCode: 400
                );
            }

            var user = await userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                return Results.Json(
                    new { error = L["Usuário não encontrado."] },
                    statusCode: 404
                );
            }

            try
            {
                var decodedToken = Uri.UnescapeDataString(request.Token);
                var result = await userManager.ResetPasswordAsync(user, decodedToken, request.NewPassword);

                if (!result.Succeeded)
                {
                    return Results.Json(
                        new
                        {
                            error = L["Falha ao redefinir a senha."],
                            details = result.Errors
                        },
                        statusCode: 400
                    );
                }

                return Results.Ok(new
                {
                    message = L["Senha redefinida com sucesso! Agora você deve fazer login novamente."]
                });
            }
            catch (Exception ex)
            {
                return Results.Json(
                    new { error = L["Erro inesperado: {0}", ex.Message] },
                    statusCode: 500
                );
            }
        });

        // =========================
        // REGISTER
        // =========================
        app.MapPost("/register", async (
            [FromBody] RegisterRequest request,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IStringLocalizer<SharedResources> L
        ) =>
        {
            var user = new IdentityUser { UserName = request.Email, Email = request.Email };
            var result = await userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
                return Results.BadRequest(result.Errors);

            if (!await roleManager.RoleExistsAsync("User"))
                await roleManager.CreateAsync(new IdentityRole("User"));

            await userManager.AddToRoleAsync(user, "User");

            return Results.Ok(new ApiResponse<object>(
                true,
                new { user.Id, user.Email },
                Array.Empty<string>()
            ));
        });

        // =========================
        // LOGIN
        // =========================
        app.MapPost("/auth/login", async ([FromBody] LoginRequest request, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signIn, IConfiguration config) =>
        {
            Log.Information("Login attempt for {Email}", request.Email);

            var user = await userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                Log.Warning("Login failed for {Email}: User not found", request.Email);
                return Results.Unauthorized();
            }

            var result = await signIn.CheckPasswordSignInAsync(user, request.Password, false);
            if (!result.Succeeded)
            {
                Log.Warning("Login failed for {Email}: Invalid password", request.Email);
                return Results.Unauthorized();
            }

            var roles = await userManager.GetRolesAsync(user);

            var culture = string.IsNullOrWhiteSpace(request.CultureName)
                ? "pt-BR"
                : request.CultureName;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim("culture", culture) // ✅ CORRECT PLACE
            };

            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var token = GenerateJwt(claims, config);

            return Results.Ok(new LoginResponse(
                Token: token,
                Email: user.Email!,
                UserID: user.Id,
                Roles: roles.ToList()
            ));
        });


        // =========================
        // CHANGE PASSWORD
        // =========================
        app.MapPost("/users/change-password", async (
            [FromBody] ChangePasswordRequest request,
            HttpContext ctx,
            UserManager<IdentityUser> userManager,
            IStringLocalizer<SharedResources> L
        ) =>
        {
            var userId = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Results.Json(new { message = L["Usuário não autenticado."] },
                    statusCode: StatusCodes.Status401Unauthorized);

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
                return Results.Json(new { message = L["Usuário não existe."] },
                    statusCode: StatusCodes.Status404NotFound);

            var passwordCheck = await userManager.CheckPasswordAsync(user, request.CurrentPassword);
            if (!passwordCheck)
                return Results.Json(new { message = L["Senha atual incorreta."] },
                    statusCode: StatusCodes.Status400BadRequest);

            var change = await userManager.ChangePasswordAsync(
                user, request.CurrentPassword, request.NewPassword);

            if (!change.Succeeded)
            {
                return Results.Json(
                    new
                    {
                        message = L["Erro"],
                        errors = change.Errors.Select(e => e.Description)
                    },
                    statusCode: StatusCodes.Status400BadRequest
                );
            }

            return Results.Json(new { message = L["Senha alterada com sucesso!"] });
        }).RequireAuthorization();

        // =========================
        // USERS CRUD (Admin)
        // =========================
        app.MapGet("/users", async (UserManager<IdentityUser> userManager) =>
            Results.Ok(userManager.Users.Select(u => new
            {
                u.Id,
                u.UserName,
                u.Email
            }).ToList())
        ).RequireAuthorization("Admin");

        app.MapGet("/users/{userId}", async (string userId, UserManager<IdentityUser> userManager) =>
        {
            var user = await userManager.FindByIdAsync(userId);
            return user == null
                ? Results.NotFound()
                : Results.Ok(new { user.Id, user.UserName, user.Email });
        }).RequireAuthorization("Admin");

        app.MapPost("/users", async (
            [FromBody] CreateUserRequest request,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager
        ) =>
        {
            var user = new IdentityUser { UserName = request.Email, Email = request.Email };
            var result = await userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
                return Results.BadRequest(result.Errors);

            if (!await roleManager.RoleExistsAsync("User"))
                await roleManager.CreateAsync(new IdentityRole("User"));

            await userManager.AddToRoleAsync(user, "User");
            return Results.Ok(new { user.Id, user.Email });
        }).RequireAuthorization("Admin");

        app.MapDelete("/users/{userId}", async (string userId, UserManager<IdentityUser> userManager) =>
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return Results.NotFound();
            var result = await userManager.DeleteAsync(user);
            return result.Succeeded ? Results.NoContent() : Results.BadRequest(result.Errors);
        }).RequireAuthorization("Admin");

        // =========================
        // ROLES CRUD (Admin)
        // =========================
        app.MapGet("/roles", async (RoleManager<IdentityRole> roleManager) =>
            Results.Ok(roleManager.Roles.Select(r => new { r.Id, r.Name }).ToList())
        ).RequireAuthorization("Admin");

        app.MapPost("/roles", async (
            [FromBody] CreateRoleRequest request,
            RoleManager<IdentityRole> roleManager,
            IStringLocalizer<SharedResources> L
        ) =>
        {
            if (await roleManager.RoleExistsAsync(request.Name))
                return Results.BadRequest(L["Role já existe"]);

            await roleManager.CreateAsync(new IdentityRole(request.Name));
            return Results.Ok();
        }).RequireAuthorization("Admin");

        app.MapDelete("/roles/{roleName}", async (
            string roleName,
            RoleManager<IdentityRole> roleManager
        ) =>
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role == null) return Results.NotFound();
            var result = await roleManager.DeleteAsync(role);
            return result.Succeeded ? Results.NoContent() : Results.BadRequest(result.Errors);
        }).RequireAuthorization("Admin");

        // =========================
        // USER-ROLE MANAGEMENT
        // =========================
        app.MapPost("/users/{userId}/roles", async (
            string userId,
            [FromBody] AddUserRoleRequest request,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IStringLocalizer<SharedResources> L
        ) =>
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return Results.NotFound();

            if (!await roleManager.RoleExistsAsync(request.RoleName))
                return Results.BadRequest(L["Role não existe"]);

            await userManager.AddToRoleAsync(user, request.RoleName);
            return Results.Ok();
        }).RequireAuthorization("Admin");

        app.MapDelete("/users/{userId}/roles", async (
            string userId,
            [FromBody] RemoveUserRoleRequest request,
            UserManager<IdentityUser> userManager
        ) =>
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return Results.NotFound();
            await userManager.RemoveFromRoleAsync(user, request.RoleName);
            return Results.Ok();
        }).RequireAuthorization("Admin");

        // =========================
        // USER-CLAIMS MANAGEMENT
        // =========================
        app.MapPost("/users/{userId}/claims", async (
            string userId,
            [FromBody] AddClaimRequest request,
            UserManager<IdentityUser> userManager
        ) =>
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return Results.NotFound();
            await userManager.AddClaimAsync(user, new Claim(request.Type, request.Value));
            return Results.Ok();
        }).RequireAuthorization("Admin");

        app.MapDelete("/users/{userId}/claims", async (
            string userId,
            [FromBody] RemoveClaimRequest request,
            UserManager<IdentityUser> userManager
        ) =>
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return Results.NotFound();

            var claim = (await userManager.GetClaimsAsync(user))
                .FirstOrDefault(c => c.Type == request.Type && c.Value == request.Value);

            if (claim != null)
                await userManager.RemoveClaimAsync(user, claim);

            return Results.Ok();
        }).RequireAuthorization("Admin");

        // =========================
        // PASSWORD RESET
        // =========================
        app.MapPost("/auth/reset-password", async (
            [FromBody] PasswordResetRequest request,
            UserManager<IdentityUser> userManager,
            IEmailService emailService,
            IStringLocalizer<SharedResources> L
        ) =>
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                return Results.Json(new { error = L["O e-mail é obrigatório."] }, statusCode: 400);

            var user = await userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return Results.Json(new { error = L["E-mail não cadastrado."] }, statusCode: 404);

            try
            {
                var token = await userManager.GeneratePasswordResetTokenAsync(user);

                var emailSent = await emailService.SendPasswordResetEmailAsync(
                    user.Email!,
                    user.Id,
                    token
                );

                if (!emailSent)
                    return Results.Json(new { error = L["Falha ao enviar o e-mail de redefinição."] },
                        statusCode: 500);

                return Results.Ok(new { message = L["E-mail de redefinição de senha enviado com sucesso."] });
            }
            catch (Exception ex)
            {
                return Results.Json(
                    new { error = L["Erro inesperado: {0}", ex.Message] },
                    statusCode: 500
                );
            }
        });
    }

    // =========================
    // JWT Helper
    // =========================
    private static string GenerateJwt(IEnumerable<Claim> claims, IConfiguration config)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );

        return new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);
    }

    // =========================
    // DTOs
    // =========================
    public record LoginResponse(string Token, string Email, string UserID, List<string> Roles);
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
    public record LoginRequest(string Email, string Password, string CultureName);
    public record PasswordResetRequest(string Email);
    public record ConfirmResetPasswordRequest(string UserId, string NewPassword, string Token);
}
