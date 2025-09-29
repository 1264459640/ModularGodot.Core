// In Phoenix.Infrastructure.Mediator.MediatR
using MediatR;
using ModularGodot.Contracts.Abstractions.Messaging;
using ModularGodot.Contracts.Attributes;

namespace ModularGodot.Infrastructure.Messaging;

public class MediatRAdapter : IDispatcher
{
    private readonly IMediator _mediatR;

    public MediatRAdapter(IMediator mediatR)
    {
        _mediatR = mediatR;
    }

    public Task<TResponse> Send<TResponse>(ICommand<TResponse> command, CancellationToken ct = default)
    {
        var wrapper = new CommandWrapper<ICommand<TResponse>, TResponse>(command);
        return _mediatR.Send(wrapper, ct);
    }
    public Task<TResponse> Send<TResponse>(IQuery<TResponse> query, CancellationToken ct = default)
    {
        var wrapper = new QueryWrapper<IQuery<TResponse>, TResponse>(query);
        return _mediatR.Send(wrapper, ct);
    }
}
