using LexManagerProMax.Domain.Entities.Anagrafica;
using LexManagerProMax.Application.Interfaces;

namespace LexManagerProMax.Application.Interfaces;

public interface ISoggettoRepository : IRepository<Soggetto>
{
    Task<IReadOnlyList<Soggetto>> SearchAsync(
        string query,
        CancellationToken ct = default);

    Task<Soggetto?> GetByCodiceFiscaleAsync(
        string cf,
        CancellationToken ct = default);

    Task<IReadOnlyList<Soggetto>> GetClientiAsync(
        CancellationToken ct = default);
}
