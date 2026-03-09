using System;

namespace LexManagerProMax.AvaloniaApp.ViewModels.Dashboard;

public enum TipoAttivita
{
    Info,
    Pratica,
    Cliente,
    Scadenza,
    Documento,
    AI
}

public class AttivitaRecenteItem
{
    public string Icona { get; init; } = "📋";
    public string Descrizione { get; init; } = default!;
    public string Dettaglio { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
    public TipoAttivita Tipo { get; init; }

    public string TimestampLabel => Timestamp.Date == DateTime.Today
        ? Timestamp.ToString("HH:mm")
        : Timestamp.ToString("dd/MM HH:mm");

    public string ColoreIcona => Tipo switch
    {
        TipoAttivita.Pratica => "#DBEAFE",
        TipoAttivita.Cliente => "#DCFCE7",
        TipoAttivita.Scadenza => "#FEF9C3",
        TipoAttivita.Documento => "#F3E8FF",
        TipoAttivita.AI => "#FCE7F3",
        _ => "#F3F4F6"
    };
}