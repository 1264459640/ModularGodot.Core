
namespace ModularGodot.Contracts.Abstractions.Messaging;

// In Phoenix.Abstractions/Messaging/
// 100% PURE - NO MediatR dependency!

public interface ICommand<out TResponse> { }
public interface IQuery<out TResponse> { }

// 调度器接口
public interface IDispatcher
{
    Task<TResponse> Send<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default);
    Task<TResponse> Send<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default);
}

// 我们自己的Handler接口
public interface ICommandHandler<in TCommand, TResponse> where TCommand : ICommand<TResponse>
{
    Task<TResponse> Handle(TCommand command, CancellationToken cancellationToken);
}

public interface IQueryHandler<in TQuery, TResponse> where TQuery : IQuery<TResponse>
{
    Task<TResponse> Handle(TQuery query, CancellationToken cancellationToken);
}
