using kakeibo.api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Ykakeibo.api.Models;

namespace kakeibo.api.Data
{
    public class KakeiboDBContext : IdentityDbContext<IdentityUser>
    {
        public KakeiboDBContext(DbContextOptions<KakeiboDBContext> options) : base(options)
        {
        }

        public DbSet<TiposDeDespesa> tiposDeDespesas { get; set; }
        public DbSet<TiposDeEntradas> tiposDeEntradas { get; set; }
        public DbSet<Despesas> despesas { get; set; }
        public DbSet<Entradas> entradas { get; set; }
        public DbSet<PlanoAnual> planoAnuals { get; set; }
        public DbSet<Resultados> resultados { get; set; }
        public DbSet<Saidas> saidas { get; set; }
        public DbSet<DailyExpensesTotals> dailyExpensesTotals { get; set; }
        public DbSet<MonthlyExpensesTotals> monthlyExpensesTotals { get; set; }
        public DbSet<OwnedMoney> ownedMoneys { get; set; }
        public DbSet<DespesasVariaveis> despesasVariaveis { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // importante chamar isso para Identity

            // Exemplo para IDs - melhor trocar para long/int se possível
            modelBuilder.Entity<Despesas>()
                .Property(d => d.DespesaID)
                .HasColumnType("decimal(18,0)");

            modelBuilder.Entity<Entradas>()
                .ToTable(tb => tb.HasTrigger("trg_RecalcOwnedMoney_FromEntradas"))
                .Property(e => e.EntradaID)
                .HasColumnType("decimal(18,0)");

            modelBuilder.Entity<PlanoAnual>()
                .Property(e => e.PlanoAnualID)
                .HasColumnType("decimal(18,0)");

            modelBuilder.Entity<Resultados>()
                .Property(p => p.ResultadoID)
                .HasColumnType("decimal(18,0)");

            modelBuilder.Entity<Saidas>()
                .ToTable(tb => tb.HasTrigger("trg_RecalcDailyAndMonthlyExpenses"))
                .Property(r => r.SaidaID)
                .HasColumnType("decimal(18,0)");        

            modelBuilder.Entity<TiposDeDespesa>()
                .Property(r => r.TipoDespesaID)
                .HasColumnType("decimal(18,0)");

            modelBuilder.Entity<TiposDeEntradas>()
                .Property(r => r.TipoDeEntradaID)
                .HasColumnType("decimal(18,0)");

            modelBuilder.Entity<DailyExpensesTotals>()
                .Property(r => r.DailyRecordID)
                .HasColumnType("decimal(18,0)");

            modelBuilder.Entity<MonthlyExpensesTotals>()
                .Property(r => r.MonthlyRecordID)
                .HasColumnType("decimal(18,0)");

            modelBuilder.Entity<OwnedMoney>()
                .Property(r => r.OwnedMoneyID)
                .HasColumnType("decimal(18,0)");

            modelBuilder.Entity<DespesasVariaveis>(entity =>
            {
                // Define a Chave Primária Composta
                entity.HasKey(e => new { e.UserID, e.Ano, e.Mes });

                // Mapeia para a View (garante que o EF não tente criar uma tabela)
                entity.ToView("vwDespesasVariaveis");

                entity.Property(e => e.TotalDestaCategoria)
                      .HasPrecision(19, 2);
            });
        }
    }
}
