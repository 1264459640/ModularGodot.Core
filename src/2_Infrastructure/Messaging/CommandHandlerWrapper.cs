using MediatR;
using ModularGodot.Contracts.Abstractions.Messaging;

namespace ModularGodot.Infrastructure.Messaging;

public class CommandHandlerWrapper<TCommand, TResponse> : IRequestHandler<CommandWrapper<TCommand, TResponse>, TResponse>
    where TCommand : ICommand<TResponse>
{
    private readonly ICommandHandler<TCommand, TResponse> _handler;
    public CommandHandlerWrapper(ICommandHandler<TCommand, TResponse> handler) => _handler = handler;
    public Task<TResponse> Handle(CommandWrapper<TCommand, TResponse> request, CancellationToken ct)
        => _handler.Handle(request.Command, ct);
}