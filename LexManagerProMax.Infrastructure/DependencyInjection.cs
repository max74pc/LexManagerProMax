using LexManagerProMax.Application.Interfaces;
using LexManagerProMax.Infrastructure.Persistence;
using LexManagerProMax.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace LexManagerProMax.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database — SQL Server LocalDB
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? @"Server=(localdb)\MSSQLLocalDB;Database=LexManagerDB;Trusted_Connection=true;MultipleActiveResultSets=true";

        services.AddDbContext<LexManagerProMaxDbContext>(options =>
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(3);
                sqlOptions.CommandTimeout(30);
            }));

        // Repositories
        services.AddScoped<ISoggettoRepository, SoggettoRepository>();
        //services.AddScoped<IFascicoloRepository, FascicoloRepository>();   //TODO
        //services.AddScoped<IModelloAttoRepository, ModelloAttoRepository>();  //TODO
        //services.AddScoped<ISessioneAIRepository, SessioneAIRepository>();  //TODO

        // Services
        //services.AddScoped<IAIService, OpenAIService>();  //TODO
        //services.AddScoped<IPdfService, PdfService>();  //TODO
        //services.AddScoped<ICalcolatoreScadenzeService, CalcolatoreScadenzeService>();  //TODO

        return services;
    }

    public static IServiceCollection AddSerilogLogging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "LexManager")
            .WriteTo.File(
                path: "logs/lexmanager-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
            .WriteTo.Seq(
                serverUrl: configuration["Seq:ServerUrl"] ?? "http://localhost:5341")
            .CreateLogger();

        services.AddLogging(loggingBuilder =>
            loggingBuilder.AddSerilog(dispose: true));

        return services;
    }
}
