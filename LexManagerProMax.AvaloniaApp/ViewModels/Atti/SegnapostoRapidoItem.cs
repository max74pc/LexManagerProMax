namespace LexManagerProMax.AvaloniaApp.ViewModels.Atti
{
    public class SegnapostoRapidoItem
    {
        /// <summary>Testo mostrato sul chip button, es. "NOME_CLIENTE"</summary>
        public string Label { get; init; } = default!;

        /// <summary>Testo inserito nell'editor, es. "{{NOME_CLIENTE}}"</summary>
        public string Valore { get; init; } = default!;
    }
}
