using MediatR;
using ModularGodot.Core.Contracts.Abstractions.Messaging;

namespace ModularGodot.Core.Infrastructure.Messaging;

/// <summary>
/// 查询处理器包装器类
/// 适配IQueryHandler到MediatR的IRequestHandler，提供框架查询处理器接口与MediatR之间的桥梁
/// </summary>
/// <typeparam name="TQuery">查询类型，必须实现IQuery&lt;TResponse&gt;接口</typeparam>
/// <typeparam name="TResponse">响应类型</typeparam>
public class QueryHandlerWrapper<TQuery, TResponse> : IRequestHandler<QueryWrapper<TQuery, TResponse>, TResponse>
    where TQuery : IQuery<TResponse>
{
    private readonly IQueryHandler<TQuery, TResponse> _handler;

    /// <summary>
    /// 初始化查询处理器包装器实例
    /// </summary>
    /// <param name="handler">实际要委托的查询处理器</param>
    public QueryHandlerWrapper(IQueryHandler<TQuery, TResponse> handler) => _handler = handler;

    /// <summary>
    /// 处理查询，通过委托给实际的查询处理器
    /// </summary>
    /// <param name="request">包装的查询请求</param>
    /// <param name="ct">用于协作取消的取消令牌</param>
    /// <returns>来自查询处理器的响应</returns>
    public Task<TResponse> Handle(QueryWrapper<TQuery, TResponse> request, CancellationToken ct)
        => _handler.Handle(request.Query, ct);
}