using MediatR;
using ModularGodot.Contracts.Abstractions.Messaging;

namespace ModularGodot.Infrastructure.Messaging;

/// <summary>
/// Wrapper class that encapsulates a query and implements IRequest for MediatR
/// Provides compile-time type safety through generic constraints
/// </summary>
/// <typeparam name="TQuery">The query type that must implement IQuery&lt;TResponse&gt;</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public class QueryWrapper<TQuery, TResponse> : IRequest<TResponse>
    where TQuery : IQuery<TResponse>
{
    public TQuery Query { get; }

    public QueryWrapper(TQuery query) => Query = query;
}