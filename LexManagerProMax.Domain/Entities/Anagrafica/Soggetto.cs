using LexManagerProMax.Domain.Common;

namespace LexManagerProMax.Domain.Entities.Anagrafica;

public enum TipoSoggetto { PersonaFisica, PersonaGiuridica, Ente }

public sealed class Soggetto : BaseEntity
{

    public TipoSoggetto Tipo { get; private set; }

    // Persona fisica    
    public string? Cognome { get; private set; }
    public string? Nome { get; private set; }
    public string? CodiceFiscale { get; private set; }
    public DateOnly? DataNascita { get; private set; }
    public string? LuogoNascita { get; private set; }

    // Persona giuridica    
    public string? RagioneSociale { get; private set; }
    public string? PartitaIva { get; private set; }
    public string? FormaGiuridica { get; private set; }

    // Recapiti    
    public string? Email { get; private set; }
    public string? Pec { get; private set; }
    public string? Telefono { get; private set; }
    public string? CellulareUno { get; private set; }

    // Indirizzo   
    public string? Via { get; private set; }
    public string? Citta { get; private set; }
    public string? Cap { get; private set; }
    public string? Provincia { get; private set; }
    public string? Nazione { get; private set; } = "Italia";
    public string? Note { get; private set; }
    public bool IsCliente { get; private set; }
    public bool IsControparte { get; private set; }

    private Soggetto() { }

    public static Soggetto CreaPersonaFisica(
        string cognome, string nome,
        string? codiceFiscale = null,
        DateOnly? dataNascita = null,
        string? email = null,
        string? pec = null,
        string? telefono = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cognome); ArgumentException.ThrowIfNullOrWhiteSpace(nome);

        return new Soggetto
        {
            Tipo = TipoSoggetto.PersonaFisica,
            Cognome = cognome.Trim().ToUpperInvariant(),
            Nome = nome.Trim(),
            CodiceFiscale = codiceFiscale?.Trim().ToUpperInvariant(),
            DataNascita = dataNascita,
            Email = email,
            Pec = pec,
            Telefono = telefono
        };
    }

    public static Soggetto CreaPersonaGiuridica(
        string ragioneSociale,
        string? partitaIva = null,
        string? formaGiuridica = null,
        string? email = null,
        string? pec = null)
    {

        ArgumentException.ThrowIfNullOrWhiteSpace(ragioneSociale);

        return new Soggetto
        {
            Tipo = TipoSoggetto.PersonaGiuridica,
            RagioneSociale = ragioneSociale.Trim(),
            PartitaIva = partitaIva?.Trim(),
            FormaGiuridica = formaGiuridica,
            Email = email,
            Pec = pec
        };
    }

    public string NomeCompleto => Tipo == TipoSoggetto.PersonaFisica
        ? $"{Cognome} {Nome}"
        : RagioneSociale ?? string.Empty;

    public void AggiornaDati(
       string? email = null,
       string? pec = null,
       string? telefono = null,
       string? cellulare = null,
       string? via = null,
       string? citta = null,
       string? cap = null,
       string? provincia = null,
       string? note = null)
    {
        Email = email ?? Email;
        Pec = pec ?? Pec;
        Telefono = telefono ?? Telefono;
        CellulareUno = cellulare ?? CellulareUno;
        Via = via ?? Via;
        Citta = citta ?? Citta;
        Cap = cap ?? Cap;
        Provincia = provincia ?? Provincia;
        Note = note ?? Note;
        MarkAsUpdated();
    }
    public void ImpostaRuolo(bool isCliente, bool isControparte)
    {
        IsCliente = isCliente;
        IsControparte = isControparte; MarkAsUpdated();
    }
}