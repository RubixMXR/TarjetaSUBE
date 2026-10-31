using Microsoft.EntityFrameworkCore;

namespace TarjetaSUBE;

public class SubeDbContext : DbContext
{
    public SubeDbContext() { }

    public SubeDbContext(DbContextOptions<SubeDbContext> opciones)
        : base(opciones) { }

    public DbSet<Tarjeta> Tarjetas => Set<Tarjeta>();
    public DbSet<Colectivo> Colectivos => Set<Colectivo>();
    public DbSet<Boleto> Boletos => Set<Boleto>();
    public DbSet<TarjetaTipo> TarjetaTipos => Set<TarjetaTipo>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=Sube.db");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tarjeta>()
            .Property(t => t.Saldo)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Colectivo>()
            .Property(c => c.Tarifa)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Boleto>()
            .Property(b => b.Monto)
            .HasPrecision(18, 2);

        modelBuilder.Entity<TarjetaTipo>()
            .Property(t => t.Coeficiente)
            .HasPrecision(18, 2);

        modelBuilder.Entity<TarjetaTipo>().HasData(
            new TarjetaTipo { Id = 1, Nombre = "Normal", Coeficiente = 1m },
            new TarjetaTipo { Id = 2, Nombre = "MedioBoletoEstudiantil", Coeficiente = 0.5m },
            new TarjetaTipo { Id = 3, Nombre = "BoletoGratuitoEstudiantil", Coeficiente = 0m },
            new TarjetaTipo { Id = 4, Nombre = "FranquiciaCompleta", Coeficiente = 0m }
        );
    }
}