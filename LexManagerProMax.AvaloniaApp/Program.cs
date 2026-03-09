using Avalonia;
using LexManagerProMax.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Threading.Tasks;

namespace LexManagerProMax.AvaloniaApp;

class Program
{
    [STAThread]
    static async Task Main(string[] args)
    {
        // Inizializza Serilog prima di tutto
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        try
        {
            Log.Information("Avvio LexManager...");

            // Esegui le migration automaticamente all'avvio
            await RunMigrationsAsync();

            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Errore critico all'avvio di LexManager");
            throw;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static async Task RunMigrationsAsync()
    {
        var sp = AppBootstrapper.BuildServiceProvider();
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LexManagerProMaxDbContext>();

        Log.Information("Applicazione migration database...");
        await db.Database.MigrateAsync();
        Log.Information("Database pronto.");
    }

    static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();

    
}
