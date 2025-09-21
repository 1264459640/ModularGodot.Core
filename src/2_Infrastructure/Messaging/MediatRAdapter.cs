// In Phoenix.Infrastructure.Mediator.MediatR
using MediatR;
using ModularGodot.Contracts.Abstractions.Messaging;

namespace MF.Infrastructure.Messaging;

public class MediatRAdapter : IMyMediator
{
    private readonly IMediator _mediatR;

    public MediatRAdapter(IMediator mediatR)
    {
        _mediatR = mediatR;
    }

    public Task<TResponse> Send<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default)
    {
        // 发送前，用适配器包装我们的命令
        var requestWrapper = new MediatRRequestAdapter<TResponse>(command);
        return _mediatR.Send(requestWrapper, cancellationToken);
    }

    public Task<TResponse> Send<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default)
    {
        // 发送前，用适配器包装我们的查询
        var requestWrapper = new MediatRRequestAdapter<TResponse>(query);
        return _mediatR.Send(requestWrapper, cancellationToken);
    }
}
