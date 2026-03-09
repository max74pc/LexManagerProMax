using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LexManagerProMax.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace LexManagerProMax.AvaloniaApp.ViewModels.Dashboard;

public partial class DashboardViewModel : ObservableObject
{
    private readonly ILogger<DashboardViewModel> _logger;

    // ─────────────────────────────────────────────────────────────
    // KPI — numeri grandi in cima
    // ─────────────────────────────────────────────────────────────

    private int _fascicoliAperti;
    public int FascicoliAperti
    {
        get => _fascicoliAperti;
        set => SetProperty(ref _fascicoliAperti, value);
    }

    private int _scadenzeOggi;
    public int ScadenzeOggi
    {
        get => _scadenzeOggi;
        set => SetProperty(ref _scadenzeOggi, value);
    }

    private int _scadenze7Giorni;
    public int Scadenze7Giorni
    {
        get => _scadenze7Giorni;
        set => SetProperty(ref _scadenze7Giorni, value);
    }

    private int _totaleClienti;
    public int TotaleClienti
    {
        get => _totaleClienti;
        set => SetProperty(ref _totaleClienti, value);
    }

    private int _fascicoliChiusiMese;
    public int FascicoliChiusiMese
    {
        get => _fascicoliChiusiMese;
        set => SetProperty(ref _fascicoliChiusiMese, value);
    }

    private int _sessioniAIUsate;
    public int SessioniAIUsate
    {
        get => _sessioniAIUsate;
        set => SetProperty(ref _sessioniAIUsate, value);
    }

    // ─────────────────────────────────────────────────────────────
    // Stato
    // ─────────────────────────────────────────────────────────────

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    private string _saluto = string.Empty;
    public string Saluto
    {
        get => _saluto;
        set => SetProperty(ref _saluto, value);
    }

    private string _dataOggi = string.Empty;
    public string DataOggi
    {
        get => _dataOggi;
        set => SetProperty(ref _dataOggi, value);
    }

    // ─────────────────────────────────────────────────────────────
    // Attività recenti
    // ─────────────────────────────────────────────────────────────

    private ObservableCollection<AttivitaRecenteItem> _attivitaRecenti = [];
    public ObservableCollection<AttivitaRecenteItem> AttivitaRecenti
    {
        get => _attivitaRecenti;
        set => SetProperty(ref _attivitaRecenti, value);
    }

    // ─────────────────────────────────────────────────────────────
    // Accessi rapidi
    // ─────────────────────────────────────────────────────────────

    public IReadOnlyList<AccessoRapidoItem> AccessiRapidi { get; } =
    [
        new() { Titolo = "Nuova Pratica",    Icona = "📁", ColoreAccento = "#2563EB", NavigateTo = "nuova-pratica"    },
        new() { Titolo = "Nuovo Cliente",    Icona = "👤", ColoreAccento = "#059669", NavigateTo = "nuovo-cliente"    },
        new() { Titolo = "Nuovo Modello",    Icona = "📄", ColoreAccento = "#7C3AED", NavigateTo = "nuovo-modello"    },
        new() { Titolo = "Assistente AI",    Icona = "🤖", ColoreAccento = "#DC2626", NavigateTo = "ai-assistente"    },
    ];

    // ─────────────────────────────────────────────────────────────
    // Costruttore
    // ─────────────────────────────────────────────────────────────

    public DashboardViewModel(ILogger<DashboardViewModel> logger)
    {
        _logger = logger;
        ImpostaSaluto();
        _ = LoadAsync();
    }

    // ─────────────────────────────────────────────────────────────
    // Caricamento dati
    // ─────────────────────────────────────────────────────────────

    private void ImpostaSaluto()
    {
        var ora = DateTime.Now.Hour;
        Saluto = ora switch
        {
            < 12 => "Buongiorno",
            < 18 => "Buon pomeriggio",
            _ => "Buonasera"
        };
        DataOggi = DateTime.Now.ToString("dddd d MMMM yyyy",
            new System.Globalization.CultureInfo("it-IT"));
    }

    private async Task LoadAsync()
    {
        IsLoading = true;
        try
        {
            // TODO: sostituire con query reali via IMediator
            // Esempio:
            // var kpi = await _mediator.Send(new GetDashboardKpiQuery());
            // FascicoliAperti   = kpi.FascicoliAperti;
            // ScadenzeOggi      = kpi.ScadenzeOggi;
            // Scadenze7Giorni   = kpi.Scadenze7Giorni;
            // TotaleClienti     = kpi.TotaleClienti;

            // Dati placeholder per il rendering immediato
            await Task.Delay(50); // simula chiamata async
            FascicoliAperti = 0;
            ScadenzeOggi = 0;
            Scadenze7Giorni = 0;
            TotaleClienti = 0;
            FascicoliChiusiMese = 0;
            SessioniAIUsate = 0;

            CaricaAttivitaRecenti();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore caricamento dati dashboard");
        }
        finally { IsLoading = false; }
    }

    private void CaricaAttivitaRecenti()
    {
        // TODO: sostituire con query reale
        // var items = await _mediator.Send(new GetAttivitaRecentiQuery(limit: 10));
        AttivitaRecenti = new ObservableCollection<AttivitaRecenteItem>
        {
            new()
            {
                Icona       = "📁",
                Descrizione = "Nessuna attività recente",
                Dettaglio   = "Le attività appariranno qui",
                Timestamp   = DateTime.Now,
                Tipo        = TipoAttivita.Info
            }
        };
    }

    // ─────────────────────────────────────────────────────────────
    // Commands
    // ─────────────────────────────────────────────────────────────

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadAsync();
    }

    [RelayCommand]
    private void NavigaA(string destination)
    {
        // TODO: collegare al NavigationService dell'app
        // _navigationService.NavigateTo(destination);
        _logger.LogInformation("Navigazione richiesta verso: {Destination}", destination);
    }
}