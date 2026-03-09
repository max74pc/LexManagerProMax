using LexManagerProMax.Application.Interfaces;
using LexManagerProMax.Domain.Entities.AI;
using LexManagerProMax.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LexManagerProMax.Infrastructure.Repositories;

public class SessioneAIRepository(LexManagerProMaxDbContext db) : ISessioneAIRepository
{
    public async Task<SessioneAI?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await db.SessioniAI
                   .Include(s => s.Messaggi)
                   .FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<SessioneAI?> GetByIdWithMessaggiAsync(
        Guid id, CancellationToken ct = default)
        => await db.SessioniAI
                   .Include(s => s.Messaggi.OrderBy(m => m.CreatedAt))
                   .FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<IReadOnlyList<SessioneAI>> GetAllAsync(CancellationToken ct = default)
        => await db.SessioniAI
                   .OrderByDescending(s => s.UpdatedAt)
                   .ToListAsync(ct);

    public async Task<IReadOnlyList<SessioneAI>> GetAttivaAsync(CancellationToken ct = default)
        => await db.SessioniAI
                   .Where(s => !s.IsDeleted)
                   .OrderByDescending(s => s.UpdatedAt)
                   .ToListAsync(ct);

    public async Task<IReadOnlyList<SessioneAI>> GetByTipoAsync(
        TipoRicercaAI tipo, CancellationToken ct = default)
        => await db.SessioniAI
                   .Where(s => s.TipoRicerca == tipo && !s.IsDeleted)
                   .OrderByDescending(s => s.UpdatedAt)
                   .ToListAsync(ct);

    public async Task<IReadOnlyList<SessioneAI>> SearchAsync(
        string query, CancellationToken ct = default)
    {
        var q = query.Trim().ToLower();
        return await db.SessioniAI
                       .Where(s => !s.IsDeleted &&
                                   (s.Titolo.ToLower().Contains(q) ||
                                    s.Messaggi.Any(m => m.Testo.ToLower().Contains(q))))
                       .OrderByDescending(s => s.UpdatedAt)
                       .Take(30)
                       .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<SessioneAI>> GetRecentiAsync(
        int count = 10, CancellationToken ct = default)
        => await db.SessioniAI
                   .Where(s => !s.IsDeleted)
                   .OrderByDescending(s => s.UpdatedAt)
                   .Take(count)
                   .ToListAsync(ct);

    public async Task<SessioneAI> AddAsync(SessioneAI entity, CancellationToken ct = default)
    {
        db.SessioniAI.Add(entity);
        await db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task UpdateAsync(SessioneAI entity, CancellationToken ct = default)
    {
        db.SessioniAI.Update(entity);
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

    public async Task AddMessaggioAsync(
        Guid sessioneId, MessaggioAI messaggio, CancellationToken ct = default)
    {
        var sessione = await GetByIdAsync(sessioneId, ct)
            ?? throw new InvalidOperationException(
                $"Sessione AI {sessioneId} non trovata.");

        // AggiungiMessaggio vuole (string ruolo, string contenuto)
        sessione.AggiungiMessaggio(messaggio.Contenuto, messaggio.Ruolo);
        await db.SaveChangesAsync(ct);
    }
}