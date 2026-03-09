using LexManagerProMax.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LexManagerProMax.Application.UseCases.Anagrafica;

/// <summary>
/// Soft-delete di un soggetto. Non rimuove fisicamente il record dal DB.
/// </summary>
public record DeleteSoggettoCommand(Guid Id) : IRequest;

public class DeleteSoggettoCommandHandler(
    ISoggettoRepository repo,
    ILogger<DeleteSoggettoCommandHandler> logger)
    : IRequestHandler<DeleteSoggettoCommand>
{
    public async Task Handle(DeleteSoggettoCommand cmd, CancellationToken ct)
    {
        await repo.DeleteAsync(cmd.Id, ct);
        logger.LogInformation("Eliminato (soft-delete) soggetto [{Id}]", cmd.Id);
    }
}
