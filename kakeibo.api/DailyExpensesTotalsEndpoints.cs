using kakeibo.api.Data;
using Microsoft.EntityFrameworkCore;

namespace kakeibo.api;

public static class DailyExpensesTotalsEndpoints
{
    public static void MapDailyExpensesTotalsEndpoints(this WebApplication app)
    {
        app.MapGet("/dailyexpenses/{userId}/{ano:int}/{mes:int}/{dia:int}", async (string userId, int ano, int mes, int dia, KakeiboDBContext db) =>
        {
            var dataSaida = new DateTime(ano, mes, dia);

            var item = await db.dailyExpensesTotals
                .FirstOrDefaultAsync(x =>
                    x.UserID == userId &&
                    x.Ano == ano.ToString("0000") &&
                    x.Mes == mes.ToString("00") &&
                    x.DataSaida == dataSaida);

            return item is null
                ? Results.NotFound()
                : Results.Ok(item);
        }).RequireAuthorization();

        app.MapGet("/dailyexpenses/{id:decimal}", async (decimal id, KakeiboDBContext db) =>
        {
            var item = await db.dailyExpensesTotals
                .FirstOrDefaultAsync(x => x.DailyRecordID == id);

            return item is not null ? Results.Ok(item) : Results.NotFound();
        }).RequireAuthorization();
    }
}
