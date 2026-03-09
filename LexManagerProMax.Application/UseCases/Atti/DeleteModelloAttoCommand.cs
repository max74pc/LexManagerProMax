using LexManagerProMax.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LexManagerProMax.Application.UseCases.Atti;

public record DeleteModelloAttoCommand(Guid Id) : IRequest;

public class DeleteModelloAttoCommandHandler(
    IModelloAttoRepository repo,
    ILogger<DeleteModelloAttoCommandHandler> logger)
    : IRequestHandler<DeleteModelloAttoCommand>
{
    public async Task Handle(DeleteModelloAttoCommand cmd, CancellationToken ct)
    {
        await repo.DeleteAsync(cmd.Id, ct);
        logger.LogInformation("Eliminato (soft-delete) ModelloAtto [{Id}]", cmd.Id);
    }
}