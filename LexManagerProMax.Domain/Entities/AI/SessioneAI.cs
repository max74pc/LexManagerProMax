using LexManagerProMax.Domain.Common;

namespace LexManagerProMax.Domain.Entities.AI;

public enum TipoRicercaAI { Dottrina, Giurisprudenza, Normativa, Assistenza }

public sealed class SessioneAI : BaseEntity
{
    public string Titolo { get; private set; } = default!;
    public TipoRicercaAI TipoRicerca { get; private set; }
    public DateTime DataSessione { get; private set; } = DateTime.UtcNow;
    public Guid? FascicoloId { get; private set; }

    private readonly List<MessaggioAI> _messaggi = [];
    public IReadOnlyList<MessaggioAI> Messaggi => _messaggi.AsReadOnly();

    private SessioneAI() { }

    public static SessioneAI Crea(
        string titolo,
        TipoRicercaAI tipo,
        Guid? fascicoloId = null)
        => new() { Titolo = titolo, TipoRicerca = tipo, FascicoloId = fascicoloId };

    public void AggiungiMessaggio(string contenuto, string ruolo)
    {
        _messaggi.Add(new MessaggioAI(contenuto, ruolo == "user")
        {
            Ruolo = ruolo,
            Contenuto = contenuto,
            Timestamp = DateTime.UtcNow,
            Ordine = _messaggi.Count + 1
        });
    }

    /// <summary>Rinomina la sessione. Chiamato da RinominaSessioneAICommand.</summary>
    public void Rinomina(string nuovoTitolo)
    {
        if (string.IsNullOrWhiteSpace(nuovoTitolo))
            throw new ArgumentException("Il titolo non può essere vuoto.", nameof(nuovoTitolo));

        Titolo = nuovoTitolo.Trim();
    }
}