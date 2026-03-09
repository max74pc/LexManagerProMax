using LexManagerProMax.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LexManagerProMax.Application.UseCases.AI;

public record DeleteSessioneAICommand(Guid Id) : IRequest;

public class DeleteSessioneAICommandHandler(
    ISessioneAIRepository repo,
    ILogger<DeleteSessioneAICommandHandler> logger)
    : IRequestHandler<DeleteSessioneAICommand>
{
    public async Task Handle(DeleteSessioneAICommand cmd, CancellationToken ct)
    {
        await repo.DeleteAsync(cmd.Id, ct);
        logger.LogInformation(
            "Eliminata (soft-delete) SessioneAI [{Id}]", cmd.Id);
    }
}