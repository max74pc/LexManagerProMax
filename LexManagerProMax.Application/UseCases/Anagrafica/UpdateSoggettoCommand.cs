using LexManagerProMax.Application.Interfaces;
using LexManagerProMax.Domain.Entities.Anagrafica;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LexManagerProMax.Application.UseCases.Anagrafica;

/// <summary>
/// Aggiorna i dati modificabili di un Soggetto esistente.
/// I campi anagrafici immodificabili (nome, CF, P.IVA) non vengono toccati.
/// </summary>
public record UpdateSoggettoCommand(
    Guid Id,
    string? Email,
    string? Pec,
    string? Telefono,
    string? Cellulare,
    string? Via,
    string? Citta,
    string? Cap,
    string? Provincia,
    string? Note,
    bool IsCliente,
    bool IsControparte
) : IRequest<Soggetto>;

public class UpdateSoggettoCommandHandler(
    ISoggettoRepository repo,
    ILogger<UpdateSoggettoCommandHandler> logger)
    : IRequestHandler<UpdateSoggettoCommand, Soggetto>
{
    public async Task<Soggetto> Handle(
        UpdateSoggettoCommand cmd,
        CancellationToken ct)
    {
        var soggetto = await repo.GetByIdAsync(cmd.Id, ct)
            ?? throw new InvalidOperationException(
                $"Soggetto con Id={cmd.Id} non trovato.");

        soggetto.AggiornaDati(
            email: cmd.Email,
            pec: cmd.Pec,
            telefono: cmd.Telefono,
            cellulare: cmd.Cellulare,
            via: cmd.Via,
            citta: cmd.Citta,
            cap: cmd.Cap,
            provincia: cmd.Provincia,
            note: cmd.Note);

        soggetto.ImpostaRuolo(cmd.IsCliente, cmd.IsControparte);

        await repo.UpdateAsync(soggetto, ct);

        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation(
                "Aggiornato soggetto {NomeCompleto} [{Id}]",
                soggetto.NomeCompleto, soggetto.Id);

        return soggetto;
    }
}