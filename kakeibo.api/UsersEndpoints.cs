using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;

namespace kakeibo.api;

public static class UsersEndpoints
{
    public static void MapUsersEndpoints(this WebApplication app)
    {
        // =========================
        // REGISTER
        // =========================
        app.MapPost("/register", async ([FromBody] RegisterRequest request, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager) =>
        {
            var user = new IdentityUser { UserName = request.Email, Email = request.Email };
            var result = await userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded) return Results.BadRequest(result.Errors);

            if (!await roleManager.RoleExistsAsync("User")) await roleManager.CreateAsync(new IdentityRole("User"));
            await userManager.AddToRoleAsync(user, "User");

            return Results.Ok(new ApiResponse<object>(true, new { user.Id, user.Email }, Array.Empty<string>()));
        });

        // =========================
        // LOGIN
        // =========================
        app.MapPost("/auth/login", async ([FromBody] LoginRequest request, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signIn, IConfiguration config) =>
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user == null) return Results.Unauthorized();

            var result = await signIn.CheckPasswordSignInAsync(user, request.Password, false);
            if (!result.Succeeded) return Results.Unauthorized();

            var roles = await userManager.GetRolesAsync(user);
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, user.Id), new Claim(ClaimTypes.Email, user.Email!) };
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
        app.MapPost("/users/change-password", async ([FromBody] ChangePasswordRequest request, HttpContext ctx, UserManager<IdentityUser> userManager) =>
        {
            var userId = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Results.Json(new { message = "Usuário não autenticado." }, statusCode: StatusCodes.Status401Unauthorized);

            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return Results.Json(new { message = "Usuário não existe." }, statusCode: StatusCodes.Status404NotFound);

            var passwordCheck = await userManager.CheckPasswordAsync(user, request.CurrentPassword);
            if (!passwordCheck) return Results.Json(new { message = "Senha atual incorreta." }, statusCode: StatusCodes.Status400BadRequest);

            var change = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
            if (!change.Succeeded) return Results.Json(new { message = "Erro", errors = change.Errors.Select(e => e.Description) }, statusCode: StatusCodes.Status400BadRequest);

            return Results.Json(new { message = "Senha alterada com sucesso!" });
        }).RequireAuthorization();
        
        // =========================
        // USERS CRUD (Admin)
        // =========================
        app.MapGet("/users", async (UserManager<IdentityUser> userManager) => Results.Ok(userManager.Users.Select(u => new { u.Id, u.UserName, u.Email }).ToList())).RequireAuthorization("Admin");
        app.MapGet("/users/{userId}", async (string userId, UserManager<IdentityUser> userManager) => { var user = await userManager.FindByIdAsync(userId); return user == null ? Results.NotFound() : Results.Ok(new { user.Id, user.UserName, user.Email }); }).RequireAuthorization("Admin");
        app.MapPost("/users", async ([FromBody] CreateUserRequest request, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager) => { var user = new IdentityUser { UserName = request.Email, Email = request.Email }; var result = await userManager.CreateAsync(user, request.Password); if (!result.Succeeded) return Results.BadRequest(result.Errors); if (!await roleManager.RoleExistsAsync("User")) await roleManager.CreateAsync(new IdentityRole("User")); await userManager.AddToRoleAsync(user, "User"); return Results.Ok(new { user.Id, user.Email }); }).RequireAuthorization("Admin");
        app.MapDelete("/users/{userId}", async (string userId, UserManager<IdentityUser> userManager) => { var user = await userManager.FindByIdAsync(userId); if (user == null) return Results.NotFound(); var result = await userManager.DeleteAsync(user); return result.Succeeded ? Results.NoContent() : Results.BadRequest(result.Errors); }).RequireAuthorization("Admin");

        // =========================
        // ROLES CRUD (Admin)
        // =========================
        app.MapGet("/roles", async (RoleManager<IdentityRole> roleManager) => Results.Ok(roleManager.Roles.Select(r => new { r.Id, r.Name }).ToList())).RequireAuthorization("Admin");
        app.MapPost("/roles", async ([FromBody] CreateRoleRequest request, RoleManager<IdentityRole> roleManager) => { if (await roleManager.RoleExistsAsync(request.Name)) return Results.BadRequest("Role já existe"); await roleManager.CreateAsync(new IdentityRole(request.Name)); return Results.Ok(); }).RequireAuthorization("Admin");
        app.MapDelete("/roles/{roleName}", async (string roleName, RoleManager<IdentityRole> roleManager) => { var role = await roleManager.FindByNameAsync(roleName); if (role == null) return Results.NotFound(); var result = await roleManager.DeleteAsync(role); return result.Succeeded ? Results.NoContent() : Results.BadRequest(result.Errors); }).RequireAuthorization("Admin");

        // =========================
        // USER-ROLE MANAGEMENT
        // =========================
        app.MapPost("/users/{userId}/roles", async (string userId, [FromBody] AddUserRoleRequest request, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager) => { var user = await userManager.FindByIdAsync(userId); if (user == null) return Results.NotFound(); if (!await roleManager.RoleExistsAsync(request.RoleName)) return Results.BadRequest("Role não existe"); await userManager.AddToRoleAsync(user, request.RoleName); return Results.Ok(); }).RequireAuthorization("Admin");
        app.MapDelete("/users/{userId}/roles", async (string userId, [FromBody] RemoveUserRoleRequest request, UserManager<IdentityUser> userManager) => { var user = await userManager.FindByIdAsync(userId); if (user == null) return Results.NotFound(); await userManager.RemoveFromRoleAsync(user, request.RoleName); return Results.Ok(); }).RequireAuthorization("Admin");

        // =========================
        // USER-CLAIMS MANAGEMENT
        // =========================
        app.MapPost("/users/{userId}/claims", async (string userId, [FromBody] AddClaimRequest request, UserManager<IdentityUser> userManager) =>
        {
            var user = await userManager.FindByIdAsync(userId);

            if (user == null) return Results.NotFound();

            await userManager.AddClaimAsync(user, new Claim(request.Type, request.Value));

            return Results.Ok();
        }).RequireAuthorization("Admin");

        app.MapDelete("/users/{userId}/claims", async (string userId, [FromBody] RemoveClaimRequest request, UserManager<IdentityUser> userManager) => { var user = await userManager.FindByIdAsync(userId); if (user == null) return Results.NotFound(); var claim = (await userManager.GetClaimsAsync(user)).FirstOrDefault(c => c.Type == request.Type && c.Value == request.Value); if (claim != null) await userManager.RemoveClaimAsync(user, claim); return Results.Ok(); }).RequireAuthorization("Admin");

    } 

    // The GenerateJwt helper method must be defined inside the class but outside the MapUsersEndpoints method.
    private static string GenerateJwt(IEnumerable<Claim> claims, IConfiguration config)
    {
        // 1. Get the secret key from configuration
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // 2. Create the JWT Security Token
        var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7), // Token expires in 7 days
            signingCredentials: creds
        );

        // 3. Write the token
        return new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);
    }

    // =========================
    // DTOs (Defined inside the class or namespace)
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
    public record LoginRequest(string Email, string Password);

} 