using LexManagerProMax.Application.Interfaces;
using LexManagerProMax.Domain.Entities.Anagrafica;
using LexManagerProMax.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LexManagerProMax.Infrastructure.Repositories;

public class SoggettoRepository(LexManagerProMaxDbContext db) : ISoggettoRepository
{
    public async Task<Soggetto?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await db.Soggetti.FindAsync([id], ct);

    public async Task<IReadOnlyList<Soggetto>> GetAllAsync(CancellationToken ct = default)
        => await db.Soggetti.OrderBy(s => s.Cognome).ThenBy(s => s.Nome).ToListAsync(ct);

    public async Task<IReadOnlyList<Soggetto>> SearchAsync(string query, CancellationToken ct = default)
    {
        query = query.Trim().ToLower();
        return await db.Soggetti
            .Where(s => (s.Cognome != null && s.Cognome.Contains(query, StringComparison.CurrentCultureIgnoreCase)) ||
                        (s.Nome != null && s.Nome.Contains(query, StringComparison.CurrentCultureIgnoreCase)) ||
                        (s.RagioneSociale != null && s.RagioneSociale.Contains(query, StringComparison.CurrentCultureIgnoreCase)) ||
                        (s.CodiceFiscale != null && s.CodiceFiscale.Contains(query, StringComparison.CurrentCultureIgnoreCase)))
            .OrderBy(s => s.Cognome)
            .Take(50)
            .ToListAsync(ct);
    }

    public async Task<Soggetto?> GetByCodiceFiscaleAsync(string cf, CancellationToken ct = default)
        => await db.Soggetti.FirstOrDefaultAsync(s => s.CodiceFiscale != null && s.CodiceFiscale.Equals(cf, StringComparison.InvariantCultureIgnoreCase), ct);

    public async Task<IReadOnlyList<Soggetto>> GetClientiAsync(CancellationToken ct = default)
        => await db.Soggetti.Where(s => s.IsCliente).OrderBy(s => s.Cognome).ToListAsync(ct);

    public async Task<Soggetto> AddAsync(Soggetto entity, CancellationToken ct = default)
    {
        db.Soggetti.Add(entity);
        await db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task UpdateAsync(Soggetto entity, CancellationToken ct = default)
    {
        db.Soggetti.Update(entity);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await GetByIdAsync(id, ct);
        if (entity is not null)
        {
            entity.MarkAsDeleted();
            await db.SaveChangesAsync(ct);
        }
    }
}
