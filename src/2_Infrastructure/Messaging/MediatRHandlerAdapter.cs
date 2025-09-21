// In Phoenix.Infrastructure.Mediator.MediatR
using MediatR;
using MF.Contracts.Abstractions.Messaging;

namespace MF.Infrastructure.Messaging;

internal class MediatRHandlerAdapter<TMyCommand, TResponse> : IRequestHandler<MediatRRequestAdapter<TResponse>, TResponse>
    where TMyCommand : ICommand<TResponse>
{
    private readonly IMyCommandHandler<TMyCommand, TResponse> _myHandler;

    // DI 注入我们自己�?Handler
    public MediatRHandlerAdapter(IMyCommandHandler<TMyCommand, TResponse> myHandler)
    {
        _myHandler = myHandler;
    }

    public Task<TResponse> Handle(MediatRRequestAdapter<TResponse> requestWrapper, CancellationToken cancellationToken)
    {
        // 从包装器中解开真正的命令对象，然后传递给我们自己的Handler
        return _myHandler.Handle((TMyCommand)requestWrapper.Request, cancellationToken);
    }
}
