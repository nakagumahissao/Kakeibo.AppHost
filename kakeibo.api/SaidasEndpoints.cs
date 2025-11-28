using kakeibo.api.Data;
using Microsoft.EntityFrameworkCore;

namespace kakeibo.api
{
    public static class SaidasEndpoints
    {
        public static void MapSaidasEndpoints(this WebApplication app)
        {
            app.MapGet("/saidas", async (KakeiboDBContext db) =>
            {
                var all = await db.saidas.ToListAsync();
                return Results.Ok(all);
            });

            app.MapGet("/saidas/{id}", async (int id, KakeiboDBContext db) =>
            {
                var item = await db.saidas.FindAsync(id);
                return item is not null ? Results.Ok(item) : Results.NotFound();
            });

            app.MapGet("/saidas/{dia}-{mes}-{ano}", async (int dia, int mes, int ano, KakeiboDBContext db) =>
            {
                DateTime dt = new DateTime(ano, mes, dia, 0, 0, 0);
                var item = await db.saidas.Where(w => w.DataSaida == dt).ToListAsync();
                return item is not null ? Results.Ok(item) : Results.NotFound();
            });

            app.MapPost("/saidas", async (Saidas saida, KakeiboDBContext db) =>
            {
                db.saidas.Add(saida);
                await db.SaveChangesAsync();
                return Results.Created($"/saidas/{saida.SaidaID}", saida);
            });

            app.MapPut("/saidas/{id}", async (int id, Saidas saida, KakeiboDBContext db) =>
            {
                var existing = await db.saidas.FindAsync(id);
                if (existing is null) return Results.NotFound();

                // Update properties (example, replace with your actual properties)
                existing.DataSaida = saida.DataSaida;
                existing.Ano = saida.Ano;
                existing.Mes = saida.Mes;
                existing.UserID = saida.UserID;
                existing.DespesaID = saida.DespesaID;
                existing.Descricao = saida.Descricao;
                existing.NomeDespesa = saida.NomeDespesa;
                existing.ValorDespesa = saida.ValorDespesa;

                await db.SaveChangesAsync();
                return Results.Ok(existing);
            });


            app.MapDelete("/saidas/{id}", async (int id, KakeiboDBContext db) =>
            {
                var s = await db.saidas.FindAsync(id);
                if (s is null) return Results.NotFound();

                db.saidas.Remove(s);
                await db.SaveChangesAsync();

                return Results.NoContent();
            });
        }
    }
}
