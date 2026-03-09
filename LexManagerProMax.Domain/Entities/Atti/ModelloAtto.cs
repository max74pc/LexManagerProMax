using LexManagerProMax.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace LexManagerProMax.Domain.Entities.Atti
{
    public enum CategoriaAtto
    {
        Contratto, Ricorso, Atto_Citazione, Comparsa,
        Memoria, Istanza, Procura, Verbale, Altro
    }

    public sealed class ModelloAtto : BaseEntity
    {
        public string Nome { get; private set; } = default!;
        public string? Descrizione { get; private set; }
        public CategoriaAtto Categoria { get; private set; }
        public string Contenuto { get; private set; } = default!;  // RTF / HTML con segnaposto {{NOME}}, {{DATA}} ecc.
        public string? Tags { get; private set; }
        public int VersioneCorrente { get; private set; } = 1;
        public bool IsAttivo { get; private set; } = true;

        private readonly List<VersioneModello> _versioni = [];
        public IReadOnlyList<VersioneModello> Versioni => _versioni.AsReadOnly();

        private ModelloAtto() { }

        public static ModelloAtto Crea(string nome, CategoriaAtto categoria, string contenuto, string? descrizione = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(nome);
            ArgumentException.ThrowIfNullOrWhiteSpace(contenuto);

            var modello = new ModelloAtto
            {
                Nome = nome,
                Categoria = categoria,
                Contenuto = contenuto,
                Descrizione = descrizione
            };
            modello._versioni.Add(new VersioneModello
            {
                ModelloAttoId = modello.Id,
                Versione = 1,
                Contenuto = contenuto,
                DataCreazione = DateTime.UtcNow
            });
            return modello;
        }

        public void AggiornaContenuto(string nuovoContenuto, string? motivazione = null)
        {
            VersioneCorrente++;
            Contenuto = nuovoContenuto;
            _versioni.Add(new VersioneModello
            {
                ModelloAttoId = Id,
                Versione = VersioneCorrente,
                Contenuto = nuovoContenuto,
                Note = motivazione,
                DataCreazione = DateTime.UtcNow
            });
            MarkAsUpdated();
        }

        /// <summary>Sostituisce i segnaposto con i valori forniti</summary>
        public string GeneraDocumento(Dictionary<string, string> segnaposto)
        {
            var result = Contenuto;
            foreach (var (chiave, valore) in segnaposto)
                result = result.Replace($"{{{{{chiave}}}}}", valore);
            return result;
        }
    }

    public class VersioneModello
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ModelloAttoId { get; set; }
        public int Versione { get; set; }
        public string Contenuto { get; set; } = default!;
        public string? Note { get; set; }
        public DateTime DataCreazione { get; set; }
    }
}
