using LexManagerProMax.Application.Interfaces;
using LexManagerProMax.Domain.Entities.AI;
using MediatR;

namespace LexManagerProMax.Application.UseCases.AI;

public record RinominaSessioneAICommand(Guid Id, string NuovoTitolo) : IRequest<SessioneAI>;

public class RinominaSessioneAICommandHandler(ISessioneAIRepository repo)
    : IRequestHandler<RinominaSessioneAICommand, SessioneAI>
{
    public async Task<SessioneAI> Handle(
        RinominaSessioneAICommand cmd, CancellationToken ct)
    {
        var sessione = await repo.GetByIdAsync(cmd.Id, ct)
            ?? throw new InvalidOperationException(
                $"Sessione AI {cmd.Id} non trovata.");

        sessione.Rinomina(cmd.NuovoTitolo.Trim());
        await repo.UpdateAsync(sessione, ct);
        return sessione;
    }
}