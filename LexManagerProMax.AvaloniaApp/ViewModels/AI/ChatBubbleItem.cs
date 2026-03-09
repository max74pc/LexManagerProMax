using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace LexManagerProMax.AvaloniaApp.ViewModels.AI;

/// <summary>
/// Rappresenta una singola bolla nella chat.
/// IsLoading = true mostra l'animazione di attesa mentre lo stream arriva.
/// Testo viene aggiornato chunk per chunk durante lo streaming.
/// </summary>
public partial class ChatBubbleItem : ObservableObject
{
    private string _testo = string.Empty;
    public string Testo
    {
        get => _testo;
        set => SetProperty(ref _testo, value);
    }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public bool IsUtente { get; init; }
    public DateTime Timestamp { get; init; }

    // Helpers per la view
    public bool IsAssistente => !IsUtente;
    public string TimestampLabel => Timestamp.ToString("HH:mm");
    public string AlignmentLabel => IsUtente ? "Right" : "Left";
    public string BubbleBackground => IsUtente ? "#2563EB" : "#FFFFFF";
    public string TextColor => IsUtente ? "#FFFFFF" : "#1F2937";
}