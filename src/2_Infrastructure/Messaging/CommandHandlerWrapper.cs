using Autofac;
using MediatR;
using ModularGodot.Contracts.Abstractions.Messaging;

namespace ModularGodot.Infrastructure.Messaging
{
    public class MediatRHandlerAdapter<TResponse> : IRequestHandler<MediatRRequestAdapter<TResponse>, TResponse>
    {
        private readonly IComponentContext _context;

        public MediatRHandlerAdapter(IComponentContext context)
        {
            _context = context;
        }

        public Task<TResponse> Handle(MediatRRequestAdapter<TResponse> requestWrapper, CancellationToken cancellationToken)
        {
            var request = requestWrapper.Request;
            var requestType = request.GetType();
            
            dynamic handler;
            if (request is ICommand<TResponse>)
            {
                var handlerType = typeof(ICommandHandler<,>).MakeGenericType(requestType, typeof(TResponse));
                handler = _context.Resolve(handlerType);
            }
            else if (request is IQuery<TResponse>)
            {
                var handlerType = typeof(IQueryHandler<,>).MakeGenericType(requestType, typeof(TResponse));
                handler = _context.Resolve(handlerType);
            }
            else
            {
                throw new System.ArgumentException($"Unsupported request type: {requestType.Name}");
            }

            return handler.Handle((dynamic)request, cancellationToken);
        }
    }
}
