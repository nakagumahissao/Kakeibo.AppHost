using kakeibo.api.Data;
using Microsoft.EntityFrameworkCore;

namespace kakeibo.api
{
    public static class PlanoAnualEndpoints
    {
        public static void MapPlanoAnualEndpoints(this WebApplication app)
        {
            app.MapGet("/panual/{UserID}", async (string UserID, KakeiboDBContext db) =>
            {
                var all = await db.planoAnuals.Where(w => w.UserID == UserID).ToListAsync();
                return Results.Ok(all);
            }).RequireAuthorization();

            app.MapGet("/panual/{id:decimal}", async (decimal id, KakeiboDBContext db) =>
            {
                var item = await db.planoAnuals.FindAsync(id);
                return item is not null ? Results.Ok(item) : Results.NotFound();
            }).RequireAuthorization();

            app.MapPost("/panual", async (PlanoAnual pa, KakeiboDBContext db) =>
            {
                db.planoAnuals.Add(pa);
                await db.SaveChangesAsync();
                return Results.Created($"/panual/{pa.PlanoAnualID}", pa);
            }).RequireAuthorization();

            app.MapPut("/panual/{id:decimal}", async (decimal id, PlanoAnual pa, KakeiboDBContext db) =>
            {
                var existing = await db.planoAnuals.FindAsync(id);
                if (existing is null) return Results.NotFound();

                // Update properties (example, replace with your actual properties)
                existing.Ano = pa.Ano;
                existing.Mes = pa.Mes;
                existing.UserID = pa.UserID;
                existing.Objetivo = pa.Objetivo;
                existing.MetaEmValor = pa.MetaEmValor;
                existing.Observacoes = pa.Observacoes;
                existing.MetaAtingida = pa.MetaAtingida;

                await db.SaveChangesAsync();
                return Results.Ok(existing);
            }).RequireAuthorization();


            app.MapDelete("/panual/{id:decimal}", async (decimal id, KakeiboDBContext db) =>
            {
                var r = await db.planoAnuals.FindAsync(id);
                if (r is null) return Results.NotFound();

                db.planoAnuals.Remove(r);
                await db.SaveChangesAsync();

                return Results.NoContent();
            }).RequireAuthorization();
        }
    }
}
