using kakeibo.api.Data;
using kakeibo.api.Models;
using Microsoft.EntityFrameworkCore;

namespace kakeibo.api;

public static class MonthlyExpensesTotalsEndpoints
{
    public static void MapMonthlyExpensesTotalsEndpoints(this WebApplication app)
    {
        app.MapGet("/monthlyexpenses/{ano}/{mes}/{userId}",
        async (string ano, string mes, string userId, KakeiboDBContext db) =>
        {
            var item = await db.monthlyExpensesTotals
                .FirstOrDefaultAsync(x =>
                    x.Ano == ano &&
                    x.Mes == mes &&
                    x.UserID == userId);

            if (item == null)
            {
                item = new MonthlyExpensesTotals
                {
                    Ano = ano,
                    Mes = mes,
                    UserID = userId,
                    BankWithdrawals = 0,
                    FoodCosts = 0,
                    OtherExpenses = 0,
                    MonthlyTotal = 0,
                    SaldoParaMesSeguinte = 0
                };
            }

            return Results.Ok(item);
        })
        .RequireAuthorization();
    }
}
