using MediatR;

namespace ModularGodot.Infrastructure.Messaging;

public class QueryWrapper<TQuery,TResponse> : IRequest<TResponse>
{
    public TQuery Query { get; }
    public QueryWrapper(TQuery query) => Query = query;
}