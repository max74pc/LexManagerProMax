using LexManagerProMax.Domain.Entities.AI;
using System;

namespace LexManagerProMax.AvaloniaApp.ViewModels.AI;

public class SessioneAIListItem
{
    public Guid Id { get; init; }
    public string Titolo { get; init; } = default!;
    public TipoRicercaAI Tipo { get; init; }
    public int NumMsg { get; init; }
    public DateTime Aggiornata { get; init; }

    public string TipoLabel => Tipo.ToString();
    public string TipoIcon => Tipo switch
    {
        TipoRicercaAI.Assistenza => "🤖",
        TipoRicercaAI.Giurisprudenza => "⚖",
        TipoRicercaAI.Dottrina => "📚",
        TipoRicercaAI.Normativa => "📜",
        _ => "💬"
    };
    public string TipoColor => Tipo switch
    {
        TipoRicercaAI.Assistenza => "#DBEAFE",
        TipoRicercaAI.Giurisprudenza => "#FCE7F3",
        TipoRicercaAI.Dottrina => "#DCFCE7",
        TipoRicercaAI.Normativa => "#FEF9C3",
        _ => "#F3F4F6"
    };
    public string DataLabel => Aggiornata.ToString("dd/MM HH:mm");
    public string NumMsgLabel => NumMsg == 1 ? "1 msg" : $"{NumMsg} msg";

    public static SessioneAIListItem FromDomain(SessioneAI s) => new()
    {
        Id = s.Id,
        Titolo = s.Titolo,
        Tipo = s.TipoRicerca,
        NumMsg = s.Messaggi?.Count ?? 0,
        Aggiornata = s.UpdatedAt ?? s.CreatedAt,
    };
}