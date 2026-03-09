using LexManagerProMax.Domain.Entities.Atti;
using System;

namespace LexManagerProMax.AvaloniaApp.ViewModels.Atti;

/// <summary>
/// DTO leggero per la visualizzazione nella lista laterale.
/// Separato dal Domain per non esporre l'entità direttamente alla UI.
/// </summary>
public class ModelloAttoListItem
{
    public Guid Id { get; init; }
    public string Nome { get; init; } = default!;
    public string Contenuto { get; init; } = default!;
    public CategoriaAtto CategoriaEnum { get; init; }
    public string Categoria => CategoriaEnum.ToString().Replace("_", " ");
    public string? Tags { get; init; }
    public int Versione { get; init; }
    public string VersioneLabel => $"v{Versione}";
    public bool IsAttivo { get; init; }
    public string IsAttivoColor => IsAttivo ? "#22C55E" : "#D1D5DB";

    public string CategoriaIcon => CategoriaEnum switch
    {
        CategoriaAtto.Contratto => "📝",
        CategoriaAtto.Ricorso => "⚖",
        CategoriaAtto.Atto_Citazione => "📋",
        CategoriaAtto.Comparsa => "📄",
        CategoriaAtto.Memoria => "📑",
        CategoriaAtto.Istanza => "📩",
        CategoriaAtto.Procura => "🖊",
        CategoriaAtto.Verbale => "📰",
        _ => "📄"
    };

    public string CategoriaColor => CategoriaEnum switch
    {
        CategoriaAtto.Contratto => "#DBEAFE",
        CategoriaAtto.Ricorso => "#FCE7F3",
        CategoriaAtto.Atto_Citazione => "#FEF9C3",
        CategoriaAtto.Comparsa => "#DCFCE7",
        CategoriaAtto.Memoria => "#EDE9FE",
        CategoriaAtto.Istanza => "#FFEDD5",
        CategoriaAtto.Procura => "#F0FDF4",
        CategoriaAtto.Verbale => "#F0F9FF",
        _ => "#F3F4F6"
    };

    public static ModelloAttoListItem FromDomain(ModelloAtto m) => new()
    {
        Id = m.Id,
        Nome = m.Nome,
        Contenuto = m.Contenuto,
        CategoriaEnum = m.Categoria,
        Tags = m.Tags,
        Versione = m.VersioneCorrente,
        IsAttivo = m.IsAttivo,
    };
}

