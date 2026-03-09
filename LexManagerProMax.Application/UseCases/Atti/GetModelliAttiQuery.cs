using LexManagerProMax.Application.Interfaces;
using LexManagerProMax.Domain.Entities.Atti;
using MediatR;

namespace LexManagerProMax.Application.UseCases.Atti;

// ── Carica tutti i modelli (con filtro opzionale) ──────────────────
public record GetModelliAttiQuery(
    string? SearchTerm = null,
    CategoriaAtto? Categoria = null
) : IRequest<IReadOnlyList<ModelloAtto>>;

public class GetModelliAttiQueryHandler(IModelloAttoRepository repo)
    : IRequestHandler<GetModelliAttiQuery, IReadOnlyList<ModelloAtto>>
{
    public async Task<IReadOnlyList<ModelloAtto>> Handle(
        GetModelliAttiQuery request, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            return await repo.SearchAsync(request.SearchTerm, ct);

        if (request.Categoria.HasValue)
            return await repo.GetByCategoriaAsync(request.Categoria.Value, ct);

        return await repo.GetAttiviAsync(ct);
    }
}
