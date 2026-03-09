using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LexManagerProMax.Application.UseCases.Atti;
using LexManagerProMax.Domain.Entities.Atti;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LexManagerProMax.AvaloniaApp.ViewModels.Atti;

public partial class ModelliAttiViewModel : ObservableObject
{
    private readonly IMediator _mediator;
    private readonly ILogger<ModelliAttiViewModel> _logger;

    // ─────────────────────────────────────────────────────────────
    // Stato lista
    // ─────────────────────────────────────────────────────────────

    [ObservableProperty]
    private ObservableCollection<ModelloAttoListItem> _modelliRaggruppati = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TitoloEditor))]
    [NotifyPropertyChangedFor(nameof(VersioneLabel))]
    private ModelloAttoListItem? _modelloSelezionato;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private CategoriaAtto? _filtroCategoriaSelezionata;

    [ObservableProperty]
    private bool _isLoading;

    // ─────────────────────────────────────────────────────────────
    // Editor
    // ─────────────────────────────────────────────────────────────

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasModifiche))]
    private string _contenutoEditor = string.Empty;

    private string _contenutoOriginale = string.Empty;

    [ObservableProperty]
    private string _tagsEditor = string.Empty;

    [ObservableProperty]
    private string _nomeModello = string.Empty;

    [ObservableProperty]
    private string _descrizioneModello = string.Empty;

    [ObservableProperty]
    private string _nomeNuovoModello = string.Empty;

    [ObservableProperty]
    private string _noteVersione = string.Empty;

    [ObservableProperty]
    private CategoriaAtto _categoriaSelezionata = CategoriaAtto.Altro;

    // ─────────────────────────────────────────────────────────────
    // Modalità visualizzazione
    // ─────────────────────────────────────────────────────────────

    public enum ViewMode { Editor, Anteprima }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEditorVisibile))]
    [NotifyPropertyChangedFor(nameof(IsAnteprimaVisibile))]
    private ViewMode _modoEditor = ViewMode.Editor;

    public bool IsEditorVisibile => ModoEditor == ViewMode.Editor;
    public bool IsAnteprimaVisibile => ModoEditor == ViewMode.Anteprima;

    [ObservableProperty]
    private string _anteprimaContenuto = string.Empty;

    // ─────────────────────────────────────────────────────────────
    // Pannello segnaposto
    // ─────────────────────────────────────────────────────────────

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SegnapostoPanelWidth))]
    private bool _isSegnapostoPanelVisible;

    public double SegnapostoPanelWidth => IsSegnapostoPanelVisible ? 280 : 0;

    [ObservableProperty]
    private ObservableCollection<SegnapostoItem> _segnapostoItems = [];

    // ─────────────────────────────────────────────────────────────
    // Stato salvataggio
    // ─────────────────────────────────────────────────────────────

    [ObservableProperty]
    private bool _isSaving;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private string _successMessage = string.Empty;

    // ─────────────────────────────────────────────────────────────
    // Dialogo salvataggio nuovo modello
    // ─────────────────────────────────────────────────────────────

    [ObservableProperty]
    private bool _isNuovoModelloDialogOpen;

    // ─────────────────────────────────────────────────────────────
    // Proprietà derivate
    // ─────────────────────────────────────────────────────────────

    public IEnumerable<CategoriaAtto?> CategorieDisponibili =>
        new CategoriaAtto?[] { null }.Concat(Enum.GetValues<CategoriaAtto>().Cast<CategoriaAtto?>());

    public bool HasModifiche =>
        ModelloSelezionato is not null && ContenutoEditor != _contenutoOriginale;

    public string TitoloEditor => ModelloSelezionato is null
        ? "Nuovo modello"
        : ModelloSelezionato.Nome;

    public string VersioneLabel => ModelloSelezionato is null
        ? string.Empty
        : $"v{ModelloSelezionato.Versione}";

    // ─────────────────────────────────────────────────────────────
    // Costruttore
    // ─────────────────────────────────────────────────────────────

    public ModelliAttiViewModel(
        IMediator mediator,
        ILogger<ModelliAttiViewModel> logger)
    {
        _mediator = mediator;
        _logger = logger;
        _ = LoadAsync();
    }

    // ─────────────────────────────────────────────────────────────
    // Caricamento lista
    // ─────────────────────────────────────────────────────────────

    private async Task LoadAsync(
        string? searchTerm = null,
        CategoriaAtto? categoria = null)
    {
        IsLoading = true;
        try
        {
            var result = await _mediator.Send(
                new GetModelliAttiQuery(searchTerm, categoria));

            ModelliRaggruppati = new ObservableCollection<ModelloAttoListItem>(
                result.Select(ModelloAttoListItem.FromDomain));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore caricamento modelli atti");
            ErrorMessage = "Errore nel caricamento dei modelli.";
        }
        finally { IsLoading = false; }
    }

    // ─────────────────────────────────────────────────────────────
    // Selezione modello → carica nell'editor
    // ─────────────────────────────────────────────────────────────

    partial void OnModelloSelezionatoChanged(ModelloAttoListItem? value)
    {
        if (value is null) return;

        ContenutoEditor = value.Contenuto;
        _contenutoOriginale = value.Contenuto;
        CategoriaSelezionata = value.CategoriaEnum;
        TagsEditor = value.Tags ?? string.Empty;
        NoteVersione = string.Empty;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;
        ModoEditor = ViewMode.Editor;

        // Rileva i segnaposto presenti nel testo
        RilevaSegnpostoInContenuto(value.Contenuto);
    }

    // ─────────────────────────────────────────────────────────────
    // Rilevamento automatico segnaposto {{CHIAVE}}
    // ─────────────────────────────────────────────────────────────

    private void RilevaSegnpostoInContenuto(string contenuto)
    {
        var trovati = Regex.Matches(contenuto, @"\{\{(\w+)\}\}")
                           .Select(m => m.Groups[1].Value)
                           .Distinct()
                           .OrderBy(k => k)
                           .ToList();

        // Descrizioni predefinite per i segnaposto comuni
        var descrizioni = new Dictionary<string, (string Desc, string Placeholder)>
        {
            ["NOME_CLIENTE"] = ("Nome completo del cliente", "es. Mario Rossi"),
            ["NOME_CONTROPARTE"] = ("Nome della controparte", "es. Banca XYZ S.p.A."),
            ["DATA_OGGI"] = ("Data odierna", "gg/mm/aaaa"),
            ["TRIBUNALE"] = ("Tribunale competente", "es. Tribunale di Milano"),
            ["NR_RG"] = ("Numero di Ruolo Generale", "es. 12345/2024"),
            ["GIUDICE"] = ("Nome del giudice", "es. Dott.ssa Bianchi"),
            ["NOME_AVVOCATO"] = ("Nome dell'avvocato", "es. Avv. Giovanni Verdi"),
            ["CF_CLIENTE"] = ("Codice fiscale del cliente", "es. RSSMRA80A01F205X"),
            ["PIVA_CLIENTE"] = ("Partita IVA del cliente", "es. 12345678901"),
            ["INDIRIZZO"] = ("Indirizzo", "es. Via Roma 1, Milano"),
            ["IMPORTO"] = ("Importo in cifre e lettere", "es. € 10.000,00"),
            ["OGGETTO"] = ("Oggetto del procedimento", "es. risarcimento danni"),
        };

        // Mantieni i valori già inseriti se il segnaposto esisteva
        var esistenti = SegnapostoItems.ToDictionary(s => s.Chiave, s => s.Valore);

        SegnapostoItems = new ObservableCollection<SegnapostoItem>(
            trovati.Select(chiave =>
            {
                var (desc, placeholder) = descrizioni.TryGetValue(chiave, out var info)
                    ? info
                    : ("Valore personalizzato", $"inserisci {chiave.ToLower()}");

                return new SegnapostoItem
                {
                    Chiave = chiave,
                    Descrizione = desc,
                    Placeholder = placeholder,
                    Valore = esistenti.GetValueOrDefault(chiave, string.Empty)
                };
            }));
    }

    // ─────────────────────────────────────────────────────────────
    // Commands — navigazione vista
    // ─────────────────────────────────────────────────────────────

    [RelayCommand]
    private void MostraEditor() => ModoEditor = ViewMode.Editor;

    [RelayCommand]
    private void MostraAnteprima()
    {
        GeneraAnteprimaConValoriTest();
        ModoEditor = ViewMode.Anteprima;
    }

    [RelayCommand]
    private void ToggleSegnaposto()
    {
        IsSegnapostoPanelVisible = !IsSegnapostoPanelVisible;
        OnPropertyChanged(nameof(SegnapostoPanelWidth));
    }

    // ─────────────────────────────────────────────────────────────
    // Commands — editor
    // ─────────────────────────────────────────────────────────────

    [RelayCommand]
    private void InserisciTesto(string testo)
    {
        // Appende al cursore — in produzione usare il riferimento al TextBox
        ContenutoEditor += testo;
    }

    [RelayCommand]
    private void InserisciSegnaposto()
    {
        ContenutoEditor += "{{NUOVO_CAMPO}}";
        RilevaSegnpostoInContenuto(ContenutoEditor);
    }

    [RelayCommand]
    private void GeneraAnteprima()
    {
        GeneraAnteprimaConValoriTest();
        ModoEditor = ViewMode.Anteprima;
    }

    private void GeneraAnteprimaConValoriTest()
    {
        var valoriTest = new Dictionary<string, string>
        {
            ["NOME_CLIENTE"] = "Mario Rossi",
            ["NOME_CONTROPARTE"] = "Banca XYZ S.p.A.",
            ["DATA_OGGI"] = DateTime.Today.ToString("dd MMMM yyyy"),
            ["TRIBUNALE"] = "Tribunale di Milano",
            ["NR_RG"] = "12345/2024",
            ["GIUDICE"] = "Dott.ssa Giovanna Bianchi",
            ["NOME_AVVOCATO"] = "Avv. Giovanni Verdi",
            ["CF_CLIENTE"] = "RSSMRA80A01F205X",
            ["PIVA_CLIENTE"] = "12345678901",
            ["INDIRIZZO"] = "Via Roma, 1 — 20121 Milano (MI)",
            ["IMPORTO"] = "€ 10.000,00 (diecimila/00)",
            ["OGGETTO"] = "risarcimento danni da inadempimento contrattuale",
        };

        // Usa anche i valori personalizzati inseriti nel pannello segnaposto
        foreach (var item in SegnapostoItems.Where(s => !string.IsNullOrWhiteSpace(s.Valore)))
            valoriTest[item.Chiave] = item.Valore;

        var testo = ContenutoEditor;
        foreach (var (chiave, valore) in valoriTest)
            testo = testo.Replace($"{{{{{chiave}}}}}", valore);

        AnteprimaContenuto = testo;
    }

    [RelayCommand]
    private void GeneraConValori()
    {
        var valori = SegnapostoItems
            .ToDictionary(s => s.Chiave, s => string.IsNullOrWhiteSpace(s.Valore)
                ? $"[{s.Chiave}]"
                : s.Valore);

        var testo = ContenutoEditor;
        foreach (var (chiave, valore) in valori)
            testo = testo.Replace($"{{{{{chiave}}}}}", valore);

        AnteprimaContenuto = testo;
        ModoEditor = ViewMode.Anteprima;
    }

    // ─────────────────────────────────────────────────────────────
    // Commands — CRUD modello
    // ─────────────────────────────────────────────────────────────

    [RelayCommand]
    private void NuovoModello()
    {
        ModelloSelezionato = null;
        ContenutoEditor = GetTemplateVuoto();
        _contenutoOriginale = string.Empty;
        CategoriaSelezionata = CategoriaAtto.Altro;
        TagsEditor = string.Empty;
        NoteVersione = string.Empty;
        NomeNuovoModello = string.Empty;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;
        IsNuovoModelloDialogOpen = true;
        RilevaSegnpostoInContenuto(ContenutoEditor);
    }

    [RelayCommand]
    private async Task SalvaModelloAsync()
    {
        if (string.IsNullOrWhiteSpace(ContenutoEditor))
        {
            ErrorMessage = "Il contenuto del modello non può essere vuoto.";
            return;
        }

        IsSaving = true;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        try
        {
            ModelloAtto risultato;

            if (ModelloSelezionato is null)
            {
                // Nuovo modello
                if (string.IsNullOrWhiteSpace(NomeNuovoModello))
                {
                    ErrorMessage = "Inserire il nome del modello.";
                    return;
                }

                risultato = await _mediator.Send(new CreateModelloAttoCommand(
                    Nome: NomeNuovoModello.Trim(),
                    Categoria: CategoriaSelezionata,
                    Contenuto: ContenutoEditor,
                    Descrizione: DescrizioneModello.Trim()
                ));

                IsNuovoModelloDialogOpen = false;
                SuccessMessage = $"Modello '{risultato.Nome}' creato.";
            }
            else
            {
                // Aggiornamento
                risultato = await _mediator.Send(new UpdateModelloAttoCommand(
                    Id: ModelloSelezionato.Id,
                    Nome: ModelloSelezionato.Nome,
                    Categoria: CategoriaSelezionata,
                    Contenuto: ContenutoEditor,
                    NoteVersione: NoteVersione.Trim()
                ));

                SuccessMessage = $"Modello aggiornato alla v{risultato.VersioneCorrente}.";
                _contenutoOriginale = ContenutoEditor;
            }

            await LoadAsync();

            // Riseleziona il modello appena salvato
            ModelloSelezionato = ModelliRaggruppati
                .FirstOrDefault(m => m.Id == risultato.Id);
        }
        catch (InvalidOperationException ex)
        {
            ErrorMessage = ex.Message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore salvataggio modello atto");
            ErrorMessage = $"Errore imprevisto: {ex.Message}";
        }
        finally { IsSaving = false; }
    }

    [RelayCommand]
    private async Task EliminaModelloAsync()
    {
        if (ModelloSelezionato is null) return;

        // In produzione: mostrare una dialog di conferma
        try
        {
            await _mediator.Send(new DeleteModelloAttoCommand(ModelloSelezionato.Id));
            ModelloSelezionato = null;
            ContenutoEditor = string.Empty;
            _contenutoOriginale = string.Empty;
            await LoadAsync();
            SuccessMessage = "Modello eliminato.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore eliminazione modello");
            ErrorMessage = $"Errore durante l'eliminazione: {ex.Message}";
        }
    }

    [RelayCommand]
    private void AnnullaModifiche()
    {
        ContenutoEditor = _contenutoOriginale;
        NoteVersione = string.Empty;
        ErrorMessage = string.Empty;
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        await LoadAsync(
            searchTerm: string.IsNullOrWhiteSpace(SearchText) ? null : SearchText,
            categoria: FiltroCategoriaSelezionata);
    }

    [RelayCommand]
    private async Task EsportaDocxAsync()
    {
        // Placeholder — integra con il PDF/DOCX service
        // var contenuto = GeneraDocumentoFinale();
        // await _docxService.EsportaAsync(contenuto, ModelloSelezionato!.Nome);
        SuccessMessage = "Esportazione .docx — funzionalità in sviluppo.";
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task ApriStoricoVersioniAsync()
    {
        // In produzione: apri un dialog/flyout con la lista delle versioni
        SuccessMessage = "Storico versioni — funzionalità in sviluppo.";
        await Task.CompletedTask;
    }

    // ─────────────────────────────────────────────────────────────
    // Reactions ai cambi di filtro
    // ─────────────────────────────────────────────────────────────

    partial void OnSearchTextChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            _ = LoadAsync(categoria: FiltroCategoriaSelezionata);
    }

    partial void OnFiltroCategoriaSelezionataChanged(CategoriaAtto? value)
        => _ = LoadAsync(searchTerm: SearchText, categoria: value);

    // ─────────────────────────────────────────────────────────────
    // Template vuoto default
    // ─────────────────────────────────────────────────────────────

    private static string GetTemplateVuoto() => """
        TRIBUNALE DI {{TRIBUNALE}}
        Sezione _______________

        Procedimento n. {{NR_RG}}
        Giudice: {{GIUDICE}}

        ───────────────────────────────────────────────────

        RICORSO / ATTO DI CITAZIONE

        Per conto e nell'interesse di:
        {{NOME_CLIENTE}}
        C.F. / P.IVA: {{CF_CLIENTE}}
        residente/con sede in {{INDIRIZZO}}

        difeso e rappresentato dall'Avv. {{NOME_AVVOCATO}},

        contro:
        {{NOME_CONTROPARTE}}

        ───────────────────────────────────────────────────

        FATTO

        [Descrivere i fatti rilevanti...]

        DIRITTO

        [Esporre le ragioni giuridiche...]

        P.Q.M.

        Si chiede che l'Ill.mo Tribunale voglia:

        1. [...]
        2. [...]

        Con vittoria di spese e competenze di lite.

        {{TRIBUNALE}}, {{DATA_OGGI}}

        Avv. {{NOME_AVVOCATO}}
        """;

    // ─────────────────────────────────────────────────────────────
    // Pulsanti rapidi per la toolbar dell'editor.
    // Le stringhe con graffe doppie sono costruite QUI in C#,
    // mai nel file AXAML, per evitare il parser error XAML.
    // ─────────────────────────────────────────────────────────────

    private List<SegnapostoRapidoItem> _segnapostoRapidi = BuildSegnapostoRapidi();
    public List<SegnapostoRapidoItem> SegnapostoRapidi
    {
        get => _segnapostoRapidi;
        set => SetProperty(ref _segnapostoRapidi, value);
    }

    private static List<SegnapostoRapidoItem> BuildSegnapostoRapidi() =>
    [
        new() { Label = "NOME_CLIENTE",     Valore = "{{NOME_CLIENTE}}"     },
        new() { Label = "DATA_OGGI",        Valore = "{{DATA_OGGI}}"        },
        new() { Label = "TRIBUNALE",        Valore = "{{TRIBUNALE}}"        },
        new() { Label = "NR_RG",            Valore = "{{NR_RG}}"            },
        new() { Label = "NOME_AVVOCATO",    Valore = "{{NOME_AVVOCATO}}"    },
        new() { Label = "NOME_CONTROPARTE", Valore = "{{NOME_CONTROPARTE}}" },
        new() { Label = "CF_CLIENTE",       Valore = "{{CF_CLIENTE}}"       },
        new() { Label = "IMPORTO",          Valore = "{{IMPORTO}}"          },
        new() { Label = "OGGETTO",          Valore = "{{OGGETTO}}"          },
    ];
}