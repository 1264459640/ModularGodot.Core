
namespace ModularGodot.Core.Contracts.Abstractions.Messaging;

// In Phoenix.Abstractions/Messaging/
// 100% PURE - NO MediatR dependency!

/// <summary>
/// 命令接口，用于定义返回指定类型响应的命令
/// </summary>
/// <typeparam name="TResponse">命令执行后返回的响应类型</typeparam>
public interface ICommand<out TResponse> { }

/// <summary>
/// 查询接口，用于定义返回指定类型响应的查询
/// </summary>
/// <typeparam name="TResponse">查询执行后返回的响应类型</typeparam>
public interface IQuery<out TResponse> { }

/// <summary>
/// 调度器接口，用于发送命令和查询到相应的处理器
/// </summary>
public interface IDispatcher
{
    /// <summary>
    /// 发送命令到相应的命令处理器
    /// </summary>
    /// <typeparam name="TResponse">命令响应的类型</typeparam>
    /// <param name="command">要发送的命令</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>命令执行后的响应结果</returns>
    Task<TResponse> Send<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default);

    /// <summary>
    /// 发送查询到相应的查询处理器
    /// </summary>
    /// <typeparam name="TResponse">查询响应的类型</typeparam>
    /// <param name="query">要发送的查询</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>查询执行后的响应结果</returns>
    Task<TResponse> Send<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default);
}

/// <summary>
/// 命令处理器接口，用于处理指定类型的命令
/// </summary>
/// <typeparam name="TCommand">要处理的命令类型</typeparam>
/// <typeparam name="TResponse">命令处理后返回的响应类型</typeparam>
public interface ICommandHandler<in TCommand, TResponse> where TCommand : ICommand<TResponse>
{
    /// <summary>
    /// 处理指定的命令
    /// </summary>
    /// <param name="command">要处理的命令</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>命令处理后的响应结果</returns>
    Task<TResponse> Handle(TCommand command, CancellationToken cancellationToken);
}

/// <summary>
/// 查询处理器接口，用于处理指定类型的查询
/// </summary>
/// <typeparam name="TQuery">要处理的查询类型</typeparam>
/// <typeparam name="TResponse">查询处理后返回的响应类型</typeparam>
public interface IQueryHandler<in TQuery, TResponse> where TQuery : IQuery<TResponse>
{
    /// <summary>
    /// 处理指定的查询
    /// </summary>
    /// <param name="query">要处理的查询</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>查询处理后的响应结果</returns>
    Task<TResponse> Handle(TQuery query, CancellationToken cancellationToken);
}
