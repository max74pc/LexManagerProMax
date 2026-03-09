using LexManagerProMax.Application.Interfaces;
using MediatR;

namespace LexManagerProMax.Application.UseCases.Atti;

/// <summary>
/// Genera il testo finale sostituendo i segnaposto {{CHIAVE}} con i valori forniti.
/// Non salva nulla: restituisce il testo pronto per visualizzazione / esportazione.
/// </summary>
public record GeneraDocumentoDaModelloCommand(
    Guid Id,
    Dictionary<string, string> Segnaposto
) : IRequest<string>;

public class GeneraDocumentoDaModelloCommandHandler(IModelloAttoRepository repo)
    : IRequestHandler<GeneraDocumentoDaModelloCommand, string>
{
    public async Task<string> Handle(GeneraDocumentoDaModelloCommand cmd, CancellationToken ct)
    {
        var modello = await repo.GetByIdAsync(cmd.Id, ct)
            ?? throw new InvalidOperationException($"Modello {cmd.Id} non trovato.");

        return modello.GeneraDocumento(cmd.Segnaposto);
    }
}