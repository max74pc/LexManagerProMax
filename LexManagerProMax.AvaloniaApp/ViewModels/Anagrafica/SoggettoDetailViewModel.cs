using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LexManagerProMax.Domain.Entities.Anagrafica;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using LexManagerProMax.Application.UseCases.Anagrafica;
using System.Text.RegularExpressions;

namespace LexManagerProMax.AvaloniaApp.ViewModels.Anagrafica;

/// <summary>
/// ViewModel per la creazione e modifica di un Soggetto (persona fisica o giuridica).
/// Viene istanziato sia per nuovi soggetti (soggetto == null) che per la modifica di esistenti.
/// </summary>
public partial class SoggettoDetailViewModel : ObservableValidator
{
    private readonly IMediator _mediator;
    private readonly Soggetto? _soggettoOriginale;

    [GeneratedRegex(@"^[A-Z0-9]{16}$")]
    private static partial Regex RegexCodiceFiscale();

    // ─────────────────────────────────────────────────────────────
    // Stato del form
    // ─────────────────────────────────────────────────────────────

    [ObservableProperty]
    private bool _isNuovo;

    [ObservableProperty]
    private bool _isSaving;

    [ObservableProperty]
    private bool _hasFormError;

    [ObservableProperty]
    private bool _hasErrors;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private string _successMessage = string.Empty;

    // ─────────────────────────────────────────────────────────────
    // Tipo soggetto
    // ─────────────────────────────────────────────────────────────

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPersonaFisica))]
    [NotifyPropertyChangedFor(nameof(IsPersonaGiuridica))]
    private TipoSoggetto _tipoSoggetto = TipoSoggetto.PersonaFisica;

    public bool IsPersonaFisica => TipoSoggetto == TipoSoggetto.PersonaFisica;
    public bool IsPersonaGiuridica => TipoSoggetto == TipoSoggetto.PersonaGiuridica;

    public static IEnumerable<TipoSoggetto> TipiSoggetto => Enum.GetValues<TipoSoggetto>();

    // ─────────────────────────────────────────────────────────────
    // Campi persona fisica
    // ─────────────────────────────────────────────────────────────

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [CustomValidation(typeof(SoggettoDetailViewModel), nameof(ValidaCognome))]
    private string _cognome = string.Empty;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [CustomValidation(typeof(SoggettoDetailViewModel), nameof(ValidaNome))]
    private string _nome = string.Empty;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [CustomValidation(typeof(SoggettoDetailViewModel), nameof(ValidaCodiceFiscale))]
    private string _codiceFiscale = string.Empty;

    [ObservableProperty]
    private DateTimeOffset? _dataNascita;

    [ObservableProperty]
    private string _luogoNascita = string.Empty;

    [ObservableProperty]
    private string _sesso = string.Empty;

    // ─────────────────────────────────────────────────────────────
    // Campi persona giuridica
    // ─────────────────────────────────────────────────────────────

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [CustomValidation(typeof(SoggettoDetailViewModel), nameof(ValidaRagioneSociale))]
    private string _ragioneSociale = string.Empty;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [CustomValidation(typeof(SoggettoDetailViewModel), nameof(ValidaPartitaIva))]
    private string _partitaIva = string.Empty;

    [ObservableProperty]
    private string _formaGiuridica = string.Empty;

    public static IEnumerable<string> FormeGiuridiche =>
    [
        "S.r.l.", "S.r.l.s.", "S.p.A.", "S.a.s.", "S.n.c.",
        "S.s.", "S.c.a r.l.", "Cooperativa", "Fondazione",
        "Associazione", "Ente Pubblico", "Altro"
    ];

    // ─────────────────────────────────────────────────────────────
    // Recapiti (comuni)
    // ─────────────────────────────────────────────────────────────

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [CustomValidation(typeof(SoggettoDetailViewModel), nameof(ValidaEmail))]
    private string _email = string.Empty;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [CustomValidation(typeof(SoggettoDetailViewModel), nameof(ValidaEmail))]
    private string _pec = string.Empty;

    [ObservableProperty]
    private string _telefono = string.Empty;

    [ObservableProperty]
    private string _cellulare = string.Empty;

    [ObservableProperty]
    private string _fax = string.Empty;

    // ─────────────────────────────────────────────────────────────
    // Indirizzo
    // ─────────────────────────────────────────────────────────────

    [ObservableProperty]
    private string _via = string.Empty;

    [ObservableProperty]
    private string _civico = string.Empty;

    [ObservableProperty]
    private string _cap = string.Empty;

    [ObservableProperty]
    private string _citta = string.Empty;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [MaxLength(2, ErrorMessage = "La provincia deve essere di 2 caratteri (es. MI)")]
    private string _provincia = string.Empty;

    [ObservableProperty]
    private string _nazione = "Italia";

    // ─────────────────────────────────────────────────────────────
    // Ruoli e note
    // ─────────────────────────────────────────────────────────────

    [ObservableProperty]
    private bool _isCliente = true;

    [ObservableProperty]
    private bool _isControparte;

    [ObservableProperty]
    private bool _isTerzo;

    [ObservableProperty]
    private string _note = string.Empty;

    [ObservableProperty]
    private string _categoriaCliente = string.Empty;

    public static IEnumerable<string> CategorieClienti =>
    [
        "Privato", "Azienda", "Ente Pubblico",
        "Assicurazione", "Banca", "Altro"
    ];

    // ─────────────────────────────────────────────────────────────
    // Proprietà derivate / UI helpers
    // ─────────────────────────────────────────────────────────────

    public string TitoloForm => IsNuovo
        ? "Nuovo Soggetto"
        : $"Modifica — {NomeCompletoCalcolato}";

    public string NomeCompletoCalcolato => TipoSoggetto == TipoSoggetto.PersonaFisica
        ? $"{Cognome} {Nome}".Trim()
        : RagioneSociale.Trim();

    public string IndirizzoCompleto =>
        string.Join(", ",
            new[] { $"{Via} {Civico}".Trim(), Cap, Citta, Provincia, Nazione }
            .Where(s => !string.IsNullOrWhiteSpace(s)));

    // ─────────────────────────────────────────────────────────────
    // Tab corrente (0=Dati, 1=Recapiti, 2=Indirizzo, 3=Note)
    // ─────────────────────────────────────────────────────────────

    [ObservableProperty]
    private int _tabSelezionato;

    // ─────────────────────────────────────────────────────────────
    // Eventi
    // ─────────────────────────────────────────────────────────────

    /// <summary>Scatena quando il salvataggio va a buon fine. Il parent può chiudere il pannello.</summary>
    public event EventHandler<Soggetto>? SalvataggioCompletato;

    /// <summary>Scatena quando l'utente annulla.</summary>
    public event EventHandler? Annullato;

    // ─────────────────────────────────────────────────────────────
    // Costruttore
    // ─────────────────────────────────────────────────────────────

    public SoggettoDetailViewModel(IMediator mediator, Soggetto? soggetto)
    {
        _mediator = mediator;
        _soggettoOriginale = soggetto;
        IsNuovo = soggetto is null;

        if (soggetto is not null)
            CaricaDaSoggetto(soggetto);
    }

    // ─────────────────────────────────────────────────────────────
    // Caricamento dati esistenti
    // ─────────────────────────────────────────────────────────────

    private void CaricaDaSoggetto(Soggetto s)
    {
        TipoSoggetto = s.Tipo;

        // Persona fisica
        Cognome = s.Cognome ?? string.Empty;
        Nome = s.Nome ?? string.Empty;
        CodiceFiscale = s.CodiceFiscale ?? string.Empty;
        LuogoNascita = s.LuogoNascita ?? string.Empty;
        DataNascita = s.DataNascita.HasValue
            ? new DateTimeOffset(s.DataNascita.Value.ToDateTime(TimeOnly.MinValue))
            : null;

        // Persona giuridica
        RagioneSociale = s.RagioneSociale ?? string.Empty;
        PartitaIva = s.PartitaIva ?? string.Empty;
        FormaGiuridica = s.FormaGiuridica ?? string.Empty;

        // Recapiti
        Email = s.Email ?? string.Empty;
        Pec = s.Pec ?? string.Empty;
        Telefono = s.Telefono ?? string.Empty;
        Cellulare = s.CellulareUno ?? string.Empty;

        // Indirizzo
        Via = s.Via ?? string.Empty;
        Cap = s.Cap ?? string.Empty;
        Citta = s.Citta ?? string.Empty;
        Provincia = s.Provincia ?? string.Empty;
        Nazione = s.Nazione ?? "Italia";

        // Ruoli
        IsCliente = s.IsCliente;
        IsControparte = s.IsControparte;

        // Note
        Note = s.Note ?? string.Empty;
    }

    // ─────────────────────────────────────────────────────────────
    // Commands
    // ─────────────────────────────────────────────────────────────

    [RelayCommand]
    private async Task SalvaAsync()
    {
        // Esegui validazione su tutti i campi
        ValidateAllProperties();

        // Validazione di business aggiuntiva
        var errBusiness = ValidaBusiness();
        if (errBusiness is not null)
        {
            HasErrors = true;
            ErrorMessage = errBusiness;
            return;
        }

        IsSaving = true;
        HasErrors = false;
        ErrorMessage = string.Empty;

        try
        {
            Soggetto risultato;

            if (IsNuovo)
            {
                var cmd = new CreateSoggettoCommand(
                    Tipo: TipoSoggetto,
                    Cognome: Cognome.Trim(),
                    Nome: Nome.Trim(),
                    CodiceFiscale: string.IsNullOrWhiteSpace(CodiceFiscale) ? null : CodiceFiscale.Trim().ToUpperInvariant(),
                    RagioneSociale: RagioneSociale.Trim(),
                    PartitaIva: string.IsNullOrWhiteSpace(PartitaIva) ? null : PartitaIva.Trim(),
                    Email: string.IsNullOrWhiteSpace(Email) ? null : Email.Trim(),
                    Pec: string.IsNullOrWhiteSpace(Pec) ? null : Pec.Trim(),
                    Telefono: string.IsNullOrWhiteSpace(Telefono) ? null : Telefono.Trim(),
                    IsCliente: IsCliente
                );
                risultato = await _mediator.Send(cmd);
            }
            else
            {
                var cmd = new UpdateSoggettoCommand(
                    Id: _soggettoOriginale!.Id,
                    Email: string.IsNullOrWhiteSpace(Email) ? null : Email.Trim(),
                    Pec: string.IsNullOrWhiteSpace(Pec) ? null : Pec.Trim(),
                    Telefono: string.IsNullOrWhiteSpace(Telefono) ? null : Telefono.Trim(),
                    Cellulare: string.IsNullOrWhiteSpace(Cellulare) ? null : Cellulare.Trim(),
                    Via: string.IsNullOrWhiteSpace(Via) ? null : Via.Trim(),
                    Citta: string.IsNullOrWhiteSpace(Citta) ? null : Citta.Trim(),
                    Cap: string.IsNullOrWhiteSpace(Cap) ? null : Cap.Trim(),
                    Provincia: string.IsNullOrWhiteSpace(Provincia) ? null : Provincia.Trim().ToUpperInvariant(),
                    Note: string.IsNullOrWhiteSpace(Note) ? null : Note.Trim(),
                    IsCliente: IsCliente,
                    IsControparte: IsControparte
                );
                risultato = await _mediator.Send(cmd);
            }

            SuccessMessage = IsNuovo
                ? $"Soggetto '{risultato.NomeCompleto}' creato con successo."
                : $"Soggetto '{risultato.NomeCompleto}' aggiornato.";

            SalvataggioCompletato?.Invoke(this, risultato);
        }
        catch (Exception ex)
        {
            HasErrors = true;
            ErrorMessage = $"Errore durante il salvataggio: {ex.Message}";
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private void Annulla()
    {
        Annullato?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void ResetForm()
    {
        if (_soggettoOriginale is not null)
            CaricaDaSoggetto(_soggettoOriginale);
        else
        {
            Cognome = string.Empty;
            Nome = string.Empty;
            CodiceFiscale = string.Empty;
            RagioneSociale = string.Empty;
            PartitaIva = string.Empty;
            Email = string.Empty;
            Pec = string.Empty;
            Telefono = string.Empty;
            Cellulare = string.Empty;
            Via = string.Empty;
            Cap = string.Empty;
            Citta = string.Empty;
            Provincia = string.Empty;
            Note = string.Empty;
        }

        HasErrors = false;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;
        ClearErrors();
    }

    /// <summary>Cambia il tab attivo (0..3)</summary>
    [RelayCommand]
    private void CambiaTabs(int index) => TabSelezionato = index;

    [RelayCommand]
    private async Task CopiaCfAsync()
    {
        var testo = IsPersonaFisica ? CodiceFiscale : PartitaIva;

        if (string.IsNullOrWhiteSpace(testo))
            return;

        var topLevel = TopLevel.GetTopLevel(App.Current?.ApplicationLifetime switch
        {
            IClassicDesktopStyleApplicationLifetime desktop => desktop.MainWindow,
            _ => null
        });

        if (topLevel?.Clipboard is not null)
            await topLevel.Clipboard.SetTextAsync(testo);
    }

    // ─────────────────────────────────────────────────────────────
    // Validatori statici (usati da [CustomValidation])
    // ─────────────────────────────────────────────────────────────

    public static ValidationResult? ValidaCognome(string value, ValidationContext ctx)
    {
        var vm = (SoggettoDetailViewModel)ctx.ObjectInstance;
        if (!vm.IsPersonaFisica) return ValidationResult.Success;
        return string.IsNullOrWhiteSpace(value)
            ? new ValidationResult("Il cognome è obbligatorio per le persone fisiche.")
            : ValidationResult.Success;
    }

    public static ValidationResult? ValidaNome(string value, ValidationContext ctx)
    {
        var vm = (SoggettoDetailViewModel)ctx.ObjectInstance;
        if (!vm.IsPersonaFisica) return ValidationResult.Success;
        return string.IsNullOrWhiteSpace(value)
            ? new ValidationResult("Il nome è obbligatorio per le persone fisiche.")
            : ValidationResult.Success;
    }

    public static ValidationResult? ValidaCodiceFiscale(string value, ValidationContext ctx)
    {
        if (string.IsNullOrWhiteSpace(value)) return ValidationResult.Success;
        ArgumentNullException.ThrowIfNull(ctx);
        var cf = value.Trim().ToUpperInvariant();
        if (cf.Length != 16)
            return new ValidationResult("Il codice fiscale deve essere di 16 caratteri.");
        if (!RegexCodiceFiscale().IsMatch(cf))
            return new ValidationResult("Il codice fiscale contiene caratteri non validi.");
        return ValidationResult.Success;
    }

    public static ValidationResult? ValidaRagioneSociale(string value, ValidationContext ctx)
    {
        var vm = (SoggettoDetailViewModel)ctx.ObjectInstance;
        if (!vm.IsPersonaGiuridica) return ValidationResult.Success;
        return string.IsNullOrWhiteSpace(value)
            ? new ValidationResult("La ragione sociale è obbligatoria per le persone giuridiche.")
            : ValidationResult.Success;
    }

    public static ValidationResult? ValidaPartitaIva(string value, ValidationContext ctx)
    {
        if (string.IsNullOrWhiteSpace(value)) return ValidationResult.Success; // facoltativa
        ArgumentNullException.ThrowIfNull(ctx);
        var piva = value.Trim();
        if (piva.Length != 11 || !piva.All(char.IsDigit))
            return new ValidationResult("La Partita IVA deve essere di 11 cifre numeriche.");
        return ValidationResult.Success;
    }

    public static ValidationResult? ValidaEmail(string value, ValidationContext ctx)
    {
        if (string.IsNullOrWhiteSpace(value)) return ValidationResult.Success;
        ArgumentNullException.ThrowIfNull(ctx);
        try
        {
            var addr = new System.Net.Mail.MailAddress(value.Trim());
            return addr.Address == value.Trim()
                ? ValidationResult.Success
                : new ValidationResult("Indirizzo email non valido.");
        }
        catch
        {
            return new ValidationResult("Indirizzo email non valido.");
        }
    }

    // ─────────────────────────────────────────────────────────────
    // Validazione di business (eseguita prima del salvataggio)
    // ─────────────────────────────────────────────────────────────

    private string? ValidaBusiness()
    {
        if (!IsCliente && !IsControparte && !IsTerzo)
            return "Il soggetto deve avere almeno un ruolo: Cliente, Controparte o Terzo.";

        if (IsPersonaGiuridica && string.IsNullOrWhiteSpace(PartitaIva) && string.IsNullOrWhiteSpace(CodiceFiscale))
            return "Inserire almeno la Partita IVA o il Codice Fiscale per le persone giuridiche.";

        return null;
    }

}