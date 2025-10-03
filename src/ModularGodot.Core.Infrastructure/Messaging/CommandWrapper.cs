using MediatR;
using ModularGodot.Contracts.Abstractions.Messaging;

namespace ModularGodot.Infrastructure.Messaging;

/// <summary>
/// Wrapper class that encapsulates a command and implements IRequest for MediatR
/// Provides compile-time type safety through generic constraints
/// </summary>
/// <typeparam name="TCommand">The command type that must implement ICommand&lt;TResponse&gt;</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public class CommandWrapper<TCommand, TResponse> : IRequest<TResponse>
    where TCommand : ICommand<TResponse>
{
    public TCommand Command { get; }

    public CommandWrapper(TCommand command) => Command = command;
}
