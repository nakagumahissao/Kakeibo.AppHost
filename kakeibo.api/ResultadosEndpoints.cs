using kakeibo.api.Data;
using Microsoft.EntityFrameworkCore;

namespace kakeibo.api
{
    public static class ResultadosEndpoints
    {
        public static void MapResultadosEndpoints(this WebApplication app)
        {
            app.MapGet("/resultados", async (KakeiboDBContext db) =>
            {
                var all = await db.resultados.ToListAsync();
                return Results.Ok(all);
            }).RequireAuthorization();

            app.MapGet("/resultados/{id}", async (int id, KakeiboDBContext db) =>
            {
                var item = await db.saidas.FindAsync(id);
                return item is not null ? Results.Ok(item) : Results.NotFound();
            }).RequireAuthorization();

            app.MapPost("/resultados", async (Resultados results, KakeiboDBContext db) =>
            {
                db.resultados.Add(results);
                await db.SaveChangesAsync();
                return Results.Created($"/resultados/{results.ResultadoID}", results);
            }).RequireAuthorization();

            app.MapPut("/resultados/{id}", async (int id, Resultados results, KakeiboDBContext db) =>
            {
                var existing = await db.resultados.FindAsync(id);
                if (existing is null) return Results.NotFound();

                // Update properties (example, replace with your actual properties)
                existing.Ano = results.Ano;
                existing.Mes = results.Mes;
                existing.UserID = results.UserID;
                existing.A_TotalEntradas = results.A_TotalEntradas;
                existing.B_TotalDespesasFixas = results.B_TotalDespesasFixas;
                existing.C_SubTotalA_B = results.C_SubTotalA_B;
                existing.D_TotalDespesasVariaveis = results.D_TotalDespesasVariaveis;
                existing.E_SubTotal_B_D = results.E_SubTotal_B_D;
                existing.ResultadoParaProxMes_A_E = results.ResultadoParaProxMes_A_E;

                await db.SaveChangesAsync();
                return Results.Ok(existing);
            }).RequireAuthorization();


            app.MapDelete("/saidas/{id}", async (int id, KakeiboDBContext db) =>
            {
                var r = await db.resultados.FindAsync(id);
                if (r is null) return Results.NotFound();

                db.resultados.Remove(r);
                await db.SaveChangesAsync();

                return Results.NoContent();
            }).RequireAuthorization();
        }
    }
}
