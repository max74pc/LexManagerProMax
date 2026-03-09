using LexManagerProMax.Domain.Entities.AI;

namespace LexManagerProMax.Application.Interfaces;

public interface ISessioneAIRepository : IRepository<SessioneAI>
{
    /// <summary>Tutte le sessioni attive, ordinate per data decrescente.</summary>
    Task<IReadOnlyList<SessioneAI>> GetAttivaAsync(CancellationToken ct = default);

    /// <summary>Sessioni filtrate per tipo di ricerca.</summary>
    Task<IReadOnlyList<SessioneAI>> GetByTipoAsync(
        TipoRicercaAI tipo, CancellationToken ct = default);

    /// <summary>Ricerca full-text su titolo e messaggi.</summary>
    Task<IReadOnlyList<SessioneAI>> SearchAsync(
        string query, CancellationToken ct = default);

    /// <summary>Sessione con tutti i messaggi inclusi (eager load).</summary>
    Task<SessioneAI?> GetByIdWithMessaggiAsync(
        Guid id, CancellationToken ct = default);

    /// <summary>Aggiunge un singolo messaggio a una sessione esistente.</summary>
    Task AddMessaggioAsync(
        Guid sessioneId, MessaggioAI messaggio, CancellationToken ct = default);

    /// <summary>Ultime N sessioni usate di recente.</summary>
    Task<IReadOnlyList<SessioneAI>> GetRecentiAsync(
        int count = 10, CancellationToken ct = default);
}