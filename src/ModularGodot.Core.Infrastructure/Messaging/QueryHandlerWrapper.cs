using MediatR;
using ModularGodot.Contracts.Abstractions.Messaging;

namespace ModularGodot.Infrastructure.Messaging;

/// <summary>
/// Wrapper class that adapts IQueryHandler to MediatR's IRequestHandler
/// Provides a bridge between the framework's query handler interface and MediatR
/// </summary>
/// <typeparam name="TQuery">The query type that must implement IQuery&lt;TResponse&gt;</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public class QueryHandlerWrapper<TQuery, TResponse> : IRequestHandler<QueryWrapper<TQuery, TResponse>, TResponse>
    where TQuery : IQuery<TResponse>
{
    private readonly IQueryHandler<TQuery, TResponse> _handler;

    /// <summary>
    /// Initializes a new instance of the QueryHandlerWrapper class
    /// </summary>
    /// <param name="handler">The actual query handler to delegate to</param>
    public QueryHandlerWrapper(IQueryHandler<TQuery, TResponse> handler) => _handler = handler;

    /// <summary>
    /// Handles the query by delegating to the actual query handler
    /// </summary>
    /// <param name="request">The wrapped query request</param>
    /// <param name="ct">Cancellation token for cooperative cancellation</param>
    /// <returns>The response from the query handler</returns>
    public Task<TResponse> Handle(QueryWrapper<TQuery, TResponse> request, CancellationToken ct)
        => _handler.Handle(request.Query, ct);
}