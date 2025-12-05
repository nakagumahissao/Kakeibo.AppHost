using kakeibo.api.Data;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace kakeibo.api;

public static class UsersEndpoints
{
    public static void MapUsersEndpoints(this WebApplication app)
    {
        // =========================
        // USERS
        // =========================

        // List all users
        app.MapGet("/users", async (UserManager<IdentityUser> userManager) =>
        {
            var users = userManager.Users
                .Select(u => new { u.Id, u.UserName, u.Email })
                .ToList();
            return Results.Ok(users);
        }).RequireAuthorization(); // only authenticated admins should access

        // Get single user by ID
        app.MapGet("/users/{userId}", async (string userId, UserManager<IdentityUser> userManager) =>
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return Results.NotFound();
            return Results.Ok(new { user.Id, user.UserName, user.Email });
        }).RequireAuthorization();

        // Create user
        app.MapPost("/users", async (CreateUserRequest request, UserManager<IdentityUser> userManager) =>
        {
            var user = new IdentityUser { UserName = request.Email, Email = request.Email };
            var result = await userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded) return Results.BadRequest(result.Errors);
            return Results.Ok(new { user.Id, user.Email });
        }).RequireAuthorization();

        // Delete user
        app.MapDelete("/users/{userId}", async (string userId, UserManager<IdentityUser> userManager) =>
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return Results.NotFound();

            var result = await userManager.DeleteAsync(user);
            if (!result.Succeeded) return Results.BadRequest(result.Errors);

            return Results.NoContent();
        }).RequireAuthorization();

        // =========================
        // ROLES
        // =========================

        // List roles
        app.MapGet("/roles", async (RoleManager<IdentityRole> roleManager) =>
        {
            var roles = roleManager.Roles.Select(r => new { r.Id, r.Name }).ToList();
            return Results.Ok(roles);
        }).RequireAuthorization();

        // Create role
        app.MapPost("/roles", async (CreateRoleRequest request, RoleManager<IdentityRole> roleManager) =>
        {
            var role = new IdentityRole(request.Name);
            var result = await roleManager.CreateAsync(role);
            if (!result.Succeeded) return Results.BadRequest(result.Errors);

            return Results.Ok(role);
        }).RequireAuthorization();

        // Assign role to user
        app.MapPost("/users/{userId}/roles", async (string userId, AddUserRoleRequest request, UserManager<IdentityUser> userManager) =>
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return Results.NotFound();

            var result = await userManager.AddToRoleAsync(user, request.RoleName);
            if (!result.Succeeded) return Results.BadRequest(result.Errors);

            return Results.Ok();
        }).RequireAuthorization();

        // Remove role from user
        app.MapDelete("/users/{userId}/roles", async (string userId, string roleName, UserManager<IdentityUser> userManager) =>
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return Results.NotFound();

            var result = await userManager.RemoveFromRoleAsync(user, roleName);
            if (!result.Succeeded) return Results.BadRequest(result.Errors);

            return Results.Ok();
        }).RequireAuthorization();

        // =========================
        // CLAIMS
        // =========================

        // List claims of a user
        app.MapGet("/users/{userId}/claims", async (string userId, UserManager<IdentityUser> userManager) =>
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return Results.NotFound();

            var claims = await userManager.GetClaimsAsync(user);
            return Results.Ok(claims.Select(c => new { c.Type, c.Value }));
        }).RequireAuthorization();

        // Add claim to user
        app.MapPost("/users/{userId}/claims", async (string userId, AddClaimRequest request, UserManager<IdentityUser> userManager) =>
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return Results.NotFound();

            var claim = new Claim(request.Type, request.Value);
            var result = await userManager.AddClaimAsync(user, claim);
            if (!result.Succeeded) return Results.BadRequest(result.Errors);

            return Results.Ok();
        }).RequireAuthorization();

        // Remove claim from user
        app.MapDelete("/users/{userId}/claims", async (string userId, string type, string value, UserManager<IdentityUser> userManager) =>
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return Results.NotFound();

            var claim = new Claim(type, value);
            var result = await userManager.RemoveClaimAsync(user, claim);
            if (!result.Succeeded) return Results.BadRequest(result.Errors);

            return Results.Ok();
        }).RequireAuthorization();
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
