using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LexManagerProMax.Application.UseCases.Anagrafica;
using LexManagerProMax.Application.UseCases.Atti;
using LexManagerProMax.Domain.Entities.Anagrafica;
using MediatR;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace LexManagerProMax.AvaloniaApp.ViewModels.Anagrafica;

public partial class AnagraficaViewModel : ObservableObject
{
    private readonly IMediator _mediator;

    [ObservableProperty] private ObservableCollection<Soggetto> _soggetti = [];
    [ObservableProperty] private Soggetto? _soggettoSelezionato;
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isDetailOpen;
    [ObservableProperty] private SoggettoDetailViewModel? _detailViewModel;

    public AnagraficaViewModel(IMediator mediator)
    {
        _mediator = mediator;
        //_ = LoadAsync();
    }

    public async Task InitializeAsync()
    {
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        IsLoading = true;
        try
        {
            var result = await _mediator.Send(new GetSoggettiQuery());
            // Aggiungi questo per debug
            System.Diagnostics.Debug.WriteLine($"[Anagrafica] Soggetti caricati: {result.Count}");
            Soggetti = new ObservableCollection<Soggetto>(result);
            System.Diagnostics.Debug.WriteLine($"[Anagrafica] Collection size: {Soggetti.Count}");
            System.Diagnostics.Debug.WriteLine(
    $"[Anagrafica] LoadAsync chiamato, thread: {System.Threading.Thread.CurrentThread.ManagedThreadId}");
            Soggetti = new ObservableCollection<Soggetto>(result);
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private async Task Search()
    {
        IsLoading = true;
        try
        {
            var result = await _mediator.Send(new GetSoggettiQuery(SearchText));
            Soggetti = new ObservableCollection<Soggetto>(result);
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private void NuovoSoggetto()
    {
        DetailViewModel = new SoggettoDetailViewModel(_mediator, null);
        IsDetailOpen = true;
    }

    [RelayCommand]
    private void ApriDettaglio(Soggetto soggetto)
    {
        SoggettoSelezionato = soggetto;
        DetailViewModel = new SoggettoDetailViewModel(_mediator, soggetto);
        IsDetailOpen = true;
    }

    [RelayCommand]
    private void ChiudiDettaglio() => IsDetailOpen = false;

    partial void OnSearchTextChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            _ = LoadAsync();
    }

    [RelayCommand]
    private void InserisciTesto(string valore)
    {
        if (string.IsNullOrWhiteSpace(valore))
            return;

        // Esempio: aggiunge il testo al campo di ricerca
        SearchText = (SearchText ?? string.Empty) + valore;
    }


}
