using System.Collections.Concurrent;
using ModularGodot.Core.Contracts.Abstractions.Logging;
using ModularGodot.Core.Contracts.Abstractions.Messaging;
using R3;

namespace ModularGodot.Core.Infrastructure.Messaging;

/// <summary>
/// 基于R3的事件总线实现
/// </summary>
public class R3EventBus : IEventBus, IDisposable
{
    private readonly ConcurrentDictionary<Type, Subject<object>> _subjects = new();
    private readonly CompositeDisposable _disposables = new();

    private bool _disposed;
    private readonly IGameLogger _logger;
    private readonly object _lock = new();
    
    /// <summary>
    /// 基于R3的事件总线实现
    /// </summary>
    /// <param name="logger"></param>
    public R3EventBus(IGameLogger logger)
    {
        _logger = logger;
        
        _logger.LogInformation("R3EventBus initialized");
    }
    
    public void Publish<TEvent>(TEvent @event) where TEvent : EventBase
    {
        if (_disposed)
        {
            _logger.LogWarning("Attempted to publish event on disposed EventBus: {EventType}", typeof(TEvent).Name);
            return;
        }
        
        try
        {
            var subject = GetOrCreateSubject<TEvent>();
            subject.OnNext(@event);
            
            _logger.LogDebug("Event published successfully: {EventType}", typeof(TEvent).Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish event: {EventType}, EventId: {EventId}", typeof(TEvent).Name, @event.EventId);
            throw;
        }
    }
    
    public IDisposable Subscribe<TEvent>(Action<TEvent> handler) where TEvent : EventBase
    {
        if (_disposed)
        {
            _logger.LogWarning("Attempted to subscribe event on disposed EventBus: {EventType}", typeof(TEvent).Name);
            return Disposable.Empty;
        }
        
        try
        {
            _logger.LogDebug("Subscribing to event: {EventType}", typeof(TEvent).Name);
            
            var subject = GetOrCreateSubject<TEvent>();
            var subscription = subject.Subscribe(evt =>
            {
                try
                {
                    handler((TEvent)evt);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in event handler for: {EventType}", typeof(TEvent).Name);
                    // 不抛出异常，避免影响其他订阅者
                }
            });
            
            _logger.LogDebug("Subscribed to event: {EventType}", typeof(TEvent).Name);
            return subscription;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to subscribe to event: {EventType}", typeof(TEvent).Name);
            throw;
        }
    }
    
    
    public IDisposable Subscribe<TEvent>(Func<TEvent, bool> filter, Action<TEvent> handler) where TEvent : EventBase
    {
        return Subscribe<TEvent>(evt =>
        {
            if (filter(evt))
            {
                handler(evt);
            }
        });
    }
    
    public IDisposable SubscribeOnce<TEvent>(Action<TEvent> handler) where TEvent : EventBase
    {
        IDisposable? subscription = null;
        subscription = Subscribe<TEvent>(evt =>
        {
            try
            {
                handler(evt);
            }
            finally
            {
                subscription?.Dispose();
            }
        });
        return subscription;
    }
    

    
    private Subject<object> GetOrCreateSubject<TEvent>() where TEvent : EventBase
    {
        var eventType = typeof(TEvent);
        return _subjects.GetOrAdd(eventType, _ =>
        {
            var subject = new Subject<object>();
            _disposables.Add(subject);
            _logger.LogDebug("Created new subject for event type: {EventType}", eventType.Name);
            return subject;
        });
    }
    

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        lock (_lock)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _logger.LogInformation("Disposing R3EventBus");

                // CompositeDisposable handles disposing all added subjects.
                _disposables.Dispose();
                _subjects.Clear();

                _logger.LogInformation("R3EventBus disposed");
            }

            _disposed = true;
        }
    }
}