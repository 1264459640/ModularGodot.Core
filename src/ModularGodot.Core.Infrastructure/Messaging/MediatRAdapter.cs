using MediatR;
using ModularGodot.Core.Contracts.Abstractions.Messaging;
using ModularGodot.Core.Contracts.Attributes;

namespace ModularGodot.Core.Infrastructure.Messaging;

/// <summary>
/// MediatR适配器类
/// 提供框架命令和查询与MediatR之间的适配功能
/// </summary>
[Injectable(Lifetime.Singleton)]
public class MediatRAdapter : IDispatcher
{
    private readonly IMediator _mediatR;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="mediatR">MediatR中介者实例</param>
    public MediatRAdapter(IMediator mediatR)
    {
        _mediatR = mediatR;
    }

    /// <summary>
    /// 发送命令到相应的处理器
    /// </summary>
    /// <typeparam name="TResponse">命令响应类型</typeparam>
    /// <param name="command">要发送的命令</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>命令处理结果</returns>
    public async Task<TResponse> Send<TResponse>(ICommand<TResponse> command, CancellationToken ct = default)
    {
        try
        {
            // 使用具体的命令类型而不是接口类型
            var wrapper = (IRequest<TResponse>)Activator.CreateInstance(
                typeof(CommandWrapper<,>).MakeGenericType(command.GetType(), typeof(TResponse)),
                command)!;
            return await _mediatR.Send(wrapper, ct);
        }
        catch (Exception ex) when (IsHandlerNotFoundException(ex))
        {
            throw new HandlerNotFoundException($"未找到命令处理器: {command.GetType().FullName}", ex);
        }
    }

    /// <summary>
    /// 发送查询到相应的处理器
    /// </summary>
    /// <typeparam name="TResponse">查询响应类型</typeparam>
    /// <param name="query">要发送的查询</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>查询处理结果</returns>
    public async Task<TResponse> Send<TResponse>(IQuery<TResponse> query, CancellationToken ct = default)
    {
        try
        {
            // 使用具体的查询类型而不是接口类型
            var wrapper = (IRequest<TResponse>)Activator.CreateInstance(
                typeof(QueryWrapper<,>).MakeGenericType(query.GetType(), typeof(TResponse)),
                query)!;
            return await _mediatR.Send(wrapper, ct);
        }
        catch (Exception ex) when (IsHandlerNotFoundException(ex))
        {
            throw new HandlerNotFoundException($"未找到查询处理器: {query.GetType().FullName}", ex);
        }
    }

    /// <summary>
    /// 判断异常是否为处理器未找到异常
    /// </summary>
    /// <param name="ex">要检查的异常</param>
    /// <returns>如果是处理器未找到异常则返回true，否则返回false</returns>
    private static bool IsHandlerNotFoundException(Exception ex)
    {
        // 检查 MediatR 自己的 handler not found 异常
        if (ex is InvalidOperationException && ex.Message.Contains("Handler was not found for request of type"))
        {
            return true;
        }

        // 检查这是否是表示未找到处理程序的依赖项解析异常
        // 我们在不直接引用 Autofac 的情况下检查 Autofac 异常消息中的常见模式
        var exceptionMessage = ex.ToString(); // 使用 ToString() 获取包括内部异常在内的完整异常信息
        return exceptionMessage.Contains("None of the constructors found") ||
               exceptionMessage.Contains("Cannot resolve parameter") ||
               exceptionMessage.Contains("DependencyResolutionException") ||
               exceptionMessage.Contains("no constructors can be invoked");
    }
}
