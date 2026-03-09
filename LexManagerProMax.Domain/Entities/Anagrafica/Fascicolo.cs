using LexManagerProMax.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace LexManagerProMax.Domain.Entities.Anagrafica
{
    public enum StatoFascicolo { Aperto, Sospeso, Chiuso, Archiviato }
    public enum RitoProcessuale
    {
        Civile, Penale, Amministrativo, Tributario, Lavoro,
        Altro
    }

    public sealed class Fascicolo : BaseEntity
    {
        public string Numero { get; private set; } = default!;
        public string Oggetto { get; private set; } = default!;
        public StatoFascicolo Stato { get; private set; } = StatoFascicolo.Aperto; public RitoProcessuale Rito { get; private set; }
        public DateTime DataApertura { get; private set; } = DateTime.UtcNow;
        public DateTime? DataChiusura { get; private set; }

        // Tribunale/Giudice    
        public string? Tribunale { get; private set; }
        public string? NrRG { get; private set; }
        public string? Giudice { get; private set; }

        // Relazioni    
        public Guid ClienteId { get; private set; }
        public Soggetto Cliente { get; private set; } = default!;
        private readonly List<FascicoloSoggetto> _controparti = [];
        public IReadOnlyList<FascicoloSoggetto> Controparti => _controparti.AsReadOnly();

        public string? Note { get; private set; }

        private Fascicolo() { }

        public static Fascicolo Crea(
            string numero,
            string oggetto,
            Guid clienteId,
            RitoProcessuale rito,
            string? tribunale = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(numero); ArgumentException.ThrowIfNullOrWhiteSpace(oggetto);

            return new Fascicolo
            {
                Numero = numero,
                Oggetto = oggetto,
                ClienteId = clienteId,
                Rito = rito,
                Tribunale = tribunale
            };
        }

        public void AggiungiControparte(Guid soggettoId, string ruolo)
        {
            if (_controparti.Any(c => c.SoggettoId == soggettoId)) return; _controparti.Add(new FascicoloSoggetto
            {
                SoggettoId = soggettoId,
                Ruolo =
     ruolo,
                FascicoloId = Id
            });
        }

        public void Chiudi(string? note = null)
        {
            Stato = StatoFascicolo.Chiuso;
            DataChiusura = DateTime.UtcNow;
            Note = note;
            MarkAsUpdated();
        }
    }

    public class FascicoloSoggetto : BaseEntity
    {
        public Guid FascicoloId { get; set; }
        public Guid SoggettoId { get; set; }
        public string Ruolo { get; set; } = default!;
        public Fascicolo Fascicolo { get; set; } = default!;
        public Soggetto Soggetto { get; set; } = default!;
    }
}
