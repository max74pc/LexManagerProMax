using LexManagerProMax.Application.Interfaces;
using LexManagerProMax.Domain.Entities.Atti;
using LexManagerProMax.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LexManagerProMax.Infrastructure.Repositories;

public class ModelloAttoRepository(LexManagerProMaxDbContext db) : IModelloAttoRepository
{
    public async Task<ModelloAtto?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await db.ModelliAtti
                   .Include(m => m.Versioni)
                   .FirstOrDefaultAsync(m => m.Id == id, ct);

    public async Task<IReadOnlyList<ModelloAtto>> GetAllAsync(CancellationToken ct = default)
        => await db.ModelliAtti
                   .OrderBy(m => m.Categoria)
                   .ThenBy(m => m.Nome)
                   .ToListAsync(ct);

    public async Task<IReadOnlyList<ModelloAtto>> GetAttiviAsync(CancellationToken ct = default)
        => await db.ModelliAtti
                   .Where(m => m.IsAttivo)
                   .OrderBy(m => m.Categoria)
                   .ThenBy(m => m.Nome)
                   .ToListAsync(ct);

    public async Task<IReadOnlyList<ModelloAtto>> GetByCategoriaAsync(
        CategoriaAtto categoria, CancellationToken ct = default)
        => await db.ModelliAtti
                   .Where(m => m.Categoria == categoria && m.IsAttivo)
                   .OrderBy(m => m.Nome)
                   .ToListAsync(ct);

    public async Task<IReadOnlyList<ModelloAtto>> SearchAsync(string query, CancellationToken ct = default)
    {
        var q = query.Trim().ToLower();
        return await db.ModelliAtti
                       .Where(m => m.IsAttivo &&
                                   (m.Nome.ToLower().Contains(q) ||
                                    (m.Descrizione != null && m.Descrizione.ToLower().Contains(q)) ||
                                    (m.Tags != null && m.Tags.ToLower().Contains(q))))
                       .OrderBy(m => m.Nome)
                       .Take(50)
                       .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<VersioneModello>> GetVersioniAsync(
        Guid modelloId, CancellationToken ct = default)
        => await db.VersioniModello
                   .Where(v => v.ModelloAttoId == modelloId)
                   .OrderByDescending(v => v.Versione)
                   .ToListAsync(ct);

    public async Task<bool> ExistsWithNameAsync(
        string nome, Guid? excludeId = null, CancellationToken ct = default)
        => await db.ModelliAtti
                   .AnyAsync(m => m.Nome == nome &&
                                  (excludeId == null || m.Id != excludeId), ct);

    public async Task<ModelloAtto> AddAsync(ModelloAtto entity, CancellationToken ct = default)
    {
        db.ModelliAtti.Add(entity);
        await db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task UpdateAsync(ModelloAtto entity, CancellationToken ct = default)
    {
        db.ModelliAtti.Update(entity);
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

