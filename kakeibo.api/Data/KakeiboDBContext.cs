using Microsoft.EntityFrameworkCore;

namespace kakeibo.api.Data
{
    public class KakeiboDBContext : DbContext
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

    }
}
