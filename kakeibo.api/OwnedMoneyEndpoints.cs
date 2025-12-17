using kakeibo.api.Data;
using Microsoft.EntityFrameworkCore;

namespace kakeibo.api;

public static class OwnedMoneyEndpoints
{
    public static void MapOwnedMoneyEndpoints(this WebApplication app)
    {
        app.MapGet("/ownedmoney/{userId}", async (string userId, KakeiboDBContext db) =>
        {
            var list = await db.ownedMoneys
                .Where(x => x.UserID == userId)
                .OrderBy(x => x.Ano)
                .ThenBy(x => x.Mes)
                .ToListAsync();

            return Results.Ok(list);
        }).RequireAuthorization();

        app.MapGet("/ownedmoney/{ano}/{mes}/{userId}", async (string ano, string mes, string userId, KakeiboDBContext db) =>
        {
            var item = await db.ownedMoneys
                .FirstOrDefaultAsync(x =>
                    x.Ano == ano &&
                    x.Mes == mes &&
                    x.UserID == userId);

            return item is not null ? Results.Ok(item) : Results.NotFound();
        }).RequireAuthorization();
    }
}
