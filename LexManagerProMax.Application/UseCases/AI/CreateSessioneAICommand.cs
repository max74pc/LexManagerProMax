using LexManagerProMax.Application.Interfaces;
using LexManagerProMax.Domain.Entities.AI;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LexManagerProMax.Application.UseCases.AI;

public record CreateSessioneAICommand(
    TipoRicercaAI Tipo,
    string? Titolo = null
) : IRequest<SessioneAI>;

public class CreateSessioneAICommandHandler(
    ISessioneAIRepository repo,
    ILogger<CreateSessioneAICommandHandler> logger)
    : IRequestHandler<CreateSessioneAICommand, SessioneAI>
{
    public async Task<SessioneAI> Handle(
        CreateSessioneAICommand cmd, CancellationToken ct)
    {
        var titolo = string.IsNullOrWhiteSpace(cmd.Titolo)
            ? $"Sessione {cmd.Tipo} — {DateTime.Now:dd/MM/yyyy HH:mm}"
            : cmd.Titolo.Trim();

        var sessione = SessioneAI.Crea(titolo, cmd.Tipo);
        var result = await repo.AddAsync(sessione, ct);

        logger.LogInformation(
            "Creata SessioneAI '{Titolo}' [{Id}] tipo={Tipo}",
            result.Titolo, result.Id, result.TipoRicerca);

        return result;
    }
}