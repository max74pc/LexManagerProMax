//using LexManagerProMax.Application.Interfaces;
//using LexManagerProMax.Domain.Entities.Anagrafica;
//using MediatR;
//using Microsoft.Extensions.Logging;

//namespace LexManagerProMax.Application.UseCases.Anagrafica;

//public record CreateModelloAttoCommand(
//    TipoSoggetto Tipo,
//    string? Cognome,
//    string? Nome,
//    string? CodiceFiscale,
//    string? RagioneSociale,
//    string? PartitaIva,
//    string? Email,
//    string? Pec,
//    string? Telefono,
//    bool IsCliente = true) : IRequest<Soggetto>;

//public class CreateSoggettoCommandHandler(
//    ISoggettoRepository repo,
//    ILogger<CreateSoggettoCommandHandler> logger,
//    TipoSoggetto Tipo,
//    string Cognome)
//    : IRequestHandler<CreateModelloAttoCommand, Soggetto>
//{
//    public async Task<Soggetto> Handle(CreateModelloAttoCommand cmd, CancellationToken ct)
//    {
//        Soggetto soggetto = cmd.Tipo == TipoSoggetto.PersonaFisica
//            ? Soggetto.CreaPersonaFisica(cmd.Cognome!, cmd.Nome!, cmd.CodiceFiscale, null, cmd.Email, cmd.Pec, cmd.Telefono)
//            : Soggetto.CreaPersonaGiuridica(cmd.RagioneSociale!, cmd.PartitaIva, null, cmd.Email, cmd.Pec);

//        soggetto.ImpostaRuolo(cmd.IsCliente, false);

//        var result = await repo.AddAsync(soggetto, ct);
//        logger.LogInformation("Creato soggetto {NomeCompleto} [{Id}]", result.NomeCompleto, result.Id);
//        return result;
//    }

//  }

using LexManagerProMax.Application.Interfaces;
using LexManagerProMax.Domain.Entities.Anagrafica;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LexManagerProMax.Application.UseCases.Anagrafica;

/// <summary>
/// Crea un nuovo Soggetto (persona fisica o giuridica) e lo persiste.
/// </summary>
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
    bool IsCliente = true
) : IRequest<Soggetto>;

public class CreateSoggettoCommandHandler(
    ISoggettoRepository repo,
    ILogger<CreateSoggettoCommandHandler> logger)
    : IRequestHandler<CreateSoggettoCommand, Soggetto>
{
    public async Task<Soggetto> Handle(
        CreateSoggettoCommand cmd,
        CancellationToken ct)
    {
        Soggetto soggetto = cmd.Tipo == TipoSoggetto.PersonaFisica
            ? Soggetto.CreaPersonaFisica(
                cmd.Cognome!,
                cmd.Nome!,
                cmd.CodiceFiscale,
                dataNascita: null,
                cmd.Email,
                cmd.Pec,
                cmd.Telefono)
            : Soggetto.CreaPersonaGiuridica(
                cmd.RagioneSociale!,
                cmd.PartitaIva,
                formaGiuridica: null,
                cmd.Email,
                cmd.Pec);

        soggetto.ImpostaRuolo(cmd.IsCliente, isControparte: false);

        var result = await repo.AddAsync(soggetto, ct);

        logger.LogInformation(
            "Creato soggetto {NomeCompleto} [{Id}]",
            result.NomeCompleto, result.Id);

        return result;
    }
}
