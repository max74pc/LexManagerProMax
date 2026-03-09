using LexManagerProMax.Application.Interfaces;
using LexManagerProMax.Application.UseCases.Anagrafica;
using LexManagerProMax.AvaloniaApp.ViewModels;
using LexManagerProMax.AvaloniaApp.ViewModels.AI;
using LexManagerProMax.AvaloniaApp.ViewModels.Anagrafica;
using LexManagerProMax.AvaloniaApp.ViewModels.Atti;
using LexManagerProMax.AvaloniaApp.ViewModels.Dashboard;
using LexManagerProMax.Infrastructure;
using LexManagerProMax.Infrastructure.Repositories;
using LexManagerProMax.Infrastructure.Services;

/// TODO
//using LexManagerProMax.AvaloniaApp.ViewModels.Utilita;

using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace LexManagerProMax.AvaloniaApp;

public static class AppBootstrapper
{
    public static IServiceProvider BuildServiceProvider()
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.local.json", optional: true)
            .Build();

        var services = new ServiceCollection();

        // Infrastructure
        services.AddInfrastructure(config);
        services.AddSerilogLogging(config);

        // MediatR
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(GetSoggettiQueryHandler).Assembly));

        // ViewModels
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<AnagraficaViewModel>();
        services.AddTransient<SoggettoDetailViewModel>();
        services.AddTransient<ModelliAttiViewModel>();
        services.AddTransient<AIAssistantViewModel>();
        services.AddTransient<DashboardViewModel>();
        //services.AddTransient<FascicoliViewModel>(); //TODO 
        //services.AddTransient<UtilitaViewModel>();  //TODO 
        //services.AddTransient<StatusBarViewModel>();  //TODO 


        // Repository
        services.AddScoped<ISessioneAIRepository, SessioneAIRepository>();

        // Registra il servizio AI come Scoped
        services.AddScoped<IAIService, OpenAIService>();


        // Il DbContext deve avere il DbSet:
        // public DbSet<SessioneAI> SessioniAI => Set<SessioneAI>();

        return services.BuildServiceProvider();
    }
}
