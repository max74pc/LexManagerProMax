using LexManagerProMax.Application.Interfaces;
using LexManagerProMax.Domain.Entities.Atti;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LexManagerProMax.Application.UseCases.Atti;

public record CreateModelloAttoCommand(
    string Nome,
    CategoriaAtto Categoria,
    string Contenuto,
    string? Descrizione = null,
    string? Tags = null
,
    Domain.Entities.Anagrafica.TipoSoggetto Tipo = default) : IRequest<ModelloAtto>;

public class CreateModelloAttoCommandHandler(
    IModelloAttoRepository repo,
    ILogger<CreateModelloAttoCommandHandler> logger)
    : IRequestHandler<CreateModelloAttoCommand, ModelloAtto>
{
    public async Task<ModelloAtto> Handle(CreateModelloAttoCommand cmd, CancellationToken ct)
    {
        // Unicità del nome
        if (await repo.ExistsWithNameAsync(cmd.Nome, null, ct))
            throw new InvalidOperationException($"Esiste già un modello con il nome '{cmd.Nome}'.");

        var modello = ModelloAtto.Crea(cmd.Nome, cmd.Categoria, cmd.Contenuto, cmd.Descrizione);

        var result = await repo.AddAsync(modello, ct);
        logger.LogInformation("Creato ModelloAtto '{Nome}' [{Id}]", result.Nome, result.Id);
        return result;
    }
}
