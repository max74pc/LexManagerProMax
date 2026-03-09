using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LexManagerProMax.AvaloniaApp.ViewModels.AI;
using LexManagerProMax.AvaloniaApp.ViewModels.Anagrafica;
using LexManagerProMax.AvaloniaApp.ViewModels.Atti;
using LexManagerProMax.AvaloniaApp.ViewModels.Dashboard;
using LexManagerProMax.AvaloniaApp.Views.AI;
using LexManagerProMax.AvaloniaApp.Views.Anagrafica;
using LexManagerProMax.AvaloniaApp.Views.Dashboard;
using LexManagerProMax.AvaloniaApp.Views.ModelloAtto;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;

namespace LexManagerProMax.AvaloniaApp.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly IServiceProvider _sp;
    private readonly DispatcherTimer _clockTimer;

    [ObservableProperty] private object? _currentView;
    [ObservableProperty] private bool _isSidebarExpanded = true;
    [ObservableProperty] private bool _isRightPanelVisible = false;
    [ObservableProperty] private bool _isStatusBarVisible = true;
    [ObservableProperty] private string _statusMessage = "Pronto";
    [ObservableProperty] private string _dbStatus = "● DB OK";
    [ObservableProperty] private string _currentDateTime = DateTime.Now.ToString("ddd dd/MM/yyyy  HH:mm");
    [ObservableProperty] private string _utenteCorrente = "Avv. Giovanna Turchio";
    [ObservableProperty] private string _rightPanelTitle = "Dettaglio";
    [ObservableProperty] private object? _rightPanelContent;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private bool _isIndeterminate = true;
    [ObservableProperty] private double _busyProgress;
    [ObservableProperty] private string _sidebarSearchText = string.Empty;

    public double SidebarWidth => IsSidebarExpanded ? 220 : 52;
    public double RightPanelWidth => IsRightPanelVisible ? 320 : 0;

    public ObservableCollection<NavItem> NavigationItems { get; } =
    [
        new("anagrafica", "👥", "Anagrafica"),
        new("atti",       "📄", "Modelli Atti"),
        new("ai",         "🤖", "Assistente AI"),
        new("dashboard",  "🏠", "Dashboard"),
        //new("fascicoli",   "📁", "Fascicoli"),
        //new("scadenzario", "📅", "Scadenzario"),
        //new("parcelle",    "💶", "Parcelle"),
        //new("utilita",     "🔧", "Utilità"),
    ];

    // ─────────────────────────────────────────────────────────────
    // Costruttore
    // ─────────────────────────────────────────────────────────────

    public MainWindowViewModel(IServiceProvider sp)
    {
        _sp = sp;

        _clockTimer = new DispatcherTimer
        { Interval = TimeSpan.FromSeconds(30) };
        _clockTimer.Tick += (_, _) =>
            CurrentDateTime = DateTime.Now.ToString("ddd dd/MM/yyyy  HH:mm");
        _clockTimer.Start();

        NavigateToModule("dashboard");
    }

    // ─────────────────────────────────────────────────────────────
    // Navigazione — crea la View e le assegna il ViewModel dal DI
    // ─────────────────────────────────────────────────────────────

    [RelayCommand]
    private void NavigateToModule(string moduleKey)
    {
        StatusMessage = $"Caricamento {moduleKey}...";

        CurrentView = moduleKey switch
        {
           "anagrafica" => CreateView<AnagraficaView, AnagraficaViewModel>(),
            "atti" => CreateView<ModelliAttiView, ModelliAttiViewModel>(),
            "ai" => CreateView<AIAssistantView, AIAssistantViewModel>(),
            "dashboard" => CreateView<DashboardView, DashboardViewModel>(),
            // "fascicoli"  => CreateView<FascicoliView, FascicoliViewModel>(),
            // "utilita"    => CreateView<UtilitaView,   UtilitaViewModel>(),
            _ => throw new NotImplementedException(
                     $"Modulo '{moduleKey}' non implementato.")
        };

        StatusMessage = $"Modulo {moduleKey} caricato";
    }

    /// <summary>
    /// Crea la View, risolve il ViewModel dal DI e lo assegna come DataContext.
    /// </summary>
    private TView CreateView<TView, TViewModel>()
        where TView : Avalonia.Controls.UserControl, new()
        where TViewModel : class
    {
        var view = new TView();
        var viewModel = _sp.GetRequiredService<TViewModel>();
        view.DataContext = viewModel;

        // DEBUG — rimuovere dopo
        System.Diagnostics.Debug.WriteLine(
            $"[Nav] Creata {typeof(TView).Name} con VM {typeof(TViewModel).Name}, " +
            $"DataContext null? {view.DataContext is null}");
        return view;
    }

    // ─────────────────────────────────────────────────────────────
    // Commands
    // ─────────────────────────────────────────────────────────────

    [RelayCommand]
    private void NavigateTo(string key) => NavigateToModule(key);

    [RelayCommand]
    private void ToggleSidebar()
    {
        IsSidebarExpanded = !IsSidebarExpanded;
        OnPropertyChanged(nameof(SidebarWidth));
    }

    [RelayCommand]
    private void ToggleRightPanel()
    {
        IsRightPanelVisible = !IsRightPanelVisible;
        OnPropertyChanged(nameof(RightPanelWidth));
    }

    [RelayCommand]
    private void ToggleStatusBar() => IsStatusBarVisible = !IsStatusBarVisible;

    [RelayCommand]
    private void NuovoFascicolo() => NavigateToModule("fascicoli");

    [RelayCommand]
    private void NuovoSoggetto() => NavigateToModule("anagrafica");

    [RelayCommand]
    private static void ApriImpostazioni() { /* futuro */ }

    [RelayCommand]
    private static void Esci() => Environment.Exit(0);

    // ─────────────────────────────────────────────────────────────
    // Pannello destro
    // ─────────────────────────────────────────────────────────────

    public void ShowRightPanel(string title, object content)
    {
        RightPanelTitle = title;
        RightPanelContent = content;
        IsRightPanelVisible = true;
        OnPropertyChanged(nameof(RightPanelWidth));
    }
}

public record NavItem(string ModuleKey, string Icon, string Label);