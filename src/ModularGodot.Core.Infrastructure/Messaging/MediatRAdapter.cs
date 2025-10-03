using MediatR;
using ModularGodot.Contracts.Abstractions.Messaging;
using ModularGodot.Contracts.Attributes;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ModularGodot.Infrastructure.Messaging;

[Injectable(Lifetime.Singleton)]
public class MediatRAdapter : IDispatcher
{
    private readonly IMediator _mediatR;

    public MediatRAdapter(IMediator mediatR)
    {
        _mediatR = mediatR;
    }

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
            throw new HandlerNotFoundException($"No handler found for command: {command.GetType().FullName}", ex);
        }
    }

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
            throw new HandlerNotFoundException($"No handler found for query: {query.GetType().FullName}", ex);
        }
    }

    private static bool IsHandlerNotFoundException(Exception ex)
    {
        // Check if this is a dependency resolution exception indicating handler not found
        // We check for common patterns in Autofac exception messages without directly referencing Autofac
        var exceptionMessage = ex.ToString(); // Use ToString() to get full exception info including inner exceptions
        return exceptionMessage.Contains("None of the constructors found") ||
               exceptionMessage.Contains("Cannot resolve parameter") ||
               exceptionMessage.Contains("DependencyResolutionException") ||
               exceptionMessage.Contains("no constructors can be invoked");
    }
}
