using LexManagerProMax.Application.Interfaces;
using LexManagerProMax.Domain.Entities.AI;
using MediatR;

namespace LexManagerProMax.Application.UseCases.AI;

/// <summary>
/// Persiste la risposta completa dell'AI al termine dello streaming.
/// Da chiamare nel ViewModel dopo aver accumulato tutti i chunk.
/// </summary>
public record SaveRispostaAICommand(
    Guid SessioneId,
    string TestoRisposta
) : IRequest;

public class SaveRispostaAICommandHandler(ISessioneAIRepository repo)
    : IRequestHandler<SaveRispostaAICommand>
{
    public async Task Handle(SaveRispostaAICommand cmd, CancellationToken ct)
    {
        var sessione = await repo.GetByIdAsync(cmd.SessioneId, ct)
            ?? throw new InvalidOperationException(
                $"Sessione AI {cmd.SessioneId} non trovata.");

        // AggiungiMessaggio è il domain method reale di SessioneAI
        sessione.AggiungiMessaggio("assistant", cmd.TestoRisposta);
        await repo.UpdateAsync(sessione, ct);
    }
}
