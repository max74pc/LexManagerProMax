using LexManagerProMax.Domain.Entities.Atti;

namespace LexManagerProMax.Application.Interfaces;

public interface IModelloAttoRepository : IRepository<ModelloAtto>
{
    /// <summary>Tutti i modelli attivi, ordinati per categoria e nome.</summary>
    Task<IReadOnlyList<ModelloAtto>> GetAttiviAsync(CancellationToken ct = default);

    /// <summary>Filtra per categoria.</summary>
    Task<IReadOnlyList<ModelloAtto>> GetByCategoriaAsync(CategoriaAtto categoria, CancellationToken ct = default);

    /// <summary>Ricerca full-text su nome, descrizione e tags.</summary>
    Task<IReadOnlyList<ModelloAtto>> SearchAsync(string query, CancellationToken ct = default);

    /// <summary>Restituisce tutte le versioni storiche di un modello.</summary>
    Task<IReadOnlyList<VersioneModello>> GetVersioniAsync(Guid modelloId, CancellationToken ct = default);

    /// <summary>Verifica se esiste già un modello con lo stesso nome (escludendo l'id fornito).</summary>
    Task<bool> ExistsWithNameAsync(string nome, Guid? excludeId = null, CancellationToken ct = default);
}
