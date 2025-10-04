using MediatR;
using ModularGodot.Core.Contracts.Abstractions.Messaging;

namespace ModularGodot.Core.Infrastructure.Messaging;

/// <summary>
/// 命令包装器类
/// 封装命令并实现MediatR的IRequest接口，通过泛型约束提供编译时类型安全
/// </summary>
/// <typeparam name="TCommand">命令类型，必须实现ICommand&lt;TResponse&gt;接口</typeparam>
/// <typeparam name="TResponse">响应类型</typeparam>
public class CommandWrapper<TCommand, TResponse> : IRequest<TResponse>
    where TCommand : ICommand<TResponse>
{
    /// <summary>
    /// 获取包装的命令实例
    /// </summary>
    public TCommand Command { get; }

    /// <summary>
    /// 初始化命令包装器实例
    /// </summary>
    /// <param name="command">要包装的命令实例</param>
    public CommandWrapper(TCommand command) => Command = command;
}
