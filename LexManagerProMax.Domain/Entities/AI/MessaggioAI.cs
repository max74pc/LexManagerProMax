using LexManagerProMax.Domain.Common;

namespace LexManagerProMax.Domain.Entities.AI;

/// <summary>
/// Rappresenta un singolo messaggio in una sessione AI.
/// Immutabile — creato solo tramite factory method.
/// </summary>
public class MessaggioAI : BaseEntity
{
    // ─────────────────────────────────────────────────────────────
    // Proprietà
    // ─────────────────────────────────────────────────────────────

    public string Testo { get; } = default!;
    public bool DaUtente { get; }
    public DateTime Timestamp { get; set; }

    //public string Ruolo { get; set; } = default!;      
    public string Contenuto { get; set; } = default!;
    public int Ordine { get; set; }

    /// <summary>
    /// Ruolo nel formato OpenAI: "user" | "assistant".
    /// Derivato da DaUtente — non persistere separatamente.
    /// </summary>
    public string Ruolo { get; set; } = default!; // "user" | "assistant"

    /// <summary>
    /// Riferimento alla sessione padre (FK per EF Core).
    /// </summary>
    public Guid SessioneAIId { get; private set; }

    // ✅ Costruttore senza parametri — riservato a EF Core
    protected MessaggioAI() { }

    // ─────────────────────────────────────────────────────────────
    // Costruttore privato
    // ─────────────────────────────────────────────────────────────

    public MessaggioAI(string testo, bool daUtente)
    {
        Id = Guid.NewGuid();
        Testo = testo;
        DaUtente = daUtente;
        Timestamp = DateTime.UtcNow;
        Ruolo = daUtente ? "user" : "assistant";
    }

    // ─────────────────────────────────────────────────────────────
    // Factory methods
    // ─────────────────────────────────────────────────────────────

    /// <summary>Crea un messaggio inviato dall'utente.</summary>
    public static MessaggioAI DaUtenteMsg(string testo)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(testo);
        return new MessaggioAI(testo.Trim(), daUtente: true);
    }

    /// <summary>Crea un messaggio generato dall'AI.</summary>
    public static MessaggioAI DaAI(string testo)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(testo);
        return new MessaggioAI(testo.Trim(), daUtente: false);
    }

    // ─────────────────────────────────────────────────────────────
    // Metodi interni (usati da SessioneAI)
    // ─────────────────────────────────────────────────────────────

    internal void AssegnaSessione(Guid sessioneId)
        => SessioneAIId = sessioneId;

    // ─────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Converti in tupla per IAIService.StreamChatAsync / ChatAsync.
    /// </summary>
    public (string Role, string Content) ToTuple()
        => (Ruolo, Testo);

    public override string ToString()
        => $"[{(DaUtente ? "Utente" : "AI")} {Timestamp:HH:mm}] {Testo[..Math.Min(60, Testo.Length)]}...";
}
