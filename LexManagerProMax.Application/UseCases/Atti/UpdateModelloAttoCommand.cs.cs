using LexManagerProMax.Application.Interfaces;
using LexManagerProMax.Domain.Entities.Atti;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LexManagerProMax.Application.UseCases.Atti;

public record UpdateModelloAttoCommand(
    Guid Id,
    string Nome,
    CategoriaAtto Categoria,
    string Contenuto,
    string? Descrizione = null,
    string? Tags = null,
    string? NoteVersione = null
) : IRequest<ModelloAtto>;

public class UpdateModelloAttoCommandHandler(
    IModelloAttoRepository repo,
    ILogger<UpdateModelloAttoCommandHandler> logger)
    : IRequestHandler<UpdateModelloAttoCommand, ModelloAtto>
{
    public async Task<ModelloAtto> Handle(UpdateModelloAttoCommand cmd, CancellationToken ct)
    {
        var modello = await repo.GetByIdAsync(cmd.Id, ct)
            ?? throw new InvalidOperationException($"Modello {cmd.Id} non trovato.");

        // Controlla unicità nome (escluso se stesso)
        if (await repo.ExistsWithNameAsync(cmd.Nome, cmd.Id, ct))
            throw new InvalidOperationException($"Esiste già un altro modello con il nome '{cmd.Nome}'.");

        modello.AggiornaContenuto(cmd.Contenuto, cmd.NoteVersione);

        await repo.UpdateAsync(modello, ct);
        logger.LogInformation(
            "Aggiornato ModelloAtto '{Nome}' [{Id}] → v{Ver}",
            modello.Nome, modello.Id, modello.VersioneCorrente);
        return modello;
    }
}