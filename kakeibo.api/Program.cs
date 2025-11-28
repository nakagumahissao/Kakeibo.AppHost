
using Microsoft.EntityFrameworkCore;
using kakeibo.api.Data;

namespace kakeibo.api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddServiceDefaults();

        // Retrieve the connection string from configuration
        var connectionString = builder.Configuration.GetConnectionString("DatabaseConnection");

        // Example: register a DbContext
        builder.Services.AddDbContext<KakeiboDBContext>(options =>
            options.UseSqlServer(connectionString));

        // Add services to the container.
        builder.Services.AddAuthorization();

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        var app = builder.Build();

        app.MapDefaultEndpoints();

        app.MapTiposDeDespesaEndpoints();
        app.MapTiposDeEntradasEndpoints();
        app.MapSaidasEndpoints();
        app.MapResultadosEndpoints();
        app.MapPlanoAnualEndpoints();
        app.MapEntradasEndpoints();
        app.MapDespesasEndpoints();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();        

        app.Run();
    }
}
