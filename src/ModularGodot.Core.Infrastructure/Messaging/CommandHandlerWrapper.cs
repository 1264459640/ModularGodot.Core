using MediatR;
using ModularGodot.Core.Contracts.Abstractions.Messaging;

namespace ModularGodot.Core.Infrastructure.Messaging;

/// <summary>
/// 命令处理器包装器类
/// 适配ICommandHandler到MediatR的IRequestHandler，提供框架命令处理器接口与MediatR之间的桥梁
/// </summary>
/// <typeparam name="TCommand">命令类型，必须实现ICommand&lt;TResponse&gt;接口</typeparam>
/// <typeparam name="TResponse">响应类型</typeparam>
public class CommandHandlerWrapper<TCommand, TResponse> : IRequestHandler<CommandWrapper<TCommand, TResponse>, TResponse>
    where TCommand : ICommand<TResponse>
{
    private readonly ICommandHandler<TCommand, TResponse> _handler;

    /// <summary>
    /// 初始化命令处理器包装器实例
    /// </summary>
    /// <param name="handler">实际要委托的命令处理器</param>
    public CommandHandlerWrapper(ICommandHandler<TCommand, TResponse> handler) => _handler = handler;

    /// <summary>
    /// 处理命令，通过委托给实际的命令处理器
    /// </summary>
    /// <param name="request">包装的命令请求</param>
    /// <param name="ct">用于协作取消的取消令牌</param>
    /// <returns>来自命令处理器的响应</returns>
    public Task<TResponse> Handle(CommandWrapper<TCommand, TResponse> request, CancellationToken ct)
        => _handler.Handle(request.Command, ct);
}