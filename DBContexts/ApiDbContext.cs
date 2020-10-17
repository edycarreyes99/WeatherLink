using Microsoft.EntityFrameworkCore;
using WeatherLink.Models;

namespace WeatherLink.DBContexts
{
    public class ApiDbContext : DbContext
    {
        // Manejador de operaciones de las estaciones
        public DbSet<EstacionesViewModel> Estaciones { get; set; }

        // Constructor principal
        public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Se crea el modelo para la base de datos a partir del [ModelBuilder]. Igualmente se puede hacier mediante notaciones en el modelo de datos
            modelBuilder.Entity<EstacionesViewModel>().ToTable("Stations");
            modelBuilder.Entity<EstacionesViewModel>().Property(p => p.Name).IsRequired().HasColumnName("Name");
            modelBuilder.Entity<EstacionesViewModel>().Property(p => p.Longitude).IsRequired()
                .HasColumnName("Longitude");
            modelBuilder.Entity<EstacionesViewModel>().Property(p => p.Latitude).IsRequired().HasColumnName("Latitude");
        }
    }
}