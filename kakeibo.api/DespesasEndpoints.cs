using kakeibo.api.Data;
using Microsoft.EntityFrameworkCore;

namespace kakeibo.api
{
    public static class DespesasEndpoints
    {
        public static void MapDespesasEndpoints(this WebApplication app)
        {
            app.MapGet("/despesas/{UserID}", async (string UserID, KakeiboDBContext db) =>
            {
                var all = await db.despesas
                    .Include(d => d.TiposDeDespesa)
                    .Select(d => new
                    {
                        d.DespesaID,
                        d.TipoDespesaID,
                        TipoDespesaNome = d.TiposDeDespesa!.TipoDeDespesa,
                        d.UserID,
                        d.NomeDespesa
                    })
                    .Where(u => u.UserID == UserID)
                    .OrderBy(u => u.TipoDespesaNome)
                    .OrderBy(v =>  v.TipoDespesaNome)
                    .ToListAsync();

                return Results.Ok(all);
            }).RequireAuthorization();

            app.MapGet("/despesas/{id:decimal}", async (decimal id, KakeiboDBContext db) =>
            {
                var item = await db.despesas.FindAsync(id);
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
