using kakeibo.api.Data;
using Microsoft.EntityFrameworkCore;

namespace kakeibo.api
{
    public static class DespesasVariaveisEndpoints
    {
        public static void MapDespesasVariaveisEndpoints(this WebApplication app)
        {
            app.MapGet("/despesasvariaveis/{UserID}/{Mes}/{Ano}", async (string UserID, string Mes, string Ano, KakeiboDBContext db) =>
            {
                var all = await db.despesasVariaveis
                    .AsNoTracking()
                    .Where(u => u.UserID == UserID && u.Mes == Mes && u.Ano == Ano)                  
                    .OrderBy(u => u.NomeDespesa)
                    .ToListAsync();

                return Results.Ok(all);
            }).RequireAuthorization();
        }
    }
}
