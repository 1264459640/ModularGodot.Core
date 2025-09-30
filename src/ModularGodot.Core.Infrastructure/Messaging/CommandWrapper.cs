// In Phoenix.Infrastructure.Mediator.MediatR
using MediatR;

namespace ModularGodot.Infrastructure.Messaging;

// Этот класс-обертка инкапсулирует нашу команду и реализует IRequest от MediatR
// Этот класс-обертка инкапсулирует нашу команду и реализует IRequest от MediatR
public class CommandWrapper<TCommand,TResponse> : IRequest<TResponse>
{
    public TCommand Command { get; }
    public CommandWrapper(TCommand command) => Command = command;
}
