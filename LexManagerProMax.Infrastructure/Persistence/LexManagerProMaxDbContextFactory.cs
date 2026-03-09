using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace LexManagerProMax.Infrastructure.Persistence
{
    public class LexManagerProMaxDbContextFactory : IDesignTimeDbContextFactory<LexManagerProMaxDbContext>
    {
        public LexManagerProMaxDbContext CreateDbContext(string[] args)
        {
            // Individua la directory del progetto API (o del progetto che contiene appsettings.json)
            var basePath = Directory.GetCurrentDirectory();

            // Costruisce la configurazione leggendo appsettings.json
            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            var optionsBuilder = new DbContextOptionsBuilder<LexManagerProMaxDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new LexManagerProMaxDbContext(optionsBuilder.Options);
        }
    }
}
