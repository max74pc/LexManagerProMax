using Avalonia.Controls;
using LexManagerProMax.AvaloniaApp.ViewModels.Anagrafica;

namespace LexManagerProMax.AvaloniaApp.Views.Anagrafica;

public partial class AnagraficaView : UserControl
{
    public AnagraficaView()
    {
        InitializeComponent();

        // Quando il DataContext viene assegnato, lancia il caricamento
        DataContextChanged += async (_, _) =>
        {
            if (DataContext is AnagraficaViewModel vm)
                await vm.InitializeAsync();
        };
    }
}