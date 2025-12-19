using kakeibo.api.Data;
using kakeibo.api.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace kakeibo.api
{
    public static class DespesasVariaveisEndpoints
    {
        public static void MapDespesasVariaveisEndpoints(this WebApplication app)
        {
            app.MapGet("/despesasvariaveis/{UserID}/{Mes}/{Ano}", async (
                string UserID,
                string Mes,
                string Ano,
                KakeiboDBContext db,
                IStringLocalizer<SharedResources> L
            ) =>
            {
                var data = await db.despesasVariaveis
                    .AsNoTracking()
                    .Where(u => u.UserID == UserID && u.Mes == Mes && u.Ano == Ano)
                    .OrderBy(u => u.NomeDespesa)
                    .ToListAsync();

                var result = data.Select(u => new
                {
                    u.UserID,
                    NomeDespesa = L[u.NomeDespesa].Value,
                    u.TotalDestaCategoria,
                    u.Mes,
                    u.Ano
                });

                return Results.Ok(result);
            })
            .RequireAuthorization();
        }
    }
}
