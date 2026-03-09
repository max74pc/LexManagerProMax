using LexManagerProMax.Domain.Entities.AI;
using System.Collections.Generic;

namespace LexManagerProMax.AvaloniaApp.ViewModels.AI;

/// <summary>
/// Prompt rapidi mostrati come chip button nella toolbar.
/// Le stringhe vivono qui in C#, mai nell'AXAML.
/// </summary>
public class PromptRapidoItem
{
    public string Label { get; init; } = default!;
    public string Prompt { get; init; } = default!;

    public static IReadOnlyList<PromptRapidoItem> GetByTipo(TipoRicercaAI tipo) =>
        tipo switch
        {
            TipoRicercaAI.Assistenza =>
            [
                new() { Label = "Riassumi",    Prompt = "Riassumi il caso brevemente." },
                new() { Label = "Strategia",   Prompt = "Suggerisci una strategia difensiva." },
                new() { Label = "Scadenze",    Prompt = "Quali sono le scadenze processuali principali?" },
                new() { Label = "Schema atto", Prompt = "Crea uno schema per l'atto da redigere." },
            ],
            TipoRicercaAI.Giurisprudenza =>
            [
                new() { Label = "Sentenza tipo",  Prompt = "Cerca giurisprudenza rilevante sul punto." },
                new() { Label = "Cassazione",     Prompt = "Quali pronunce della Cassazione riguardano questo tema?" },
                new() { Label = "Orientamenti",   Prompt = "Quali sono gli orientamenti giurisprudenziali prevalenti?" },
                new() { Label = "Contrasto",      Prompt = "Esiste contrasto giurisprudenziale? Esponi le posizioni." },
            ],
            TipoRicercaAI.Dottrina =>
            [
                new() { Label = "Autori",       Prompt = "Quali autori principali trattano questo argomento?" },
                new() { Label = "Teorie",       Prompt = "Esponi le teorie dottrinali prevalenti." },
                new() { Label = "Manuale",      Prompt = "Indica i manuali di riferimento per questo tema." },
                new() { Label = "Critica",      Prompt = "Quali critiche dottrinali esistono all'orientamento dominante?" },
            ],
            TipoRicercaAI.Normativa =>
            [
                new() { Label = "Norma",        Prompt = "Qual è la normativa applicabile al caso?" },
                new() { Label = "Aggiornamenti",Prompt = "Ci sono aggiornamenti normativi recenti?" },
                new() { Label = "UE",           Prompt = "Esiste normativa europea applicabile?" },
                new() { Label = "Sanzioni",     Prompt = "Quali sanzioni prevede la normativa vigente?" },
            ],
            _ => []
        };
}