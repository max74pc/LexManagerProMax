using CommunityToolkit.Mvvm.ComponentModel;

namespace LexManagerProMax.AvaloniaApp.ViewModels.Atti;

/// <summary>
/// Rappresenta un singolo segnaposto {{CHIAVE}} con il valore che l'utente vuole assegnargli.
/// </summary>
public partial class SegnapostoItem : ObservableObject
{
    public string Chiave { get; init; } = default!;
    public string Descrizione { get; init; } = string.Empty;
    public string Placeholder { get; init; } = string.Empty;

    [ObservableProperty]
    private string _valore = string.Empty;
}
