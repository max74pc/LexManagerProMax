using Avalonia.Controls;
using Avalonia.Threading;
using LexManagerProMax.AvaloniaApp.ViewModels.AI;

namespace LexManagerProMax.AvaloniaApp.Views.AI;

public partial class AIAssistantView : UserControl
{
    public AIAssistantView()
    {
        InitializeComponent();

        // Auto-scroll alla fine dei messaggi quando ne arrivano di nuovi
        if (DataContext is AIAssistantViewModel vm)
            AttachScrollBehavior(vm);

        DataContextChanged += (_, _) =>
        {
            if (DataContext is AIAssistantViewModel newVm)
                AttachScrollBehavior(newVm);
        };
    }

    private void AttachScrollBehavior(AIAssistantViewModel vm)
    {
        vm.Messaggi.CollectionChanged += (_, _) => ScrollToBottom();

        // Scroll anche quando il testo di una bolla cambia (streaming)
        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(vm.Messaggi))
                ScrollToBottom();
        };
    }

    private void ScrollToBottom()
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (this.FindControl<ScrollViewer>("ScrollMessages") is { } sv)
                sv.ScrollToEnd();
        }, DispatcherPriority.Background);
    }
}