using LexManagerProMax.Application.Interfaces;
using LexManagerProMax.Domain.Entities.Anagrafica;
using MediatR;


namespace LexManagerProMax.Application.UseCases.Anagrafica;

/// <summary>
/// Restituisce tutti i soggetti, con ricerca full-text opzionale.
/// </summary>
public record GetSoggettiQuery(
    string? SearchTerm = null
) : IRequest<IReadOnlyList<Soggetto>>;

public class GetSoggettiQueryHandler(ISoggettoRepository repo)
    : IRequestHandler<GetSoggettiQuery, IReadOnlyList<Soggetto>>
{
    public async Task<IReadOnlyList<Soggetto>> Handle(
        GetSoggettiQuery request,
        CancellationToken ct)
    {
        return string.IsNullOrWhiteSpace(request.SearchTerm)
            ? await repo.GetAllAsync(ct)
            : await repo.SearchAsync(request.SearchTerm, ct);
    }
}