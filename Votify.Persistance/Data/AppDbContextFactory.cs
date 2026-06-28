using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Persistance.Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            
            // Cadena de conexión real de PostgreSQL obtenida de appsettings.Development.json
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=VotifyDb;Username=postgres;Password=1234");

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
