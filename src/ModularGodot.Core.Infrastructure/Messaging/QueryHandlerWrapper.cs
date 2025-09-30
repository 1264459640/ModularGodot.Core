using MediatR;
using ModularGodot.Contracts.Abstractions.Messaging;

namespace ModularGodot.Infrastructure.Messaging;

public class QueryHandlerWrapper<TQuery, TResponse> : IRequestHandler<QueryWrapper<TQuery, TResponse>, TResponse>
    where TQuery : IQuery<TResponse>
{
    private readonly IQueryHandler<TQuery, TResponse> _handler;
    public QueryHandlerWrapper(IQueryHandler<TQuery, TResponse> handler) => _handler = handler;
    public Task<TResponse> Handle(QueryWrapper<TQuery, TResponse> request, CancellationToken ct)
        => _handler.Handle(request.Query, ct);
}