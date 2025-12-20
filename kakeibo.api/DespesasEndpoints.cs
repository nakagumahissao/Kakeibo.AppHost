using kakeibo.api.Data;
using kakeibo.api.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace kakeibo.api
{
    public static class DespesasEndpoints
    {
        public static void MapDespesasEndpoints(this WebApplication app)
        {
            app.MapGet("/despesas/{UserID}", async (string UserID, KakeiboDBContext db, IStringLocalizer<SharedResources> L) =>
            {
                var data = await db.despesas
                    .Where(u => u.UserID == UserID)
                    .Include(d => d.TiposDeDespesa)
                    .OrderBy(u => u.NomeDespesa)
                    .ToListAsync();

                var result = data.Select(d => new
                {
                    d.DespesaID,
                    d.TipoDespesaID,
                    TipoDespesaNome = L[d.TiposDeDespesa!.TipoDeDespesa].Value,
                    d.UserID,
                    NomeDespesa = L[d.NomeDespesa].Value
                });

                return Results.Ok(result);
            })
            .RequireAuthorization();


            app.MapGet("/despesas/{id:decimal}", async (decimal id, KakeiboDBContext db) =>
            {
                var item = await db.despesas
                    .Where(u => u.DespesaID == id)
                    .Include(d => d.TiposDeDespesa)
                    .Select(d => new
                    {
                        d.DespesaID,
                        d.TipoDespesaID,
                        TipoDespesaNome = d.TiposDeDespesa!.TipoDeDespesa,
                        d.UserID,
                        d.NomeDespesa
                    })
                    .FirstOrDefaultAsync();

                return item is not null ? Results.Ok(item) : Results.NotFound();
            }).RequireAuthorization();

            app.MapPost("/despesas", async (Despesas desp, KakeiboDBContext db) =>
            {
                db.despesas.Add(desp);
                await db.SaveChangesAsync();
                return Results.Created($"/despesas/{desp.DespesaID}", desp);
            }).RequireAuthorization();

            app.MapPut("/despesas/{id:decimal}", async (decimal id, Despesas desp, KakeiboDBContext db) =>
            {
                var existing = await db.despesas.FindAsync(id);
                if (existing is null) return Results.NotFound();

                // Update properties (example, replace with your actual properties)
                existing.NomeDespesa = desp.NomeDespesa;
                existing.TipoDespesaID = desp.TipoDespesaID;

                await db.SaveChangesAsync();
                return Results.Ok(existing);
            }).RequireAuthorization();


            app.MapDelete("/despesas/{id:decimal}", async (decimal id, KakeiboDBContext db) =>
            {
                var r = await db.despesas.FindAsync(id);
                if (r is null) return Results.NotFound();

                db.despesas.Remove(r);
                await db.SaveChangesAsync();

                return Results.NoContent();
            }).RequireAuthorization();
        }
    }
}
