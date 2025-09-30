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
        // 使用具体的命令类型而不是接口类型
        var wrapper = (IRequest<TResponse>)Activator.CreateInstance(
            typeof(CommandWrapper<,>).MakeGenericType(command.GetType(), typeof(TResponse)),
            command);
        return _mediatR.Send(wrapper, ct);
    }

    public Task<TResponse> Send<TResponse>(IQuery<TResponse> query, CancellationToken ct = default)
    {
        // 使用具体的查询类型而不是接口类型
        var wrapper = (IRequest<TResponse>)Activator.CreateInstance(
            typeof(QueryWrapper<,>).MakeGenericType(query.GetType(), typeof(TResponse)),
            query);
        return _mediatR.Send(wrapper, ct);
    }
}
