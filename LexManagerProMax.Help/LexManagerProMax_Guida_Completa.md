# LexManager — Gestionale Moderno per Avvocati
## Guida Completa Step-by-Step con Codice Sorgente

---

# INDICE

1. [Struttura del Progetto](#1-struttura-del-progetto)
2. [Setup Iniziale e Prerequisiti](#2-setup-iniziale-e-prerequisiti)
3. [Solution e Progetti .NET](#3-solution-e-progetti-net)
4. [Domain Layer](#4-domain-layer)
5. [Infrastructure Layer (EF Core + SeriLog)](#5-infrastructure-layer)
6. [Application Layer (CQRS / Services)](#6-application-layer)
7. [Frontend Avalonia — Shell a 5 Zone](#7-frontend-avalonia)
8. [Modulo Anagrafica](#8-modulo-anagrafica)
9. [Modulo Gestione Modelli di Atti](#9-modulo-gestione-modelli-di-atti)
10. [Modulo AI — Ricerca e Assistenza](#10-modulo-ai)
11. [Modulo Utilità e Strumenti Professionali](#11-modulo-utilità)
12. [Configurazione SEQ + Serilog](#12-serilog--seq)
13. [Migrations e Avvio](#13-migrations-e-avvio)
14. [Estendere con Nuovi Moduli](#14-estendere-con-nuovi-moduli)

---

# 1. STRUTTURA DEL PROGETTO

```
LexManager/
├── LexManager.sln
├── src/
│   ├── LexManager.Domain/           # Entità, Value Objects, Domain Events
│   ├── LexManager.Application/      # Use Cases, DTOs, Interfaces
│   ├── LexManager.Infrastructure/   # EF Core, Repos, AI Client, Serilog
│   └── LexManager.UI/               # Avalonia MVVM Desktop App
├── tests/
│   ├── LexManager.Domain.Tests/
│   └── LexManager.Application.Tests/
├── docker/
│   └── seq/                         # docker-compose per SEQ
└── docs/
    └── architecture.md
```

---

# 2. SETUP INIZIALE E PREREQUISITI

## 2.1 Requisiti

```
- Windows 11
- .NET 10 SDK (https://dotnet.microsoft.com/download)
- Visual Studio 2022 17.12+ o Rider 2024.3+
- SQL Server LocalDB (installato con VS o standalone)
- Docker Desktop (opzionale, per SEQ)
- Node.js (non richiesto — tutto è C#)
```

## 2.2 Installa Prerequisiti via PowerShell

```powershell
# Installa .NET 10 (winget)
winget install Microsoft.DotNet.SDK.10

# Verifica LocalDB
sqllocaldb info
sqllocaldb create "LexManagerDB" 15.0 -s

# SEQ via Docker (log viewer)
docker run --name seq -d --restart unless-stopped `
  -e ACCEPT_EULA=Y `
  -p 5341:5341 -p 80:80 `
  datalust/seq:latest
```

## 2.3 Crea la Solution

```powershell
mkdir LexManager && cd LexManager

dotnet new sln -n LexManager

# Crea i progetti
dotnet new classlib -n LexManager.Domain       -f net10.0 -o src/LexManager.Domain
dotnet new classlib -n LexManager.Application  -f net10.0 -o src/LexManager.Application
dotnet new classlib -n LexManager.Infrastructure -f net10.0 -o src/LexManager.Infrastructure
dotnet new avalonia.mvvm -n LexManager.UI      -f net10.0 -o src/LexManager.UI

# Aggiungi alla solution
dotnet sln add src/LexManager.Domain/LexManager.Domain.csproj
dotnet sln add src/LexManager.Application/LexManager.Application.csproj
dotnet sln add src/LexManager.Infrastructure/LexManager.Infrastructure.csproj
dotnet sln add src/LexManager.UI/LexManager.UI.csproj

# Aggiungi riferimenti
dotnet add src/LexManager.Application/LexManager.Application.csproj reference src/LexManager.Domain/LexManager.Domain.csproj
dotnet add src/LexManager.Infrastructure/LexManager.Infrastructure.csproj reference src/LexManager.Application/LexManager.Application.csproj
dotnet add src/LexManager.UI/LexManager.UI.csproj reference src/LexManager.Application/LexManager.Application.csproj
dotnet add src/LexManager.UI/LexManager.UI.csproj reference src/LexManager.Infrastructure/LexManager.Infrastructure.csproj
```

---

# 3. SOLUTION E PROGETTI .NET

## 3.1 LexManager.Domain.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <LangVersion>14</LangVersion>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
</Project>
```

## 3.2 LexManager.Application.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <LangVersion>14</LangVersion>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="MediatR" Version="12.*" />
    <PackageReference Include="FluentValidation" Version="11.*" />
    <PackageReference Include="AutoMapper" Version="13.*" />
  </ItemGroup>
</Project>
```

## 3.3 LexManager.Infrastructure.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <LangVersion>14</LangVersion>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="10.*" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="10.*" />
    <PackageReference Include="Serilog" Version="4.*" />
    <PackageReference Include="Serilog.Sinks.Seq" Version="8.*" />
    <PackageReference Include="Serilog.Sinks.File" Version="6.*" />
    <PackageReference Include="Serilog.Extensions.Hosting" Version="8.*" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="10.*" />
    <PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="10.*" />
  </ItemGroup>
</Project>
```

## 3.4 LexManager.UI.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net10.0-windows</TargetFramework>
    <Nullable>enable</Nullable>
    <LangVersion>14</LangVersion>
    <ImplicitUsings>enable</ImplicitUsings>
    <ApplicationIcon>Assets\lexmanager.ico</ApplicationIcon>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Avalonia" Version="11.*" />
    <PackageReference Include="Avalonia.Desktop" Version="11.*" />
    <PackageReference Include="Avalonia.Themes.Fluent" Version="11.*" />
    <PackageReference Include="Avalonia.ReactiveUI" Version="11.*" />
    <PackageReference Include="CommunityToolkit.Mvvm" Version="8.*" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="10.*" />
  </ItemGroup>
</Project>
```

---

# 4. DOMAIN LAYER

## 4.1 Base Entity — `src/LexManager.Domain/Common/BaseEntity.cs`

```csharp
namespace LexManager.Domain.Common;

public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; protected set; }
    public bool IsDeleted { get; protected set; }

    public void MarkAsDeleted() => IsDeleted = true;
    public void MarkAsUpdated() => UpdatedAt = DateTime.UtcNow;
}
```

## 4.2 Anagrafica — `src/LexManager.Domain/Entities/Anagrafica/`

### `Soggetto.cs` (entità base per persone fisiche e giuridiche)

```csharp
namespace LexManager.Domain.Entities.Anagrafica;

public enum TipoSoggetto { PersonaFisica, PersonaGiuridica, Ente }

public sealed class Soggetto : BaseEntity
{
    public TipoSoggetto Tipo { get; private set; }

    // Persona fisica
    public string? Cognome { get; private set; }
    public string? Nome { get; private set; }
    public string? CodiceFiscale { get; private set; }
    public DateOnly? DataNascita { get; private set; }
    public string? LuogoNascita { get; private set; }

    // Persona giuridica
    public string? RagioneSociale { get; private set; }
    public string? PartitaIva { get; private set; }
    public string? FormaGiuridica { get; private set; }

    // Recapiti
    public string? Email { get; private set; }
    public string? Pec { get; private set; }
    public string? Telefono { get; private set; }
    public string? CellulareUno { get; private set; }

    // Indirizzo
    public string? Via { get; private set; }
    public string? Citta { get; private set; }
    public string? Cap { get; private set; }
    public string? Provincia { get; private set; }
    public string? Nazione { get; private set; } = "Italia";

    public string? Note { get; private set; }
    public bool IsCliente { get; private set; }
    public bool IsControparte { get; private set; }

    private Soggetto() { }

    public static Soggetto CreaPersonaFisica(
        string cognome, string nome,
        string? codiceFiscale = null,
        DateOnly? dataNascita = null,
        string? email = null,
        string? pec = null,
        string? telefono = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cognome);
        ArgumentException.ThrowIfNullOrWhiteSpace(nome);

        return new Soggetto
        {
            Tipo = TipoSoggetto.PersonaFisica,
            Cognome = cognome.Trim().ToUpperInvariant(),
            Nome = nome.Trim(),
            CodiceFiscale = codiceFiscale?.Trim().ToUpperInvariant(),
            DataNascita = dataNascita,
            Email = email,
            Pec = pec,
            Telefono = telefono
        };
    }

    public static Soggetto CreaPersonaGiuridica(
        string ragioneSociale,
        string? partitaIva = null,
        string? formaGiuridica = null,
        string? email = null,
        string? pec = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ragioneSociale);

        return new Soggetto
        {
            Tipo = TipoSoggetto.PersonaGiuridica,
            RagioneSociale = ragioneSociale.Trim(),
            PartitaIva = partitaIva?.Trim(),
            FormaGiuridica = formaGiuridica,
            Email = email,
            Pec = pec
        };
    }

    public string NomeCompleto => Tipo == TipoSoggetto.PersonaFisica
        ? $"{Cognome} {Nome}"
        : RagioneSociale ?? string.Empty;

    public void AggiornaDati(
        string? email = null, string? pec = null,
        string? telefono = null, string? cellulare = null,
        string? via = null, string? citta = null,
        string? cap = null, string? provincia = null,
        string? note = null)
    {
        Email = email ?? Email;
        Pec = pec ?? Pec;
        Telefono = telefono ?? Telefono;
        CellulareUno = cellulare ?? CellulareUno;
        Via = via ?? Via;
        Citta = citta ?? Citta;
        Cap = cap ?? Cap;
        Provincia = provincia ?? Provincia;
        Note = note ?? Note;
        MarkAsUpdated();
    }

    public void ImpostaRuolo(bool isCliente, bool isControparte)
    {
        IsCliente = isCliente;
        IsControparte = isControparte;
        MarkAsUpdated();
    }
}
```

### `Fascicolo.cs`

```csharp
namespace LexManager.Domain.Entities.Anagrafica;

public enum StatoFascicolo { Aperto, Sospeso, Chiuso, Archiviato }
public enum RitoProcessuale { Civile, Penale, Amministrativo, Tributario, Lavoro, Altro }

public sealed class Fascicolo : BaseEntity
{
    public string Numero { get; private set; } = default!;
    public string Oggetto { get; private set; } = default!;
    public StatoFascicolo Stato { get; private set; } = StatoFascicolo.Aperto;
    public RitoProcessuale Rito { get; private set; }
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
        ArgumentException.ThrowIfNullOrWhiteSpace(numero);
        ArgumentException.ThrowIfNullOrWhiteSpace(oggetto);

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
        if (_controparti.Any(c => c.SoggettoId == soggettoId)) return;
        _controparti.Add(new FascicoloSoggetto { SoggettoId = soggettoId, Ruolo = ruolo, FascicoloId = Id });
    }

    public void Chiudi(string? note = null)
    {
        Stato = StatoFascicolo.Chiuso;
        DataChiusura = DateTime.UtcNow;
        Note = note;
        MarkAsUpdated();
    }
}

public class FascicoloSoggetto
{
    public Guid FascicoloId { get; set; }
    public Guid SoggettoId { get; set; }
    public string Ruolo { get; set; } = default!;
    public Fascicolo Fascicolo { get; set; } = default!;
    public Soggetto Soggetto { get; set; } = default!;
}
```

## 4.3 Modelli di Atti — `src/LexManager.Domain/Entities/Atti/`

### `ModelloAtto.cs`

```csharp
namespace LexManager.Domain.Entities.Atti;

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
```

## 4.4 AI Module Domain — `src/LexManager.Domain/Entities/AI/`

```csharp
namespace LexManager.Domain.Entities.AI;

public enum TipoRicercaAI { Dottrina, Giurisprudenza, Normativa, Assistenza }

public sealed class SessioneAI : BaseEntity
{
    public string Titolo { get; private set; } = default!;
    public TipoRicercaAI TipoRicerca { get; private set; }
    public DateTime DataSessione { get; private set; } = DateTime.UtcNow;
    public Guid? FascicoloId { get; private set; }

    private readonly List<MessaggioAI> _messaggi = [];
    public IReadOnlyList<MessaggioAI> Messaggi => _messaggi.AsReadOnly();

    private SessioneAI() { }

    public static SessioneAI Crea(string titolo, TipoRicercaAI tipo, Guid? fascicoloId = null)
        => new() { Titolo = titolo, TipoRicercaAI = tipo, FascicoloId = fascicoloId };

    public void AggiungiMessaggio(string ruolo, string contenuto)
    {
        _messaggi.Add(new MessaggioAI
        {
            SessioneAIId = Id,
            Ruolo = ruolo,
            Contenuto = contenuto,
            Timestamp = DateTime.UtcNow,
            Ordine = _messaggi.Count + 1
        });
    }
}

public class MessaggioAI
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SessioneAIId { get; set; }
    public string Ruolo { get; set; } = default!;      // "user" | "assistant"
    public string Contenuto { get; set; } = default!;
    public DateTime Timestamp { get; set; }
    public int Ordine { get; set; }
}
```

---

# 5. INFRASTRUCTURE LAYER

## 5.1 DbContext — `src/LexManager.Infrastructure/Persistence/LexManagerDbContext.cs`

```csharp
using LexManager.Domain.Entities.Anagrafica;
using LexManager.Domain.Entities.Atti;
using LexManager.Domain.Entities.AI;
using Microsoft.EntityFrameworkCore;

namespace LexManager.Infrastructure.Persistence;

public class LexManagerDbContext(DbContextOptions<LexManagerDbContext> options)
    : DbContext(options)
{
    // Anagrafica
    public DbSet<Soggetto> Soggetti => Set<Soggetto>();
    public DbSet<Fascicolo> Fascicoli => Set<Fascicolo>();
    public DbSet<FascicoloSoggetto> FascicoloSoggetti => Set<FascicoloSoggetto>();

    // Atti
    public DbSet<ModelloAtto> ModelliAtti => Set<ModelloAtto>();
    public DbSet<VersioneModello> VersioniModello => Set<VersioneModello>();

    // AI
    public DbSet<SessioneAI> SessioniAI => Set<SessioneAI>();
    public DbSet<MessaggioAI> MessaggiAI => Set<MessaggioAI>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Applica tutte le configurazioni dalla stessa assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LexManagerDbContext).Assembly);

        // Soft delete global filter
        modelBuilder.Entity<Soggetto>().HasQueryFilter(s => !s.IsDeleted);
        modelBuilder.Entity<Fascicolo>().HasQueryFilter(f => !f.IsDeleted);
        modelBuilder.Entity<ModelloAtto>().HasQueryFilter(m => !m.IsDeleted);
    }
}
```

## 5.2 Entity Configurations — `src/LexManager.Infrastructure/Persistence/Configurations/`

### `SoggettoConfiguration.cs`

```csharp
using LexManager.Domain.Entities.Anagrafica;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LexManager.Infrastructure.Persistence.Configurations;

public class SoggettoConfiguration : IEntityTypeConfiguration<Soggetto>
{
    public void Configure(EntityTypeBuilder<Soggetto> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Cognome).HasMaxLength(100);
        builder.Property(s => s.Nome).HasMaxLength(100);
        builder.Property(s => s.CodiceFiscale).HasMaxLength(16);
        builder.Property(s => s.RagioneSociale).HasMaxLength(200);
        builder.Property(s => s.PartitaIva).HasMaxLength(11);
        builder.Property(s => s.Email).HasMaxLength(150);
        builder.Property(s => s.Pec).HasMaxLength(150);
        builder.Property(s => s.Telefono).HasMaxLength(20);
        builder.Property(s => s.Via).HasMaxLength(200);
        builder.Property(s => s.Citta).HasMaxLength(100);
        builder.Property(s => s.Cap).HasMaxLength(10);
        builder.Property(s => s.Provincia).HasMaxLength(2);

        builder.HasIndex(s => s.CodiceFiscale).IsUnique().HasFilter("[CodiceFiscale] IS NOT NULL");
        builder.HasIndex(s => s.PartitaIva).IsUnique().HasFilter("[PartitaIva] IS NOT NULL");
        builder.HasIndex(s => new { s.Cognome, s.Nome });
    }
}
```

### `FascicoloConfiguration.cs`

```csharp
using LexManager.Domain.Entities.Anagrafica;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LexManager.Infrastructure.Persistence.Configurations;

public class FascicoloConfiguration : IEntityTypeConfiguration<Fascicolo>
{
    public void Configure(EntityTypeBuilder<Fascicolo> builder)
    {
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Numero).HasMaxLength(50).IsRequired();
        builder.Property(f => f.Oggetto).HasMaxLength(500).IsRequired();
        builder.Property(f => f.Tribunale).HasMaxLength(200);
        builder.Property(f => f.NrRG).HasMaxLength(50);
        builder.Property(f => f.Giudice).HasMaxLength(200);

        builder.HasOne(f => f.Cliente)
               .WithMany()
               .HasForeignKey(f => f.ClienteId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(f => f.Controparti)
               .WithOne(fc => fc.Fascicolo)
               .HasForeignKey(fc => fc.FascicoloId);

        builder.HasIndex(f => f.Numero).IsUnique();
    }
}
```

## 5.3 Repositories

### `IRepository.cs` — `src/LexManager.Application/Interfaces/IRepository.cs`

```csharp
namespace LexManager.Application.Interfaces;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default);
    Task<T> AddAsync(T entity, CancellationToken ct = default);
    Task UpdateAsync(T entity, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
```

### `ISoggettoRepository.cs`

```csharp
using LexManager.Domain.Entities.Anagrafica;

namespace LexManager.Application.Interfaces;

public interface ISoggettoRepository : IRepository<Soggetto>
{
    Task<IReadOnlyList<Soggetto>> SearchAsync(string query, CancellationToken ct = default);
    Task<Soggetto?> GetByCodiceFiscaleAsync(string cf, CancellationToken ct = default);
    Task<IReadOnlyList<Soggetto>> GetClientiAsync(CancellationToken ct = default);
}
```

### `SoggettoRepository.cs` — Infrastructure

```csharp
using LexManager.Application.Interfaces;
using LexManager.Domain.Entities.Anagrafica;
using LexManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LexManager.Infrastructure.Repositories;

public class SoggettoRepository(LexManagerDbContext db) : ISoggettoRepository
{
    public async Task<Soggetto?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await db.Soggetti.FindAsync([id], ct);

    public async Task<IReadOnlyList<Soggetto>> GetAllAsync(CancellationToken ct = default)
        => await db.Soggetti.OrderBy(s => s.Cognome).ThenBy(s => s.Nome).ToListAsync(ct);

    public async Task<IReadOnlyList<Soggetto>> SearchAsync(string query, CancellationToken ct = default)
    {
        query = query.Trim().ToLower();
        return await db.Soggetti
            .Where(s => (s.Cognome != null && s.Cognome.ToLower().Contains(query)) ||
                        (s.Nome != null && s.Nome.ToLower().Contains(query)) ||
                        (s.RagioneSociale != null && s.RagioneSociale.ToLower().Contains(query)) ||
                        (s.CodiceFiscale != null && s.CodiceFiscale.ToLower().Contains(query)))
            .OrderBy(s => s.Cognome)
            .Take(50)
            .ToListAsync(ct);
    }

    public async Task<Soggetto?> GetByCodiceFiscaleAsync(string cf, CancellationToken ct = default)
        => await db.Soggetti.FirstOrDefaultAsync(s => s.CodiceFiscale == cf.ToUpperInvariant(), ct);

    public async Task<IReadOnlyList<Soggetto>> GetClientiAsync(CancellationToken ct = default)
        => await db.Soggetti.Where(s => s.IsCliente).OrderBy(s => s.Cognome).ToListAsync(ct);

    public async Task<Soggetto> AddAsync(Soggetto entity, CancellationToken ct = default)
    {
        db.Soggetti.Add(entity);
        await db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task UpdateAsync(Soggetto entity, CancellationToken ct = default)
    {
        db.Soggetti.Update(entity);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await GetByIdAsync(id, ct);
        if (entity is not null)
        {
            entity.MarkAsDeleted();
            await db.SaveChangesAsync(ct);
        }
    }
}
```

## 5.4 DI Registration — `src/LexManager.Infrastructure/DependencyInjection.cs`

```csharp
using LexManager.Application.Interfaces;
using LexManager.Infrastructure.Persistence;
using LexManager.Infrastructure.Repositories;
using LexManager.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace LexManager.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database — SQL Server LocalDB
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? @"Server=(localdb)\MSSQLLocalDB;Database=LexManagerDB;Trusted_Connection=true;MultipleActiveResultSets=true";

        services.AddDbContext<LexManagerDbContext>(options =>
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(3);
                sqlOptions.CommandTimeout(30);
            }));

        // Repositories
        services.AddScoped<ISoggettoRepository, SoggettoRepository>();
        services.AddScoped<IFascicoloRepository, FascicoloRepository>();
        services.AddScoped<IModelloAttoRepository, ModelloAttoRepository>();
        services.AddScoped<ISessioneAIRepository, SessioneAIRepository>();

        // Services
        services.AddScoped<IAIService, OpenAIService>();
        services.AddScoped<IPdfService, PdfService>();
        services.AddScoped<ICalcolatoreScadenzeService, CalcolatoreScadenzeService>();

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
```

---

# 6. APPLICATION LAYER

## 6.1 Use Cases (MediatR CQRS)

### `GetSoggettiQuery.cs`

```csharp
using LexManager.Application.Interfaces;
using LexManager.Domain.Entities.Anagrafica;
using MediatR;

namespace LexManager.Application.UseCases.Anagrafica;

public record GetSoggettiQuery(string? SearchTerm = null) : IRequest<IReadOnlyList<Soggetto>>;

public class GetSoggettiQueryHandler(ISoggettoRepository repo)
    : IRequestHandler<GetSoggettiQuery, IReadOnlyList<Soggetto>>
{
    public async Task<IReadOnlyList<Soggetto>> Handle(
        GetSoggettiQuery request, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            return await repo.SearchAsync(request.SearchTerm, ct);
        return await repo.GetAllAsync(ct);
    }
}
```

### `CreateSoggettoCommand.cs`

```csharp
using LexManager.Application.Interfaces;
using LexManager.Domain.Entities.Anagrafica;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LexManager.Application.UseCases.Anagrafica;

public record CreateSoggettoCommand(
    TipoSoggetto Tipo,
    string? Cognome,
    string? Nome,
    string? CodiceFiscale,
    string? RagioneSociale,
    string? PartitaIva,
    string? Email,
    string? Pec,
    string? Telefono,
    bool IsCliente = true) : IRequest<Soggetto>;

public class CreateSoggettoCommandHandler(
    ISoggettoRepository repo,
    ILogger<CreateSoggettoCommandHandler> logger)
    : IRequestHandler<CreateSoggettoCommand, Soggetto>
{
    public async Task<Soggetto> Handle(CreateSoggettoCommand cmd, CancellationToken ct)
    {
        Soggetto soggetto = cmd.Tipo == TipoSoggetto.PersonaFisica
            ? Soggetto.CreaPersonaFisica(cmd.Cognome!, cmd.Nome!, cmd.CodiceFiscale, null, cmd.Email, cmd.Pec, cmd.Telefono)
            : Soggetto.CreaPersonaGiuridica(cmd.RagioneSociale!, cmd.PartitaIva, null, cmd.Email, cmd.Pec);

        soggetto.ImpostaRuolo(cmd.IsCliente, false);

        var result = await repo.AddAsync(soggetto, ct);
        logger.LogInformation("Creato soggetto {NomeCompleto} [{Id}]", result.NomeCompleto, result.Id);
        return result;
    }
}
```

---

# 7. FRONTEND AVALONIA — SHELL A 5 ZONE

## 7.1 App.axaml — `src/LexManager.UI/App.axaml`

```xml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             x:Class="LexManager.UI.App"
             RequestedThemeVariant="Light">
  <Application.Styles>
    <FluentTheme />
    <StyleInclude Source="avares://LexManager.UI/Styles/LexManagerStyles.axaml" />
  </Application.Styles>
</Application>
```

## 7.2 App.axaml.cs

```csharp
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using LexManager.UI.ViewModels;
using LexManager.UI.Views;
using Microsoft.Extensions.DependencyInjection;

namespace LexManager.UI;

public partial class App : Application
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
```

## 7.3 AppBootstrapper — `src/LexManager.UI/AppBootstrapper.cs`

```csharp
using LexManager.Application.UseCases.Anagrafica;
using LexManager.Infrastructure;
using LexManager.UI.ViewModels;
using LexManager.UI.ViewModels.Anagrafica;
using LexManager.UI.ViewModels.Atti;
using LexManager.UI.ViewModels.AI;
using LexManager.UI.ViewModels.Utilita;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LexManager.UI;

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
        services.AddTransient<FascicoliViewModel>();
        services.AddTransient<ModelliAttiViewModel>();
        services.AddTransient<AIAssistantViewModel>();
        services.AddTransient<UtilitaViewModel>();
        services.AddTransient<StatusBarViewModel>();

        return services.BuildServiceProvider();
    }
}
```

## 7.4 MainWindow — Shell a 5 Zone

### `MainWindow.axaml`

```xml
<Window xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:vm="using:LexManager.UI.ViewModels"
        xmlns:views="using:LexManager.UI.Views"
        x:Class="LexManager.UI.Views.MainWindow"
        x:DataType="vm:MainWindowViewModel"
        Title="LexManager — Studio Legale"
        Width="1400" Height="860"
        MinWidth="1024" MinHeight="600"
        WindowStartupLocation="CenterScreen">

  <Grid RowDefinitions="Auto,*,Auto">

    <!-- ═══════════════════════════════════════════════════ -->
    <!-- ZONA 1: MENU BAR IN ALTO                            -->
    <!-- ═══════════════════════════════════════════════════ -->
    <Grid Grid.Row="0" Background="#1E2A3A">
      <Grid ColumnDefinitions="Auto,*,Auto">

        <!-- Logo + Hamburger -->
        <StackPanel Grid.Column="0" Orientation="Horizontal" Spacing="8" Margin="8,4">
          <Button Command="{Binding ToggleSidebarCommand}"
                  Classes="hamburger"
                  ToolTip.Tip="Mostra/Nascondi navigazione">
            <PathIcon Data="M3,6H21V8H3V6M3,11H21V13H3V11M3,16H21V18H3V16Z"
                      Width="20" Height="20" Foreground="White"/>
          </Button>
          <TextBlock Text="⚖ LexManager" FontSize="18" FontWeight="Bold"
                     Foreground="White" VerticalAlignment="Center"/>
        </StackPanel>

        <!-- Menu classico -->
        <Menu Grid.Column="1" Background="Transparent" VerticalAlignment="Center" Margin="20,0">
          <MenuItem Header="_File" Foreground="White">
            <MenuItem Header="Nuovo Fascicolo"  Command="{Binding NuovoFascicoloCommand}"/>
            <MenuItem Header="Nuovo Soggetto"   Command="{Binding NuovoSoggettoCommand}"/>
            <Separator/>
            <MenuItem Header="Impostazioni"     Command="{Binding ApriImpostazioniCommand}"/>
            <Separator/>
            <MenuItem Header="Esci"             Command="{Binding EsciCommand}"/>
          </MenuItem>
          <MenuItem Header="_Visualizza" Foreground="White">
            <MenuItem Header="Pannello destro" Command="{Binding ToggleRightPanelCommand}"/>
            <MenuItem Header="Barra di stato"  Command="{Binding ToggleStatusBarCommand}"/>
          </MenuItem>
          <MenuItem Header="_Strumenti" Foreground="White">
            <MenuItem Header="Calcola Scadenze"    Command="{Binding NavigateToCommand}" CommandParameter="Utilita"/>
            <MenuItem Header="Generatore Atti"     Command="{Binding NavigateToCommand}" CommandParameter="Atti"/>
            <MenuItem Header="Assistente AI"       Command="{Binding NavigateToCommand}" CommandParameter="AI"/>
          </MenuItem>
          <MenuItem Header="_Aiuto" Foreground="White">
            <MenuItem Header="Documentazione"/>
            <MenuItem Header="Informazioni su LexManager"/>
          </MenuItem>
        </Menu>

        <!-- User area -->
        <StackPanel Grid.Column="2" Orientation="Horizontal" Spacing="8" Margin="8,4">
          <TextBlock Text="{Binding UtenteCorrente}" Foreground="#AAC4E8"
                     VerticalAlignment="Center" FontSize="13"/>
          <Button Classes="flat-white" Content="👤" FontSize="16"/>
        </StackPanel>
      </Grid>
    </Grid>

    <!-- ═══════════════════════════════════════════════════ -->
    <!-- ZONA CENTRALE: sidebar sinistra + contenuto + destra-->
    <!-- ═══════════════════════════════════════════════════ -->
    <Grid Grid.Row="1" ColumnDefinitions="Auto,*,Auto">

      <!-- ─────────────────────────────────────────────── -->
      <!-- ZONA 2: NAVIGAZIONE SINISTRA (collassabile)     -->
      <!-- ─────────────────────────────────────────────── -->
      <Border Grid.Column="0"
              Width="{Binding SidebarWidth}"
              Background="#253347"
              BorderBrush="#1A2535"
              BorderThickness="0,0,1,0">
        <ScrollViewer>
          <StackPanel Margin="0,12,0,0">

            <!-- Ricerca rapida (visibile solo quando espansa) -->
            <TextBox IsVisible="{Binding IsSidebarExpanded}"
                     Watermark="🔍 Cerca..."
                     Margin="8,0,8,12"
                     Text="{Binding SidebarSearchText}"
                     Classes="sidebar-search"/>

            <!-- Voci di navigazione -->
            <ItemsControl ItemsSource="{Binding NavigationItems}">
              <ItemsControl.ItemTemplate>
                <DataTemplate>
                  <Button Command="{Binding $parent[Window].DataContext.NavigateToModuleCommand}"
                          CommandParameter="{Binding ModuleKey}"
                          Classes="nav-item"
                          IsEnabled="True">
                    <Grid ColumnDefinitions="32,*">
                      <TextBlock Grid.Column="0" Text="{Binding Icon}" FontSize="18"
                                 HorizontalAlignment="Center" VerticalAlignment="Center"/>
                      <TextBlock Grid.Column="1" Text="{Binding Label}"
                                 IsVisible="{Binding $parent[Window].DataContext.IsSidebarExpanded}"
                                 VerticalAlignment="Center" Foreground="White"
                                 Margin="8,0,0,0"/>
                    </Grid>
                  </Button>
                </DataTemplate>
              </ItemsControl.ItemTemplate>
            </ItemsControl>

          </StackPanel>
        </ScrollViewer>
      </Border>

      <!-- ─────────────────────────────────────────────── -->
      <!-- ZONA 3: AREA CONTENUTO PRINCIPALE               -->
      <!-- ─────────────────────────────────────────────── -->
      <ContentControl Grid.Column="1"
                      Content="{Binding CurrentView}"
                      Background="#F0F4F8"/>

      <!-- ─────────────────────────────────────────────── -->
      <!-- ZONA 4: PANNELLO CONTESTUALE DESTRO             -->
      <!-- ─────────────────────────────────────────────── -->
      <Border Grid.Column="2"
              Width="{Binding RightPanelWidth}"
              Background="White"
              BorderBrush="#D0D8E4"
              BorderThickness="1,0,0,0"
              IsVisible="{Binding IsRightPanelVisible}">
        <Grid RowDefinitions="Auto,*">
          <Border Grid.Row="0" Background="#E8EEF6" Padding="12,8">
            <Grid ColumnDefinitions="*,Auto">
              <TextBlock Grid.Column="0" Text="{Binding RightPanelTitle}"
                         FontWeight="SemiBold" FontSize="13"/>
              <Button Grid.Column="1" Content="✕" Classes="close-btn"
                      Command="{Binding ToggleRightPanelCommand}"/>
            </Grid>
          </Border>
          <ContentControl Grid.Row="1" Content="{Binding RightPanelContent}"/>
        </Grid>
      </Border>

    </Grid>

    <!-- ═══════════════════════════════════════════════════ -->
    <!-- ZONA 5: BARRA DI STATO IN BASSO                    -->
    <!-- ═══════════════════════════════════════════════════ -->
    <Border Grid.Row="2"
            Background="#1E2A3A"
            Height="26"
            IsVisible="{Binding IsStatusBarVisible}">
      <Grid ColumnDefinitions="*,Auto,Auto,Auto" Margin="8,0">
        <TextBlock Grid.Column="0" Text="{Binding StatusMessage}"
                   Foreground="#AAC4E8" FontSize="11" VerticalAlignment="Center"/>
        <TextBlock Grid.Column="1" Text="{Binding DbStatus}"
                   Foreground="#7ACCA8" FontSize="11" VerticalAlignment="Center" Margin="0,0,16,0"/>
        <TextBlock Grid.Column="2" Text="{Binding CurrentDateTime}"
                   Foreground="#AAC4E8" FontSize="11" VerticalAlignment="Center" Margin="0,0,16,0"/>
        <ProgressBar Grid.Column="3" Width="80" Height="6"
                     Value="{Binding BusyProgress}"
                     IsVisible="{Binding IsBusy}"
                     IsIndeterminate="{Binding IsIndeterminate}"/>
      </Grid>
    </Border>

  </Grid>
</Window>
```

## 7.5 MainWindowViewModel — `src/LexManager.UI/ViewModels/MainWindowViewModel.cs`

```csharp
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LexManager.UI.ViewModels.Anagrafica;
using LexManager.UI.ViewModels.Atti;
using LexManager.UI.ViewModels.AI;
using LexManager.UI.ViewModels.Utilita;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;

namespace LexManager.UI.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly IServiceProvider _sp;
    private readonly DispatcherTimer _clockTimer;

    [ObservableProperty] private object? _currentView;
    [ObservableProperty] private bool _isSidebarExpanded = true;
    [ObservableProperty] private bool _isRightPanelVisible = false;
    [ObservableProperty] private bool _isStatusBarVisible = true;
    [ObservableProperty] private string _statusMessage = "Pronto";
    [ObservableProperty] private string _dbStatus = "● DB OK";
    [ObservableProperty] private string _currentDateTime = DateTime.Now.ToString("ddd dd/MM/yyyy  HH:mm");
    [ObservableProperty] private string _utenteCorrente = "Avv. Studio";
    [ObservableProperty] private string _rightPanelTitle = "Dettaglio";
    [ObservableProperty] private object? _rightPanelContent;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private bool _isIndeterminate = true;
    [ObservableProperty] private double _busyProgress;
    [ObservableProperty] private string _sidebarSearchText = string.Empty;

    public double SidebarWidth => IsSidebarExpanded ? 220 : 52;
    public double RightPanelWidth => IsRightPanelVisible ? 320 : 0;

    public ObservableCollection<NavItem> NavigationItems { get; } =
    [
        new("dashboard",    "🏠", "Dashboard"),
        new("anagrafica",   "👥", "Anagrafica"),
        new("fascicoli",    "📁", "Fascicoli"),
        new("atti",         "📄", "Modelli Atti"),
        new("ai",           "🤖", "Assistente AI"),
        new("utilita",      "🔧", "Utilità"),
        new("scadenzario",  "📅", "Scadenzario"),
        new("parcelle",     "💶", "Parcelle"),
    ];

    public MainWindowViewModel(IServiceProvider sp)
    {
        _sp = sp;
        _clockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
        _clockTimer.Tick += (_, _) => CurrentDateTime = DateTime.Now.ToString("ddd dd/MM/yyyy  HH:mm");
        _clockTimer.Start();

        NavigateToModule("dashboard");
    }

    [RelayCommand]
    private void ToggleSidebar()
    {
        IsSidebarExpanded = !IsSidebarExpanded;
        OnPropertyChanged(nameof(SidebarWidth));
    }

    [RelayCommand]
    private void ToggleRightPanel()
    {
        IsRightPanelVisible = !IsRightPanelVisible;
        OnPropertyChanged(nameof(RightPanelWidth));
    }

    [RelayCommand]
    private void ToggleStatusBar() => IsStatusBarVisible = !IsStatusBarVisible;

    [RelayCommand]
    private void NavigateToModule(string moduleKey)
    {
        StatusMessage = $"Caricamento modulo: {moduleKey}...";
        CurrentView = moduleKey switch
        {
            "anagrafica"  => _sp.GetRequiredService<AnagraficaViewModel>(),
            "fascicoli"   => _sp.GetRequiredService<FascicoliViewModel>(),
            "atti"        => _sp.GetRequiredService<ModelliAttiViewModel>(),
            "ai"          => _sp.GetRequiredService<AIAssistantViewModel>(),
            "utilita"     => _sp.GetRequiredService<UtilitaViewModel>(),
            "dashboard"   => _sp.GetRequiredService<DashboardViewModel>(),
            _             => _sp.GetRequiredService<DashboardViewModel>()
        };
        StatusMessage = $"Modulo {moduleKey} caricato";
    }

    [RelayCommand]
    private void NavigateTo(string key) => NavigateToModule(key);

    [RelayCommand]
    private void NuovoFascicolo() => NavigateToModule("fascicoli");

    [RelayCommand]
    private void NuovoSoggetto() => NavigateToModule("anagrafica");

    [RelayCommand]
    private void ApriImpostazioni() { /* futuro */ }

    [RelayCommand]
    private void Esci() => Environment.Exit(0);

    public void ShowRightPanel(string title, object content)
    {
        RightPanelTitle = title;
        RightPanelContent = content;
        IsRightPanelVisible = true;
        OnPropertyChanged(nameof(RightPanelWidth));
    }
}

public record NavItem(string ModuleKey, string Icon, string Label);
```

---

# 8. MODULO ANAGRAFICA

## 8.1 AnagraficaViewModel — `src/LexManager.UI/ViewModels/Anagrafica/AnagraficaViewModel.cs`

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LexManager.Application.UseCases.Anagrafica;
using LexManager.Domain.Entities.Anagrafica;
using MediatR;
using System.Collections.ObjectModel;

namespace LexManager.UI.ViewModels.Anagrafica;

public partial class AnagraficaViewModel : ObservableObject
{
    private readonly IMediator _mediator;

    [ObservableProperty] private ObservableCollection<Soggetto> _soggetti = [];
    [ObservableProperty] private Soggetto? _soggettoSelezionato;
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isDetailOpen;
    [ObservableProperty] private SoggettoDetailViewModel? _detailViewModel;

    public AnagraficaViewModel(IMediator mediator)
    {
        _mediator = mediator;
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        IsLoading = true;
        try
        {
            var result = await _mediator.Send(new GetSoggettiQuery());
            Soggetti = new ObservableCollection<Soggetto>(result);
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private async Task Search()
    {
        IsLoading = true;
        try
        {
            var result = await _mediator.Send(new GetSoggettiQuery(SearchText));
            Soggetti = new ObservableCollection<Soggetto>(result);
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private void NuovoSoggetto()
    {
        DetailViewModel = new SoggettoDetailViewModel(_mediator, null);
        IsDetailOpen = true;
    }

    [RelayCommand]
    private void ApriDettaglio(Soggetto soggetto)
    {
        SoggettoSelezionato = soggetto;
        DetailViewModel = new SoggettoDetailViewModel(_mediator, soggetto);
        IsDetailOpen = true;
    }

    [RelayCommand]
    private void ChiudiDettaglio() => IsDetailOpen = false;

    partial void OnSearchTextChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            _ = LoadAsync();
    }
}
```

## 8.2 AnagraficaView.axaml

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:vm="using:LexManager.UI.ViewModels.Anagrafica"
             x:Class="LexManager.UI.Views.Anagrafica.AnagraficaView"
             x:DataType="vm:AnagraficaViewModel">

  <Grid RowDefinitions="Auto,*">

    <!-- Toolbar -->
    <Border Grid.Row="0" Background="White" Padding="16,12"
            BorderBrush="#E0E8F0" BorderThickness="0,0,0,1">
      <Grid ColumnDefinitions="Auto,*,Auto,Auto">
        <TextBlock Grid.Column="0" Text="👥 Anagrafica"
                   FontSize="20" FontWeight="Bold" Foreground="#1E2A3A"
                   VerticalAlignment="Center"/>
        <TextBox Grid.Column="1" Watermark="🔍 Cerca per nome, CF, P.IVA..."
                 Text="{Binding SearchText}"
                 Margin="20,0" MaxWidth="400"
                 Classes="search-box">
          <Interaction.Behaviors>
            <EventTriggerBehavior EventName="KeyUp">
              <InvokeCommandAction Command="{Binding SearchCommand}"/>
            </EventTriggerBehavior>
          </Interaction.Behaviors>
        </TextBox>
        <Button Grid.Column="2" Content="+ Nuovo Soggetto"
                Command="{Binding NuovoSoggettoCommand}"
                Classes="primary-btn" Margin="8,0"/>
        <Button Grid.Column="3" Content="📥 Importa"
                Classes="secondary-btn"/>
      </Grid>
    </Border>

    <!-- Tabella soggetti -->
    <DataGrid Grid.Row="1"
              ItemsSource="{Binding Soggetti}"
              SelectedItem="{Binding SoggettoSelezionato}"
              AutoGenerateColumns="False"
              IsReadOnly="True"
              GridLinesVisibility="Horizontal"
              AlternatingRowBackground="#F8FAFC"
              Background="White">
      <DataGrid.Columns>
        <DataGridTextColumn Header="Tipo"      Binding="{Binding Tipo}"       Width="120"/>
        <DataGridTextColumn Header="Nome/Ragione Sociale" Binding="{Binding NomeCompleto}" Width="*"/>
        <DataGridTextColumn Header="CF / P.IVA"
                            Binding="{Binding CodiceFiscale}" Width="140"/>
        <DataGridTextColumn Header="Email"     Binding="{Binding Email}"      Width="200"/>
        <DataGridTextColumn Header="Telefono"  Binding="{Binding Telefono}"   Width="130"/>
        <DataGridTextColumn Header="Città"     Binding="{Binding Citta}"      Width="120"/>
        <DataGridTemplateColumn Header="Ruolo" Width="100">
          <DataGridTemplateColumn.CellTemplate>
            <DataTemplate>
              <StackPanel Orientation="Horizontal" Spacing="4" Margin="4,2">
                <Border Background="#D1FAE5" CornerRadius="4" Padding="4,2"
                        IsVisible="{Binding IsCliente}">
                  <TextBlock Text="Cliente" FontSize="11" Foreground="#065F46"/>
                </Border>
                <Border Background="#FEE2E2" CornerRadius="4" Padding="4,2"
                        IsVisible="{Binding IsControparte}">
                  <TextBlock Text="Controparte" FontSize="11" Foreground="#991B1B"/>
                </Border>
              </StackPanel>
            </DataTemplate>
          </DataGridTemplateColumn.CellTemplate>
        </DataGridTemplateColumn>
        <DataGridTemplateColumn Header="" Width="80">
          <DataGridTemplateColumn.CellTemplate>
            <DataTemplate>
              <Button Content="✏️" Classes="icon-btn"
                      Command="{Binding $parent[DataGrid].DataContext.ApriDettaglioCommand}"
                      CommandParameter="{Binding}"/>
            </DataTemplate>
          </DataGridTemplateColumn.CellTemplate>
        </DataGridTemplateColumn>
      </DataGrid.Columns>
    </DataGrid>

  </Grid>
</UserControl>
```

---

# 9. MODULO GESTIONE MODELLI DI ATTI

## 9.1 ModelliAttiViewModel

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LexManager.Application.Interfaces;
using LexManager.Domain.Entities.Atti;
using System.Collections.ObjectModel;

namespace LexManager.UI.ViewModels.Atti;

public partial class ModelliAttiViewModel : ObservableObject
{
    private readonly IModelloAttoRepository _repo;

    [ObservableProperty] private ObservableCollection<ModelloAtto> _modelli = [];
    [ObservableProperty] private ModelloAtto? _modelloSelezionato;
    [ObservableProperty] private string _contenutoEditor = string.Empty;
    [ObservableProperty] private bool _isEditing;
    [ObservableProperty] private string _anteprima = string.Empty;
    [ObservableProperty] private CategoriaAtto _filtroCategoria;

    public IEnumerable<CategoriaAtto> CategorieDisponibili
        => Enum.GetValues<CategoriaAtto>();

    public ModelliAttiViewModel(IModelloAttoRepository repo)
    {
        _repo = repo;
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        var result = await _repo.GetAllAsync();
        Modelli = new ObservableCollection<ModelloAtto>(result);
    }

    [RelayCommand]
    private void NuovoModello()
    {
        ModelloSelezionato = null;
        ContenutoEditor = GetTemplateVuoto();
        IsEditing = true;
    }

    [RelayCommand]
    private void SelezionaModello(ModelloAtto modello)
    {
        ModelloSelezionato = modello;
        ContenutoEditor = modello.Contenuto;
        IsEditing = false;
    }

    [RelayCommand]
    private void GeneraAnteprima()
    {
        var segnaposto = new Dictionary<string, string>
        {
            ["NOME_CLIENTE"]   = "Mario Rossi",
            ["DATA_OGGI"]      = DateTime.Today.ToString("dd/MM/yyyy"),
            ["TRIBUNALE"]      = "Tribunale di Milano",
            ["NR_RG"]          = "12345/2024",
            ["NOME_AVVOCATO"]  = "Avv. Studio"
        };
        Anteprima = ModelloSelezionato?.GeneraDocumento(segnaposto)
                    ?? ContenutoEditor;
    }

    [RelayCommand]
    private async Task SalvaModello(string nomeModello)
    {
        if (ModelloSelezionato is null)
        {
            var nuovo = ModelloAtto.Crea(nomeModello, CategoriaAtto.Altro, ContenutoEditor);
            await _repo.AddAsync(nuovo);
            Modelli.Add(nuovo);
        }
        else
        {
            ModelloSelezionato.AggiornaContenuto(ContenutoEditor);
            await _repo.UpdateAsync(ModelloSelezionato);
        }
        IsEditing = false;
        await LoadAsync();
    }

    private static string GetTemplateVuoto() => """
        TRIBUNALE DI {{TRIBUNALE}}
        
        Procedimento n. {{NR_RG}}
        
        Giudice: {{GIUDICE}}
        
        RICORSO EX ART. ___
        
        Per conto di:
        {{NOME_CLIENTE}}
        
        Contro:
        {{NOME_CONTROPARTE}}
        
        -----------------------------------------------
        
        Con la presente, il sottoscritto {{NOME_AVVOCATO}},
        difensore del ricorrente, espone quanto segue:
        
        FATTO
        
        [...]
        
        DIRITTO
        
        [...]
        
        P.Q.M.
        
        Si chiede che l'Ill.mo Tribunale voglia:
        - [...]
        
        Data: {{DATA_OGGI}}
        
        Avv. {{NOME_AVVOCATO}}
        """;
}
```

---

# 10. MODULO AI

## 10.1 IAIService — Application Layer

```csharp
namespace LexManager.Application.Interfaces;

public interface IAIService
{
    IAsyncEnumerable<string> StreamChatAsync(
        IEnumerable<(string Role, string Content)> messages,
        string systemPrompt,
        CancellationToken ct = default);

    Task<string> ChatAsync(
        IEnumerable<(string Role, string Content)> messages,
        string systemPrompt,
        CancellationToken ct = default);
}
```

## 10.2 OpenAIService — Infrastructure

```csharp
using LexManager.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace LexManager.Infrastructure.Services;

public class OpenAIService : IAIService
{
    private readonly HttpClient _http;
    private readonly string _model;
    private readonly ILogger<OpenAIService> _logger;

    public OpenAIService(IConfiguration config, ILogger<OpenAIService> logger)
    {
        _logger = logger;
        _model = config["AI:Model"] ?? "gpt-4o";
        var apiKey = config["AI:ApiKey"] ?? throw new InvalidOperationException("AI:ApiKey non configurata");

        _http = new HttpClient { BaseAddress = new Uri("https://api.openai.com/v1/") };
        _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
    }

    public async Task<string> ChatAsync(
        IEnumerable<(string Role, string Content)> messages,
        string systemPrompt,
        CancellationToken ct = default)
    {
        var body = BuildRequest(messages, systemPrompt, false);
        var response = await _http.PostAsJsonAsync("chat/completions", body, ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? string.Empty;
    }

    public async IAsyncEnumerable<string> StreamChatAsync(
        IEnumerable<(string Role, string Content)> messages,
        string systemPrompt,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var body = BuildRequest(messages, systemPrompt, true);
        var request = new HttpRequestMessage(HttpMethod.Post, "chat/completions")
        {
            Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
        };

        using var response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream && !ct.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(ct) ?? "";
            if (!line.StartsWith("data:")) continue;
            var data = line[6..].Trim();
            if (data == "[DONE]") yield break;

            using var doc = JsonDocument.Parse(data);
            var delta = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("delta");

            if (delta.TryGetProperty("content", out var content))
                yield return content.GetString() ?? "";
        }
    }

    private object BuildRequest(IEnumerable<(string Role, string Content)> messages, string systemPrompt, bool stream)
    {
        var msgs = new List<object>
        {
            new { role = "system", content = systemPrompt }
        };
        msgs.AddRange(messages.Select(m => new { role = m.Role, content = m.Content }));

        return new { model = _model, messages = msgs, stream, max_tokens = 4096 };
    }
}
```

## 10.3 AIAssistantViewModel

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LexManager.Application.Interfaces;
using LexManager.Domain.Entities.AI;
using System.Collections.ObjectModel;

namespace LexManager.UI.ViewModels.AI;

public partial class AIAssistantViewModel : ObservableObject
{
    private readonly IAIService _ai;
    private readonly ISessioneAIRepository _sessioniRepo;

    [ObservableProperty] private ObservableCollection<ChatMessage> _messaggi = [];
    [ObservableProperty] private string _inputUtente = string.Empty;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private TipoRicercaAI _tipoRicerca = TipoRicercaAI.Assistenza;
    [ObservableProperty] private string _titoloSessione = "Nuova sessione";

    public IEnumerable<TipoRicercaAI> TipiRicerca => Enum.GetValues<TipoRicercaAI>();

    private static readonly Dictionary<TipoRicercaAI, string> SystemPrompts = new()
    {
        [TipoRicercaAI.Assistenza] = """
            Sei un assistente legale specializzato per avvocati italiani.
            Aiuti con l'analisi di casi, redazione di atti, e consulenza procedurale.
            Rispondi sempre in italiano, con precisione giuridica e citando le norme rilevanti.
            """,
        [TipoRicercaAI.Giurisprudenza] = """
            Sei un esperto di giurisprudenza italiana. 
            Aiuti a trovare sentenze rilevanti, analizzare orientamenti giurisprudenziali 
            e confrontare pronunce di Cassazione, Corte Costituzionale e Corti di merito.
            Cita sempre i riferimenti (Cass. n. xxx/anno) quando disponibili.
            """,
        [TipoRicercaAI.Dottrina] = """
            Sei un esperto di dottrina giuridica italiana.
            Aiuti ad analizzare orientamenti dottrinali, commentare articoli di legge,
            e fornire riferimenti bibliografici ai principali autori e opere del diritto.
            """,
        [TipoRicercaAI.Normativa] = """
            Sei un esperto di normativa italiana ed europea.
            Aiuti a trovare le norme applicabili, analizzare abrogazioni e modifiche legislative,
            e interpretare disposizioni di legge nel contesto del caso concreto.
            """
    };

    public AIAssistantViewModel(IAIService ai, ISessioneAIRepository sessioniRepo)
    {
        _ai = ai;
        _sessioniRepo = sessioniRepo;
    }

    [RelayCommand]
    private async Task InviaMessaggioAsync()
    {
        if (string.IsNullOrWhiteSpace(InputUtente) || IsLoading) return;

        var testo = InputUtente.Trim();
        InputUtente = string.Empty;

        Messaggi.Add(new ChatMessage("user", testo));

        IsLoading = true;
        var rispostaMsg = new ChatMessage("assistant", "");
        Messaggi.Add(rispostaMsg);

        try
        {
            var history = Messaggi.SkipLast(1)
                .Select(m => (m.Ruolo, m.Contenuto));

            var sb = new System.Text.StringBuilder();
            await foreach (var chunk in _ai.StreamChatAsync(
                history,
                SystemPrompts[TipoRicerca]))
            {
                sb.Append(chunk);
                rispostaMsg.Contenuto = sb.ToString();
            }
        }
        catch (Exception ex)
        {
            rispostaMsg.Contenuto = $"Errore: {ex.Message}";
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private void NuovaSessione()
    {
        Messaggi.Clear();
        TitoloSessione = "Nuova sessione";
    }

    [RelayCommand]
    private void InserisciPromptRapido(string tipo)
    {
        InputUtente = tipo switch
        {
            "sentenza" => "Cerca sentenze recenti della Cassazione in tema di: ",
            "norma"    => "Quali sono le norme applicabili al caso di: ",
            "schema"   => "Crea uno schema procedurale per: ",
            "atto"     => "Aiutami a redigere un'istanza per: ",
            _          => InputUtente
        };
    }
}

public partial class ChatMessage : ObservableObject
{
    public string Ruolo { get; }
    [ObservableProperty] private string _contenuto;
    public bool IsUser => Ruolo == "user";
    public DateTime Timestamp { get; } = DateTime.Now;

    public ChatMessage(string ruolo, string contenuto)
    {
        Ruolo = ruolo;
        _contenuto = contenuto;
    }
}
```

## 10.4 AIAssistantView.axaml

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:vm="using:LexManager.UI.ViewModels.AI"
             x:Class="LexManager.UI.Views.AI.AIAssistantView"
             x:DataType="vm:AIAssistantViewModel">

  <Grid RowDefinitions="Auto,*,Auto">

    <!-- Header -->
    <Border Grid.Row="0" Background="White" Padding="16,12"
            BorderBrush="#E0E8F0" BorderThickness="0,0,0,1">
      <Grid ColumnDefinitions="Auto,*,Auto,Auto">
        <TextBlock Grid.Column="0" Text="🤖 Assistente AI Legale"
                   FontSize="20" FontWeight="Bold" Foreground="#1E2A3A"
                   VerticalAlignment="Center"/>
        <ComboBox Grid.Column="2"
                  ItemsSource="{Binding TipiRicerca}"
                  SelectedItem="{Binding TipoRicerca}"
                  Width="200" Margin="0,0,12,0"/>
        <Button Grid.Column="3" Content="+ Nuova" Classes="secondary-btn"
                Command="{Binding NuovaSessioneCommand}"/>
      </Grid>
    </Border>

    <!-- Chat area -->
    <ScrollViewer Grid.Row="1" Background="#F0F4F8">
      <ItemsControl ItemsSource="{Binding Messaggi}" Margin="16">
        <ItemsControl.ItemTemplate>
          <DataTemplate x:DataType="vm:ChatMessage">
            <Grid Margin="0,6">
              <!-- Messaggio utente -->
              <Border HorizontalAlignment="Right" MaxWidth="600"
                      Background="#2563EB" CornerRadius="12,12,2,12"
                      Padding="12,10" IsVisible="{Binding IsUser}">
                <TextBlock Text="{Binding Contenuto}" TextWrapping="Wrap"
                           Foreground="White"/>
              </Border>
              <!-- Risposta AI -->
              <Border HorizontalAlignment="Left" MaxWidth="700"
                      Background="White" CornerRadius="2,12,12,12"
                      Padding="12,10" IsVisible="{Binding !IsUser}"
                      BoxShadow="0 1 3 0 #1A000000">
                <StackPanel>
                  <TextBlock Text="⚖ Assistente AI" FontSize="11"
                             Foreground="#6B7280" Margin="0,0,0,6"/>
                  <TextBlock Text="{Binding Contenuto}" TextWrapping="Wrap"
                             Foreground="#1F2937"/>
                </StackPanel>
              </Border>
            </Grid>
          </DataTemplate>
        </ItemsControl.ItemTemplate>
      </ItemsControl>
    </ScrollViewer>

    <!-- Input area -->
    <Border Grid.Row="2" Background="White" Padding="16,12"
            BorderBrush="#E0E8F0" BorderThickness="0,1,0,0">
      <Grid RowDefinitions="Auto,Auto">

        <!-- Quick prompts -->
        <StackPanel Grid.Row="0" Orientation="Horizontal" Spacing="8" Margin="0,0,0,10">
          <TextBlock Text="Rapido:" VerticalAlignment="Center" FontSize="12"
                     Foreground="#6B7280"/>
          <Button Content="📋 Sentenza" Classes="chip-btn"
                  Command="{Binding InserisciPromptRapidoCommand}"
                  CommandParameter="sentenza"/>
          <Button Content="📖 Norma"    Classes="chip-btn"
                  Command="{Binding InserisciPromptRapidoCommand}"
                  CommandParameter="norma"/>
          <Button Content="📐 Schema"   Classes="chip-btn"
                  Command="{Binding InserisciPromptRapidoCommand}"
                  CommandParameter="schema"/>
          <Button Content="✍️ Atto"    Classes="chip-btn"
                  Command="{Binding InserisciPromptRapidoCommand}"
                  CommandParameter="atto"/>
        </StackPanel>

        <!-- Input box -->
        <Grid Grid.Row="1" ColumnDefinitions="*,Auto">
          <TextBox Grid.Column="0"
                   Text="{Binding InputUtente}"
                   Watermark="Fai una domanda giuridica, cerca giurisprudenza, richiedi assistenza..."
                   AcceptsReturn="False"
                   Classes="chat-input"
                   Margin="0,0,8,0">
          </TextBox>
          <Button Grid.Column="1"
                  Content="→ Invia"
                  Command="{Binding InviaMessaggioCommand}"
                  IsEnabled="{Binding !IsLoading}"
                  Classes="primary-btn"
                  Width="90"/>
        </Grid>

      </Grid>
    </Border>

  </Grid>
</UserControl>
```

---

# 11. MODULO UTILITÀ E STRUMENTI PROFESSIONALI

## 11.1 UtilitaViewModel

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LexManager.Application.Interfaces;

namespace LexManager.UI.ViewModels.Utilita;

public partial class UtilitaViewModel : ObservableObject
{
    private readonly ICalcolatoreScadenzeService _scadenze;

    // --- Calcolo scadenze ---
    [ObservableProperty] private DateTime _dataDecorrenza = DateTime.Today;
    [ObservableProperty] private int _giorniTermine = 30;
    [ObservableProperty] private bool _escludioSabato = true;
    [ObservableProperty] private bool _escludiDomenica = true;
    [ObservableProperty] private string _dataScadenzaCalcolata = string.Empty;
    [ObservableProperty] private string _noteScadenza = string.Empty;

    // --- Convertitore CF ---
    [ObservableProperty] private string _inputCf = string.Empty;
    [ObservableProperty] private string _risultatoCf = string.Empty;

    // --- Calcolo parcella orientativa ---
    [ObservableProperty] private decimal _valoreControversia;
    [ObservableProperty] private string _tipoAttivita = "Fase Istruttoria";
    [ObservableProperty] private decimal _parcellaMinimaCalcolata;
    [ObservableProperty] private decimal _parcellaMediaCalcolata;
    [ObservableProperty] private decimal _parcellaMassimaCalcolata;

    // --- Contatore parole ---
    [ObservableProperty] private string _testoContatore = string.Empty;
    [ObservableProperty] private int _numeroParole;
    [ObservableProperty] private int _numeroCaratteri;
    [ObservableProperty] private int _numeroPagine;

    public UtilitaViewModel(ICalcolatoreScadenzeService scadenze)
    {
        _scadenze = scadenze;
    }

    [RelayCommand]
    private void CalcolaScadenza()
    {
        var dataFine = _scadenze.CalcolaScadenza(
            DataDecorrenza, GiorniTermine, EscludioSabato, EscludiDomenica);

        DataScadenzaCalcolata = dataFine.ToString("dddd dd MMMM yyyy");
        NoteScadenza = dataFine < DateTime.Today
            ? "⚠️ ATTENZIONE: scadenza già decorsa!"
            : $"Mancano {(dataFine - DateTime.Today).Days} giorni";
    }

    [RelayCommand]
    private void DecodificaCf()
    {
        if (string.IsNullOrWhiteSpace(InputCf) || InputCf.Length != 16)
        {
            RisultatoCf = "Codice fiscale non valido (richiesti 16 caratteri)";
            return;
        }

        try
        {
            var cf = InputCf.ToUpperInvariant();
            var mesiMap = new Dictionary<char, int>
            {
                ['A'] = 1, ['B'] = 2, ['C'] = 3, ['D'] = 4,
                ['E'] = 5, ['H'] = 6, ['L'] = 7, ['M'] = 8,
                ['P'] = 9, ['R'] = 10, ['S'] = 11, ['T'] = 12
            };

            var anno = cf[6..8];
            var mese = mesiMap.TryGetValue(cf[8], out var m) ? m : 0;
            var giornoRaw = int.Parse(cf[9..11]);
            var giorno = giornoRaw > 40 ? giornoRaw - 40 : giornoRaw;
            var sesso = giornoRaw > 40 ? "F" : "M";
            var comuneCode = cf[11..15];

            RisultatoCf = $"Sesso: {sesso}\nData nascita: {giorno:D2}/{mese:D2}/19{anno} (o 20{anno})\nCodice comune: {comuneCode}";
        }
        catch { RisultatoCf = "Errore nella decodifica"; }
    }

    [RelayCommand]
    private void CalcolaParcella()
    {
        // Parametri orientativi basati sul D.M. 55/2014
        var (min, med, max) = ValoreControversia switch
        {
            <= 1_100        => (270m, 500m, 900m),
            <= 5_200        => (500m, 1_100m, 2_200m),
            <= 26_000       => (1_100m, 2_800m, 5_600m),
            <= 52_000       => (2_200m, 4_500m, 9_000m),
            <= 260_000      => (4_000m, 8_000m, 16_000m),
            <= 520_000      => (6_500m, 12_000m, 24_000m),
            _               => (10_000m, 20_000m, 40_000m)
        };

        ParcellaMinimaCalcolata = min;
        ParcellaMediaCalcolata = med;
        ParcellaMassimaCalcolata = max;
    }

    partial void OnTestoContatoreChanged(string value)
    {
        var words = value.Split([' ', '\n', '\r', '\t'], StringSplitOptions.RemoveEmptyEntries);
        NumeroParole = words.Length;
        NumeroCaratteri = value.Length;
        NumeroPagine = (int)Math.Ceiling(NumeroParole / 250.0);
    }
}
```

## 11.2 CalcolatoreScadenzeService

```csharp
using LexManager.Application.Interfaces;

namespace LexManager.Infrastructure.Services;

public class CalcolatoreScadenzeService : ICalcolatoreScadenzeService
{
    private static readonly HashSet<DateTime> FestivitaItalia = ComputeFestivita();

    public DateTime CalcolaScadenza(
        DateTime dataDecorrenza,
        int giorniTermine,
        bool escludiSabato = true,
        bool escludiDomenica = true)
    {
        var data = dataDecorrenza;
        var giorniContati = 0;

        while (giorniContati < giorniTermine)
        {
            data = data.AddDays(1);

            var isWeekend = (data.DayOfWeek == DayOfWeek.Saturday && escludiSabato)
                         || (data.DayOfWeek == DayOfWeek.Sunday && escludiDomenica);

            var isFestivo = FestivitaItalia.Contains(data.Date)
                         || IsFestaRepubblica(data)
                         || IsFestaLavoro(data);

            if (!isWeekend && !isFestivo)
                giorniContati++;
        }

        return data;
    }

    private static bool IsFestaRepubblica(DateTime d) => d.Month == 6 && d.Day == 2;
    private static bool IsFestaLavoro(DateTime d) => d.Month == 5 && d.Day == 1;

    private static HashSet<DateTime> ComputeFestivita()
    {
        var year = DateTime.Now.Year;
        return
        [
            new DateTime(year, 1, 1),   // Capodanno
            new DateTime(year, 1, 6),   // Epifania
            new DateTime(year, 4, 25),  // Liberazione
            new DateTime(year, 8, 15),  // Ferragosto
            new DateTime(year, 11, 1),  // Tutti i santi
            new DateTime(year, 12, 8),  // Immacolata
            new DateTime(year, 12, 25), // Natale
            new DateTime(year, 12, 26), // S. Stefano
        ];
    }
}
```

---

# 12. SERILOG + SEQ

## 12.1 appsettings.json — `src/LexManager.UI/appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=LexManagerDB;Trusted_Connection=true;MultipleActiveResultSets=true"
  },
  "Seq": {
    "ServerUrl": "http://localhost:5341"
  },
  "AI": {
    "ApiKey": "sk-YOUR-OPENAI-KEY-HERE",
    "Model": "gpt-4o"
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.EntityFrameworkCore": "Warning",
        "System": "Warning"
      }
    }
  }
}
```

## 12.2 docker-compose per SEQ — `docker/seq/docker-compose.yml`

```yaml
version: "3.8"
services:
  seq:
    image: datalust/seq:latest
    container_name: lexmanager-seq
    restart: unless-stopped
    environment:
      - ACCEPT_EULA=Y
    ports:
      - "5341:5341"
      - "8080:80"
    volumes:
      - seq-data:/data

volumes:
  seq-data:
```

---

# 13. MIGRATIONS E AVVIO

## 13.1 Crea la Migration Iniziale

```powershell
# Installa EF Tools globalmente
dotnet tool install --global dotnet-ef

# Crea la prima migration
cd src/LexManager.UI
dotnet ef migrations add InitialCreate \
  --project ../LexManager.Infrastructure \
  --startup-project . \
  --output-dir ../LexManager.Infrastructure/Persistence/Migrations

# Applica il database
dotnet ef database update \
  --project ../LexManager.Infrastructure \
  --startup-project .
```

## 13.2 Program.cs / Main — `src/LexManager.UI/Program.cs`

```csharp
using Avalonia;
using LexManager.Infrastructure;
using LexManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace LexManager.UI;

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
        var db = scope.ServiceProvider.GetRequiredService<LexManagerDbContext>();

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
```

---

# 14. ESTENDERE CON NUOVI MODULI

## Procedura standard per aggiungere un nuovo modulo (es. "Parcelle")

### Step 1 — Domain

```
src/LexManager.Domain/Entities/Parcelle/
├── Parcella.cs
├── VoceParcella.cs
└── StatoParcella.cs (enum)
```

### Step 2 — Application

```
src/LexManager.Application/UseCases/Parcelle/
├── GetParcelleQuery.cs
├── CreateParcellaCommand.cs
└── GeneraPdfParcellaCommand.cs
```

### Step 3 — Infrastructure

```
src/LexManager.Infrastructure/
├── Repositories/ParcellaRepository.cs
└── Persistence/Configurations/ParcellaConfiguration.cs
```

### Step 4 — UI

```
src/LexManager.UI/
├── ViewModels/Parcelle/ParcelleViewModel.cs
└── Views/Parcelle/ParcelleView.axaml
```

### Step 5 — Registra nel DI

```csharp
// AppBootstrapper.cs
services.AddTransient<ParcelleViewModel>();

// DependencyInjection.cs (Infrastructure)
services.AddScoped<IParcellaRepository, ParcellaRepository>();
```

### Step 6 — Aggiungi alla Navigation

```csharp
// MainWindowViewModel.cs — NavigationItems
new("parcelle", "💶", "Parcelle"),

// NavigateToModule switch
"parcelle" => _sp.GetRequiredService<ParcelleViewModel>(),
```

### Step 7 — Migration

```powershell
dotnet ef migrations add AddParcelleModule \
  --project src/LexManager.Infrastructure \
  --startup-project src/LexManager.UI
dotnet ef database update \
  --project src/LexManager.Infrastructure \
  --startup-project src/LexManager.UI
```

---

## ALTRI MODULI CONSIGLIATI

| Modulo | Descrizione |
|--------|-------------|
| **Scadenzario** | Calendario con alert automatici, integrazione con Outlook |
| **Parcelle e Onorari** | Fatturazione, notule, parcelle con PDF (D.M. 55/2014) |
| **Agenda** | Udienze, appuntamenti, riunioni |
| **Documenti** | Archivio digitale con preview, tagging, OCR |
| **Timesheet** | Tracciamento ore per fascicolo/attività |
| **Contabilità** | Prima nota, incassi, IVA semplificata |
| **Comunicazioni** | Email centralizzata, PEC, notifiche |
| **Conformità GDPR** | Registro trattamenti, richieste interessati |

---

## STRUTTURA STILI AVALONIA — `src/LexManager.UI/Styles/LexManagerStyles.axaml`

```xml
<Styles xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

  <!-- Bottone primario -->
  <Style Selector="Button.primary-btn">
    <Setter Property="Background" Value="#2563EB"/>
    <Setter Property="Foreground" Value="White"/>
    <Setter Property="Padding" Value="14,8"/>
    <Setter Property="CornerRadius" Value="6"/>
    <Setter Property="FontWeight" Value="SemiBold"/>
    <Setter Property="Cursor" Value="Hand"/>
  </Style>
  <Style Selector="Button.primary-btn:pointerover /template/ ContentPresenter">
    <Setter Property="Background" Value="#1D4ED8"/>
  </Style>

  <!-- Bottone secondario -->
  <Style Selector="Button.secondary-btn">
    <Setter Property="Background" Value="White"/>
    <Setter Property="Foreground" Value="#374151"/>
    <Setter Property="BorderBrush" Value="#D1D5DB"/>
    <Setter Property="BorderThickness" Value="1"/>
    <Setter Property="Padding" Value="14,8"/>
    <Setter Property="CornerRadius" Value="6"/>
    <Setter Property="Cursor" Value="Hand"/>
  </Style>

  <!-- Hamburger / flat white -->
  <Style Selector="Button.hamburger">
    <Setter Property="Background" Value="Transparent"/>
    <Setter Property="Padding" Value="6"/>
    <Setter Property="Cursor" Value="Hand"/>
  </Style>

  <!-- Nav item sidebar -->
  <Style Selector="Button.nav-item">
    <Setter Property="Background" Value="Transparent"/>
    <Setter Property="Foreground" Value="#CBD5E1"/>
    <Setter Property="HorizontalAlignment" Value="Stretch"/>
    <Setter Property="HorizontalContentAlignment" Value="Left"/>
    <Setter Property="Padding" Value="8,10"/>
    <Setter Property="Cursor" Value="Hand"/>
  </Style>
  <Style Selector="Button.nav-item:pointerover /template/ ContentPresenter">
    <Setter Property="Background" Value="#2D4A6A"/>
  </Style>

  <!-- Chip buttons (AI quick prompts) -->
  <Style Selector="Button.chip-btn">
    <Setter Property="Background" Value="#EFF6FF"/>
    <Setter Property="Foreground" Value="#1D4ED8"/>
    <Setter Property="BorderBrush" Value="#BFDBFE"/>
    <Setter Property="BorderThickness" Value="1"/>
    <Setter Property="Padding" Value="10,4"/>
    <Setter Property="CornerRadius" Value="16"/>
    <Setter Property="FontSize" Value="12"/>
    <Setter Property="Cursor" Value="Hand"/>
  </Style>

  <!-- Chat input -->
  <Style Selector="TextBox.chat-input">
    <Setter Property="MinHeight" Value="44"/>
    <Setter Property="Padding" Value="12,10"/>
    <Setter Property="CornerRadius" Value="8"/>
    <Setter Property="BorderBrush" Value="#D1D5DB"/>
    <Setter Property="BorderThickness" Value="1"/>
    <Setter Property="FontSize" Value="14"/>
  </Style>

</Styles>
```

---

## RIEPILOGO ARCHITETTURA

```
┌─────────────────────────────────────────────────┐
│              LexManager.UI (Avalonia)            │
│  MainWindow (Shell 5 zone)                       │
│  ├─ Zona 1: MenuBar                              │
│  ├─ Zona 2: NavBar Sinistra (collassabile)       │
│  ├─ Zona 3: ContentArea (MVVM ViewModels)        │
│  ├─ Zona 4: RightPanel (contestuale)             │
│  └─ Zona 5: StatusBar                            │
└────────────────────┬────────────────────────────┘
                     │ MediatR (CQRS)
┌────────────────────▼────────────────────────────┐
│           LexManager.Application                 │
│  Commands / Queries / DTOs / Interfaces          │
└────────────────────┬────────────────────────────┘
        ┌────────────┴───────────┐
┌───────▼──────┐        ┌───────▼──────────────────┐
│  Domain      │        │  Infrastructure           │
│  Entities    │        │  EF Core 10 (LocalDB)     │
│  Value Obj.  │        │  Repositories             │
│  Domain Ev.  │        │  OpenAI Service           │
└──────────────┘        │  Serilog → SEQ / File     │
                        └──────────────────────────┘
```

---

*LexManager — Gestionale Avvocati | .NET 10 + C# 14 + Avalonia 11 + EF Core 10 + Serilog/SEQ*
