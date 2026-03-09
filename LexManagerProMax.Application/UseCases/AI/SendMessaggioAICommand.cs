using LexManagerProMax.Application.Interfaces;
using LexManagerProMax.Domain.Entities.AI;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LexManagerProMax.Application.UseCases.AI;

/// <summary>
/// Invia un messaggio utente, chiama il servizio AI e restituisce
/// l'IAsyncEnumerable per lo streaming della risposta.
/// Persiste sia il messaggio utente che la risposta completa.
/// </summary>
public record SendMessaggioAICommand(
    Guid SessioneId,
    string TestoUtente,
    TipoRicercaAI Tipo
) : IRequest<IAsyncEnumerable<string>>;

public class SendMessaggioAICommandHandler(
    ISessioneAIRepository repo,
    IAIService aiService,
    ILogger<SendMessaggioAICommandHandler> logger)
    : IRequestHandler<SendMessaggioAICommand, IAsyncEnumerable<string>>
{
    public async Task<IAsyncEnumerable<string>> Handle(
        SendMessaggioAICommand cmd, CancellationToken ct)
    {
        var sessione = await repo.GetByIdWithMessaggiAsync(cmd.SessioneId, ct)
            ?? throw new InvalidOperationException(
                $"Sessione AI {cmd.SessioneId} non trovata.");

        // Persisti messaggio utente tramite domain method
        sessione.AggiungiMessaggio("user", cmd.TestoUtente);
        await repo.UpdateAsync(sessione, ct);

        // Costruisci cronologia — usa Ruolo e Contenuto (proprietà reali di MessaggioAI)
        var cronologia = sessione.Messaggi
            .OrderBy(m => m.Ordine)
            .Select(m => (
                Role: m.Ruolo,
                Content: m.Contenuto))
            .ToList();

        var systemPrompt = BuildSystemPrompt(cmd.Tipo);

        logger.LogInformation(
            "Invio messaggio AI sessione [{Id}] tipo={Tipo}",
            sessione.Id, cmd.Tipo);

        // Firma IAIService: StreamChatAsync(IEnumerable<(string,string)>, string, CancellationToken)
        return aiService.StreamChatAsync(cronologia, systemPrompt, ct);
    }

    private static string BuildSystemPrompt(TipoRicercaAI tipo) => tipo switch
    {
        TipoRicercaAI.Giurisprudenza =>
            "Sei un assistente legale specializzato in ricerca giurisprudenziale italiana. " +
            "Analizza il caso e cita le sentenze più rilevanti con estremi precisi (Corte, sezione, data, numero).",

        TipoRicercaAI.Dottrina =>
            "Sei un assistente legale specializzato in dottrina giuridica italiana. " +
            "Esponi le teorie prevalenti citando autori e opere di riferimento.",

        TipoRicercaAI.Normativa =>
            "Sei un assistente legale specializzato in normativa italiana ed europea. " +
            "Indica gli articoli di legge applicabili con riferimenti precisi (legge, articolo, comma).",

        _ => // TipoRicercaAI.Assistenza e altri
            "Sei un assistente legale per avvocati italiani. " +
            "Rispondi in modo preciso, professionale e conciso. " +
            "Usa la terminologia giuridica italiana corretta."
    };
}