using LexManagerProMax.Application.Interfaces;
using LexManagerProMax.Domain.Entities.AI;
using MediatR;

namespace LexManagerProMax.Application.UseCases.AI;

public record GetSessioniAIQuery(
    string? SearchTerm = null,
    TipoRicercaAI? Tipo = null,
    int Recenti = 0      // se > 0 restituisce solo le ultime N
) : IRequest<IReadOnlyList<SessioneAI>>;

public class GetSessioniAIQueryHandler(ISessioneAIRepository repo)
    : IRequestHandler<GetSessioniAIQuery, IReadOnlyList<SessioneAI>>
{
    public async Task<IReadOnlyList<SessioneAI>> Handle(
        GetSessioniAIQuery request, CancellationToken ct)
    {
        if (request.Recenti > 0)
            return await repo.GetRecentiAsync(request.Recenti, ct);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            return await repo.SearchAsync(request.SearchTerm, ct);

        if (request.Tipo.HasValue)
            return await repo.GetByTipoAsync(request.Tipo.Value, ct);

        return await repo.GetAttivaAsync(ct);
    }
}
