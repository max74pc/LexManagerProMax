using LexManagerProMax.Domain.Entities.AI;
using LexManagerProMax.Domain.Entities.Anagrafica;
using LexManagerProMax.Domain.Entities.Atti;
using Microsoft.EntityFrameworkCore;

namespace LexManagerProMax.Infrastructure.Persistence;

public class LexManagerProMaxDbContext(DbContextOptions<LexManagerProMaxDbContext> options)
    : DbContext(options)
{
    // Anagrafica
    public DbSet<Soggetto> Soggetti => Set<Soggetto>();
    public DbSet<Fascicolo> Fascicoli => Set<Fascicolo>();
    public DbSet<FascicoloSoggetto> FascicoloSoggetti => Set<FascicoloSoggetto>();

    // Atti
    public DbSet<ModelloAtto> ModelliAtti => Set<ModelloAtto>();
    public DbSet<VersioneModello> VersioniModello => Set<VersioneModello>();

    // AI
    public DbSet<SessioneAI> SessioniAI => Set<SessioneAI>();
    public DbSet<MessaggioAI> MessaggiAI => Set<MessaggioAI>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Applica tutte le configurazioni dalla stessa assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LexManagerProMaxDbContext).Assembly);

        // Soft delete global filter
        modelBuilder.Entity<Soggetto>().HasQueryFilter(s => !s.IsDeleted);
        modelBuilder.Entity<Fascicolo>().HasQueryFilter(f => !f.IsDeleted);
        modelBuilder.Entity<ModelloAtto>().HasQueryFilter(m => !m.IsDeleted);
    }
}
