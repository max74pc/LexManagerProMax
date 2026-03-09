using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LexManagerProMax.Application.UseCases.AI;
using LexManagerProMax.Domain.Entities.AI;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LexManagerProMax.AvaloniaApp.ViewModels.AI;

public partial class AIAssistantViewModel : ObservableObject
{
    private readonly IMediator _mediator;
    private readonly ILogger<AIAssistantViewModel> _logger;
    private CancellationTokenSource? _streamCts;

    // ─────────────────────────────────────────────────────────────
    // Proprietà — tutte manuali (regola source generator)
    // ─────────────────────────────────────────────────────────────

    private ObservableCollection<SessioneAIListItem> _sessioni = [];
    public ObservableCollection<SessioneAIListItem> Sessioni
    {
        get => _sessioni;
        set => SetProperty(ref _sessioni, value);
    }

    private SessioneAIListItem? _sessioneSelezionata;
    public SessioneAIListItem? SessioneSelezionata
    {
        get => _sessioneSelezionata;
        set
        {
            if (SetProperty(ref _sessioneSelezionata, value) && value is not null)
                _ = CaricaSessioneAsync(value.Id);
        }
    }

    private ObservableCollection<ChatBubbleItem> _messaggi = [];
    public ObservableCollection<ChatBubbleItem> Messaggi
    {
        get => _messaggi;
        set => SetProperty(ref _messaggi, value);
    }

    private string _inputUtente = string.Empty;
    public string InputUtente
    {
        get => _inputUtente;
        set
        {
            if (SetProperty(ref _inputUtente, value))
                OnPropertyChanged(nameof(CanSend));
        }
    }

    private bool _isStreaming;
    public bool IsStreaming
    {
        get => _isStreaming;
        set
        {
            if (SetProperty(ref _isStreaming, value))
                OnPropertyChanged(nameof(CanSend));
        }
    }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    private string _errorMessage = string.Empty;
    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    private TipoRicercaAI _tipoSelezionato = TipoRicercaAI.Assistenza;
    public TipoRicercaAI TipoSelezionato
    {
        get => _tipoSelezionato;
        set => SetProperty(ref _tipoSelezionato, value);
    }

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value) && string.IsNullOrWhiteSpace(value))
                _ = LoadSessioniAsync();
        }
    }

    private bool _isRinominaMode;
    public bool IsRinominaMode
    {
        get => _isRinominaMode;
        set => SetProperty(ref _isRinominaMode, value);
    }

    private string _nuovoTitolo = string.Empty;
    public string NuovoTitolo
    {
        get => _nuovoTitolo;
        set => SetProperty(ref _nuovoTitolo, value);
    }

    // ─────────────────────────────────────────────────────────────
    // Proprietà derivate
    // ─────────────────────────────────────────────────────────────

    public bool CanSend =>
        !IsStreaming && !string.IsNullOrWhiteSpace(InputUtente);

    public bool HasSessione => SessioneSelezionata is not null;

    public static IEnumerable<TipoRicercaAI> TipiDisponibili =>
        Enum.GetValues<TipoRicercaAI>();

    // Prompt rapidi per tipo
    public IReadOnlyList<PromptRapidoItem> PromptRapidi =>
        PromptRapidoItem.GetByTipo(TipoSelezionato);

    // ─────────────────────────────────────────────────────────────
    // Sessione corrente
    // ─────────────────────────────────────────────────────────────

    private Guid? _sessioneCorrenteId;

    // ─────────────────────────────────────────────────────────────
    // Costruttore
    // ─────────────────────────────────────────────────────────────

    public AIAssistantViewModel(
        IMediator mediator,
        ILogger<AIAssistantViewModel> logger)
    {
        _mediator = mediator;
        _logger = logger;
        _ = LoadSessioniAsync();
    }

    // ─────────────────────────────────────────────────────────────
    // Caricamento sessioni
    // ─────────────────────────────────────────────────────────────

    private async Task LoadSessioniAsync(
        string? search = null,
        TipoRicercaAI? tipo = null)
    {
        IsLoading = true;
        try
        {
            var result = await _mediator.Send(
                new GetSessioniAIQuery(search, tipo));

            Sessioni = new ObservableCollection<SessioneAIListItem>(
                result.Select(SessioneAIListItem.FromDomain));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore caricamento sessioni AI");
            ErrorMessage = "Errore nel caricamento delle sessioni.";
        }
        finally { IsLoading = false; }
    }

    //private async Task CaricaSessioneAsync(Guid id)
    //{
    //    IsLoading = true;
    //    Messaggi.Clear();
    //    try
    //    {
    //        var result = await _mediator.Send(new GetSessioniAIQuery());
    //        // Carica i messaggi dalla sessione
    //        var sessione = await _mediator.Send(
    //            new GetSessioniAIQuery()) as IReadOnlyList<SessioneAI>;

    //        // Usa il repository direttamente tramite query dedicata
    //        // In produzione: GetSessioneByIdQuery
    //        _sessioneCorrenteId = id;
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Errore caricamento sessione {Id}", id);
    //        ErrorMessage = "Errore nel caricamento della sessione.";
    //    }
    //    finally { IsLoading = false; }
    //}
    private async Task CaricaSessioneAsync(Guid id)
    {
        IsLoading = true;
        Messaggi.Clear();
        try
        {
            var sessione = await _mediator.Send(new GetSessioneByIdQuery(id));
            if (sessione is null) return;

            _sessioneCorrenteId = id;

            foreach (var msg in sessione.Messaggi.OrderBy(m => m.CreatedAt))
            {
                Messaggi.Add(new ChatBubbleItem
                {
                    Testo = msg.Testo,
                    IsUtente = msg.Ruolo == "user",
                    Timestamp = msg.CreatedAt,
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore caricamento sessione {Id}", id);
            ErrorMessage = "Errore nel caricamento della sessione.";
        }
        finally { IsLoading = false; }
    }

    // ─────────────────────────────────────────────────────────────
    // Commands — sessioni
    // ─────────────────────────────────────────────────────────────

    [RelayCommand]
    private async Task NuovaSessioneAsync()
    {
        try
        {
            var sessione = await _mediator.Send(
                new CreateSessioneAICommand(TipoSelezionato));

            _sessioneCorrenteId = sessione.Id;
            Messaggi.Clear();
            ErrorMessage = string.Empty;

            await LoadSessioniAsync();

            SessioneSelezionata = Sessioni
                .FirstOrDefault(s => s.Id == sessione.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore creazione sessione AI");
            ErrorMessage = $"Errore: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task EliminaSessioneAsync()
    {
        if (SessioneSelezionata is null) return;
        try
        {
            await _mediator.Send(
                new DeleteSessioneAICommand(SessioneSelezionata.Id));

            _sessioneCorrenteId = null;
            SessioneSelezionata = null;
            Messaggi.Clear();
            await LoadSessioniAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore eliminazione sessione");
            ErrorMessage = $"Errore: {ex.Message}";
        }
    }

    [RelayCommand]
    private void AvviaRinomina()
    {
        if (SessioneSelezionata is null) return;
        NuovoTitolo = SessioneSelezionata.Titolo;
        IsRinominaMode = true;
    }

    [RelayCommand]
    private async Task ConfermaRinominaAsync()
    {
        if (SessioneSelezionata is null ||
            string.IsNullOrWhiteSpace(NuovoTitolo)) return;
        try
        {
            await _mediator.Send(
                new RinominaSessioneAICommand(SessioneSelezionata.Id, NuovoTitolo));

            IsRinominaMode = false;
            await LoadSessioniAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Errore rinomina: {ex.Message}";
        }
    }

    [RelayCommand]
    private void AnnullaRinomina() => IsRinominaMode = false;

    [RelayCommand]
    private async Task SearchAsync()
        => await LoadSessioniAsync(
            search: string.IsNullOrWhiteSpace(SearchText) ? null : SearchText);

    // ─────────────────────────────────────────────────────────────
    // Commands — messaggi / streaming
    // ─────────────────────────────────────────────────────────────

    [RelayCommand]
    private async Task InviaMessaggioAsync()
    {
        if (!CanSend) return;
        if (_sessioneCorrenteId is null)
        {
            // Crea sessione automaticamente se non esiste
            await NuovaSessioneAsync();
            if (_sessioneCorrenteId is null) return;
        }

        var testo = InputUtente.Trim();
        InputUtente = string.Empty;

        // Aggiunge bubble utente immediatamente
        Messaggi.Add(new ChatBubbleItem
        {
            Testo = testo,
            IsUtente = true,
            Timestamp = DateTime.Now
        });

        // Prepara bubble AI con testo vuoto (si riempirà via streaming)
        var bubbleAI = new ChatBubbleItem
        {
            Testo = string.Empty,
            IsUtente = false,
            IsLoading = true,
            Timestamp = DateTime.Now
        };
        Messaggi.Add(bubbleAI);

        IsStreaming = true;
        ErrorMessage = string.Empty;

        _streamCts = new CancellationTokenSource();
        var sb = new System.Text.StringBuilder();

        try
        {
            var stream = await _mediator.Send(
                new SendMessaggioAICommand(
                    _sessioneCorrenteId.Value,
                    testo,
                    TipoSelezionato),
                _streamCts.Token);

            bubbleAI.IsLoading = false;

            await foreach (var chunk in stream
                .WithCancellation(_streamCts.Token))
            {
                sb.Append(chunk);
                bubbleAI.Testo = sb.ToString();
            }

            // Persisti la risposta completa
            await _mediator.Send(new SaveRispostaAICommand(
                _sessioneCorrenteId.Value,
                sb.ToString()));
        }
        catch (OperationCanceledException)
        {
            bubbleAI.Testo = sb.Length > 0
                ? sb.ToString() + "\n\n[interrotto]"
                : "[messaggio interrotto]";
            bubbleAI.IsLoading = false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore streaming AI");
            bubbleAI.Testo = $"Errore: {ex.Message}";
            bubbleAI.IsLoading = false;
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsStreaming = false;
            _streamCts?.Dispose();
            _streamCts = null;
        }
    }

    [RelayCommand]
    private void InterrompiStreaming()
    {
        _streamCts?.Cancel();
    }

    [RelayCommand]
    private void UsaPromptRapido(string prompt)
    {
        InputUtente = prompt;
        OnPropertyChanged(nameof(CanSend));
    }

    [RelayCommand]
    private void CambiaTipo(TipoRicercaAI tipo)
    {
        TipoSelezionato = tipo;
        OnPropertyChanged(nameof(PromptRapidi));
    }
}