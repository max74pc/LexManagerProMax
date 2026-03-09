using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using LexManagerProMax.AvaloniaApp.ViewModels;
using LexManagerProMax.AvaloniaApp.Views;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace LexManagerProMax.AvaloniaApp
{
    public partial class App : Avalonia.Application
    {
         public static IServiceProvider Services { get; private set; } = default!;

        public override void Initialize() => AvaloniaXamlLoader.Load(this);

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var sp = AppBootstrapper.BuildServiceProvider();
                Services = sp;

                desktop.MainWindow = new MainWindow
                {
                    DataContext = sp.GetRequiredService<MainWindowViewModel>()
                };
            }
            base.OnFrameworkInitializationCompleted();
        }
    }
}

