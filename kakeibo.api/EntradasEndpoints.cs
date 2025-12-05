using kakeibo.api.Data;
using Microsoft.EntityFrameworkCore;

namespace kakeibo.api
{
    public static class EntradasEndpoints
    {
        public static void MapEntradasEndpoints(this WebApplication app)
        {
            app.MapGet("/entradas/{month}-{year}", async (string month, string year, KakeiboDBContext db) =>
            {
                DateTime dt = new DateTime(Convert.ToInt16(year), Convert.ToInt16(month), 1, 0, 0, 0);

                var all = await db.entradas.Where(w => w.Ano == dt.Year.ToString("0000") && w.Mes == dt.Month.ToString("00")).ToListAsync();

                return Results.Ok(all);
            }).RequireAuthorization();

            app.MapGet("/entradas/{id:decimal}", async (decimal id, KakeiboDBContext db) =>
            {
                var item = await db.entradas.FindAsync(id);
                return item is not null ? Results.Ok(item) : Results.NotFound();
            }).RequireAuthorization();

            app.MapPost("/entradas", async (Entradas entries, KakeiboDBContext db) =>
            {
                db.entradas.Add(entries);
                await db.SaveChangesAsync();
                return Results.Created($"/entradas/{entries.EntradaID}", entries);
            }).RequireAuthorization();

            app.MapPut("/entradas/{id:decimal}", async (decimal id, Entradas entries, KakeiboDBContext db) =>
            {
                var existing = await db.entradas.FindAsync(id);
                if (existing is null) return Results.NotFound();

                // Update properties (example, replace with your actual properties)
                existing.Ano = entries.Ano;
                existing.Mes = entries.Mes;
                existing.UserID = entries.UserID;
                existing.Descricao = entries.Descricao;
                existing.ValorEntrada = entries.ValorEntrada;

                await db.SaveChangesAsync();
                return Results.Ok(existing);
            }).RequireAuthorization();


            app.MapDelete("/entradas/{id:decimal}", async (decimal id, KakeiboDBContext db) =>
            {
                var r = await db.entradas.FindAsync(id);
                if (r is null) return Results.NotFound();

                db.entradas.Remove(r);
                await db.SaveChangesAsync();

                return Results.NoContent();
            }).RequireAuthorization();
        }
    }
}
