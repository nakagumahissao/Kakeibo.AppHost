using kakeibo.api.Data;
using Microsoft.EntityFrameworkCore;

namespace kakeibo.api
{
    public static class TiposDeEntradasEndpoints
    {
        public static void MapTiposDeEntradasEndpoints(this WebApplication app)
        {
            app.MapGet("/tiposentrada", async (KakeiboDBContext db) =>
            {
                var all = await db.tiposDeEntradas.ToListAsync();
                return Results.Ok(all);
            }).RequireAuthorization();

            app.MapGet("/tiposentrada/{id}", async (int id, KakeiboDBContext db) =>
            {
                var item = await db.tiposDeEntradas.FindAsync(id);
                return item is not null ? Results.Ok(item) : Results.NotFound();
            }).RequireAuthorization();

            app.MapPost("/tiposentrada", async (TiposDeEntradas tipo, KakeiboDBContext db) =>
            {
                db.tiposDeEntradas.Add(tipo);
                await db.SaveChangesAsync();
                return Results.Created($"/tiposentrada/{tipo.TipoDeEntradaID}", tipo);
            }).RequireAuthorization();

            app.MapPut("/tiposentrada/{id}", async (int id, TiposDeEntradas updatedTipo, KakeiboDBContext db) =>
            {
                var tipo = await db.tiposDeEntradas.FindAsync(id);
                if (tipo is null) return Results.NotFound();

                tipo.TipoDeEntrada = updatedTipo.TipoDeEntrada;
                await db.SaveChangesAsync();

                return Results.Ok(tipo);
            }).RequireAuthorization();

            app.MapDelete("/tiposentrada/{id}", async (int id, KakeiboDBContext db) =>
            {
                var tipo = await db.tiposDeEntradas.FindAsync(id);
                if (tipo is null) return Results.NotFound();

                db.tiposDeEntradas.Remove(tipo);
                await db.SaveChangesAsync();

                return Results.NoContent();
            }).RequireAuthorization();
        }
    }
}
