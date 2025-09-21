
namespace MF.Contracts.Abstractions.Messaging;

// In Phoenix.Abstractions/Messaging/
// 100% PURE - NO MediatR dependency!

public interface ICommand<out TResponse> { }
public interface IQuery<out TResponse> { }

// 我们自己的Mediator接口
public interface IMyMediator
{
    Task<TResponse> Send<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default);
    Task<TResponse> Send<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default);
}

// 我们自己的Handler接口
public interface IMyCommandHandler<in TCommand, TResponse> where TCommand : ICommand<TResponse>
{
    Task<TResponse> Handle(TCommand command, CancellationToken cancellationToken);
}

public interface IMyQueryHandler<in TQuery, TResponse> where TQuery : IQuery<TResponse>
{
    Task<TResponse> Handle(TQuery query, CancellationToken cancellationToken);
}
