namespace LexManagerProMax.Application.Interfaces;

/// <summary>
/// Astrazione per il servizio AI (OpenAI GPT-4o).
/// Usato dai command handler — non dipende da infrastruttura.
/// </summary>
public interface IAIService
{
    /// <summary>
    /// Streaming: restituisce i chunk di testo man mano che arrivano.
    /// Ogni elemento è un pezzo della risposta (delta).
    /// </summary>
    IAsyncEnumerable<string> StreamChatAsync(
        IEnumerable<(string Role, string Content)> messages,
        string systemPrompt,
        CancellationToken ct = default);

    /// <summary>
    /// Sincrono (await): restituisce la risposta completa in una volta.
    /// </summary>
    Task<string> ChatAsync(
        IEnumerable<(string Role, string Content)> messages,
        string systemPrompt,
        CancellationToken ct = default);
}

