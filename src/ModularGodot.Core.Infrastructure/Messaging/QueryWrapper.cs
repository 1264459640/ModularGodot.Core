using MediatR;
using ModularGodot.Core.Contracts.Abstractions.Messaging;

namespace ModularGodot.Core.Infrastructure.Messaging;

/// <summary>
/// 查询包装器类
/// 封装查询并实现MediatR的IRequest接口，通过泛型约束提供编译时类型安全
/// </summary>
/// <typeparam name="TQuery">查询类型，必须实现IQuery&lt;TResponse&gt;接口</typeparam>
/// <typeparam name="TResponse">响应类型</typeparam>
public class QueryWrapper<TQuery, TResponse> : IRequest<TResponse>
    where TQuery : IQuery<TResponse>
{
    /// <summary>
    /// 获取包装的查询实例
    /// </summary>
    public TQuery Query { get; }

    /// <summary>
    /// 初始化查询包装器实例
    /// </summary>
    /// <param name="query">要包装的查询实例</param>
    public QueryWrapper(TQuery query) => Query = query;
}