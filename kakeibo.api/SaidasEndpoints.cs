using kakeibo.api.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace kakeibo.api
{
    public static class SaidasEndpoints
    {
        public static void MapSaidasEndpoints(this WebApplication app)
        {
            app.MapGet("/saidas/{id:decimal}", async (decimal id, KakeiboDBContext db) =>
            {
                var item = await db.saidas.Where(s => s.SaidaID == id).FirstOrDefaultAsync();
                return item is not null ? Results.Ok(item) : Results.NotFound();
            }).RequireAuthorization();

            app.MapGet("/saidas/{UserID}/{dia:int}-{mes:int}-{ano:int}", async (string UserID, int dia, int mes, int ano, KakeiboDBContext db) =>
            {
                DateTime dt = new DateTime(ano, mes, dia, 0, 0, 0);
                var item = await db.saidas.Where(w => w.DataSaida == dt && w.UserID == UserID).ToListAsync();
                return item is not null ? Results.Ok(item) : Results.NotFound();
            }).RequireAuthorization();

            app.MapPost("/saidas", async (Saidas saida, KakeiboDBContext db) =>
            {
                db.saidas.Add(saida);
                await db.SaveChangesAsync();
                return Results.Created($"/saidas/{saida.SaidaID}", saida);
            }).RequireAuthorization();

            app.MapPut("/saidas/{id:decimal}", async (decimal id, Saidas saida, KakeiboDBContext db) =>
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
            }).RequireAuthorization();


            app.MapDelete("/saidas/{id:decimal}", async (decimal id, KakeiboDBContext db, ILogger<Program> Log) =>
            {
                var s = await db.saidas.Where(s => s.SaidaID == id).FirstOrDefaultAsync();
                if (s is null) return Results.NotFound();

                db.saidas.Remove(s);

                try
                {
                    await db.SaveChangesAsync();
                }
                catch (DbUpdateException ex)
                {
                    Log.LogError($"Error deleting Saida with ID {id}: {ex.Message}");
                }

                return Results.NoContent();
            }).RequireAuthorization();
        }
    }
}
