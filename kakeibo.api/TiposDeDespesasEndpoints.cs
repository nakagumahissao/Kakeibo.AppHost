using kakeibo.api.Data;
using kakeibo.api.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace kakeibo.api
{
    public static class TiposDeDespesasEndpoints
    {
        public static void MapTiposDeDespesaEndpoints(this WebApplication app)
        {
            app.MapGet("/tiposdespesa/{UserID}", async (string UserID, KakeiboDBContext db, IStringLocalizer<SharedResources> L) =>
            {
                var all = await db.tiposDeDespesas
                    .AsNoTracking()
                    .Where(w => w.UserID == UserID)
                    .Select(s => new TiposDeDespesa
                    {
                        TipoDespesaID = s.TipoDespesaID,
                        TipoDeDespesa = L[s.TipoDeDespesa],
                        UserID = s.UserID
                    })
                    .ToListAsync();
                return Results.Ok(all);
            }).RequireAuthorization();

            app.MapGet("/tiposdespesa/{id:decimal}", async (decimal id, KakeiboDBContext db) =>
            {
                var item = await db.tiposDeDespesas.FindAsync(id);
                return item is not null ? Results.Ok(item) : Results.NotFound();
            }).RequireAuthorization();

            app.MapPost("/tiposdespesa", async (TiposDeDespesa tipo, KakeiboDBContext db) =>
            {
                db.tiposDeDespesas.Add(tipo);
                await db.SaveChangesAsync();
                return Results.Created($"/tiposdespesa/{tipo.TipoDespesaID}", tipo);
            }).RequireAuthorization();

            app.MapPut("/tiposdespesa/{id:decimal}", async (decimal id, TiposDeDespesa updatedTipo, KakeiboDBContext db) =>
            {
                var tipo = await db.tiposDeDespesas.FindAsync(id);
                if (tipo is null) return Results.NotFound();

                tipo.TipoDeDespesa = updatedTipo.TipoDeDespesa;
                await db.SaveChangesAsync();

                return Results.Ok(tipo);
            }).RequireAuthorization();

            app.MapDelete("/tiposdespesa/{id:decimal}", async (decimal id, KakeiboDBContext db) =>
            {
                var tipo = await db.tiposDeDespesas.FindAsync(id);
                if (tipo is null) return Results.NotFound();

                db.tiposDeDespesas.Remove(tipo);
                await db.SaveChangesAsync();

                return Results.NoContent();
            }).RequireAuthorization();
        }
    }
}
