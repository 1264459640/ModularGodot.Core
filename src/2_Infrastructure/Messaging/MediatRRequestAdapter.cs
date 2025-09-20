// In Phoenix.Infrastructure.Mediator.MediatR
using MediatR;
using MF.Contracts.Infrs.Messaging;

namespace MF.Infrastructure.Messaging;

// Этот класс-обертка инкапсулирует нашу команду и реализует IRequest от MediatR
// Этот класс-обертка инкапсулирует нашу команду и реализует IRequest от MediatR
internal class MediatRRequestAdapter<TResponse> : IRequest<TResponse>
{
    // Он содержит НАСТОЯЩУЮ команду/запрос
    public object Request { get; }

    public MediatRRequestAdapter(object request)
    {
        // Убедитесь, что это либо ICommand, либо IQuery
        if (request is not ICommand<TResponse> && request is not IQuery<TResponse>)
        {
            throw new ArgumentException("Request must be an ICommand or IQuery.", nameof(request));
        }
        Request = request;
    }
}