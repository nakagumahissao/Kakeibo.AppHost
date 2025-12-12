using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // importante chamar isso para Identity

            // Exemplo para IDs - melhor trocar para long/int se possível
            modelBuilder.Entity<Despesas>()
                .Property(d => d.DespesaID)
                .HasColumnType("decimal(18,0)");

            modelBuilder.Entity<Entradas>()
                .Property(e => e.EntradaID)
                .HasColumnType("decimal(18,0)");

            modelBuilder.Entity<PlanoAnual>()
                .Property(e => e.PlanoAnualID)
                .HasColumnType("decimal(18,0)");

            modelBuilder.Entity<Resultados>()
                .Property(p => p.ResultadoID)
                .HasColumnType("decimal(18,0)");

            modelBuilder.Entity<Saidas>()
                .Property(r => r.SaidaID)
                .HasColumnType("decimal(18,0)");

            modelBuilder.Entity<TiposDeDespesa>()
                .Property(r => r.TipoDespesaID)
                .HasColumnType("decimal(18,0)");

            modelBuilder.Entity<TiposDeEntradas>()
                .Property(r => r.TipoDeEntradaID)
                .HasColumnType("decimal(18,0)");
        }
    }
}
