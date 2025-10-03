using MediatR;
using ModularGodot.Contracts.Abstractions.Messaging;

namespace ModularGodot.Infrastructure.Messaging;

/// <summary>
/// Wrapper class that adapts ICommandHandler to MediatR's IRequestHandler
/// Provides a bridge between the framework's command handler interface and MediatR
/// </summary>
/// <typeparam name="TCommand">The command type that must implement ICommand&lt;TResponse&gt;</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public class CommandHandlerWrapper<TCommand, TResponse> : IRequestHandler<CommandWrapper<TCommand, TResponse>, TResponse>
    where TCommand : ICommand<TResponse>
{
    private readonly ICommandHandler<TCommand, TResponse> _handler;

    /// <summary>
    /// Initializes a new instance of the CommandHandlerWrapper class
    /// </summary>
    /// <param name="handler">The actual command handler to delegate to</param>
    public CommandHandlerWrapper(ICommandHandler<TCommand, TResponse> handler) => _handler = handler;

    /// <summary>
    /// Handles the command by delegating to the actual command handler
    /// </summary>
    /// <param name="request">The wrapped command request</param>
    /// <param name="ct">Cancellation token for cooperative cancellation</param>
    /// <returns>The response from the command handler</returns>
    public Task<TResponse> Handle(CommandWrapper<TCommand, TResponse> request, CancellationToken ct)
        => _handler.Handle(request.Command, ct);
}