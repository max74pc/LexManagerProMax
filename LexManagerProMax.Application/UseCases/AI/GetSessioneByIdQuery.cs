using LexManagerProMax.Application.Interfaces;
using LexManagerProMax.Domain.Entities.AI;
using MediatR;

namespace LexManagerProMax.Application.UseCases.AI;

public record GetSessioneByIdQuery(Guid Id) : IRequest<SessioneAI?>;

public class GetSessioneByIdQueryHandler(ISessioneAIRepository repo)
    : IRequestHandler<GetSessioneByIdQuery, SessioneAI?>
{
    public async Task<SessioneAI?> Handle(
        GetSessioneByIdQuery request, CancellationToken ct)
        => await repo.GetByIdWithMessaggiAsync(request.Id, ct);
}