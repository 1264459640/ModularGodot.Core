using System.Collections.Concurrent;
using ModularGodot.Core.Contracts.Abstractions.Bases;
using ModularGodot.Core.Contracts.Abstractions.Logging;
using ModularGodot.Core.Contracts.Abstractions.Messaging;
using ModularGodot.Core.Contracts.Attributes;
using R3;
using System.Threading;
using System.Linq;

namespace ModularGodot.Core.Infrastructure.Messaging;

/// <summary>
/// Enhanced R3-based event bus implementing the API contract for thread safety, memory leak prevention, and proper resource cleanup
/// </summary>
[Injectable(Lifetime.Singleton)]
public class R3EventBus : BaseInfrastructure, IEventBus
{
    private readonly ConcurrentDictionary<Type, object> _subjects = new();
    private readonly ConcurrentDictionary<string, EventSubscription> _subscriptions = new();
    private readonly ReaderWriterLockSlim _subscriptionsLock = new();
    private readonly IGameLogger _logger;
    private new volatile bool _disposed = false;

    // Performance metrics tracking
    private long _publishCount = 0;
    private long _subscribeCount = 0;
    private long _unsubscribeCount = 0;
    private readonly object _metricsLock = new object();
    private DateTime _lastMetricsReset = DateTime.UtcNow;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="logger">日志记录器</param>
    public R3EventBus(IGameLogger logger)
    {
        _logger = logger;
    }

    #region New Contract-Based Implementation (with Subscription IDs)

    /// <summary>
    /// Subscribe to events of type T with an asynchronous handler, returning a subscription ID for unsubscription
    /// </summary>
    /// <typeparam name="T">Type of event to subscribe to, must inherit from EventBase</typeparam>
    /// <param name="handler">Asynchronous function to execute when event is received</param>
    /// <returns>Unique string identifier for the subscription</returns>
    public async Task<string> Subscribe<T>(Func<T, Task> handler) where T : EventBase
    {
        await ValidateAndCheckDisposedAsync();
        await ValidateEventContractAsync<T>(default(T));

        var subscriptionId = Guid.NewGuid().ToString();

        // Create R3-specific subject if it doesn't exist
        var subjectKey = typeof(T);
        var topicSubject = (TopicSubject<T>)_subjects.GetOrAdd(subjectKey, _ => new TopicSubject<T>());

        // Create the subscription object to track
        IDisposable r3Subscription = topicSubject.GetObservable().Subscribe(async evt =>
        {
            try
            {
                await handler(evt);
            }
            catch (Exception ex)
            {
                // Log error but don't break the entire event flow
                _logger.LogError(ex, "Error in async event handler for: {EventType}", typeof(T).Name);
            }
        });

        var eventSubscription = new EventSubscription(subscriptionId, r3Subscription, false, typeof(T));

        // Add to subscription tracking with thread safety
        _subscriptionsLock.EnterWriteLock();
        try
        {
            _subscriptions[subscriptionId] = eventSubscription;

            // Increment metrics counter for thread safety
            Interlocked.Increment(ref _subscribeCount);
        }
        finally
        {
            _subscriptionsLock.ExitWriteLock();
        }

        _logger.LogInformation("Subscribed to event type {EventType} with ID {SubscriptionId}", typeof(T).Name, subscriptionId);
        return subscriptionId;
    }

    /// <summary>
    /// Subscribe to the first occurrence of events of type T, then automatically unsubscribe
    /// </summary>
    /// <typeparam name="T">Type of event to subscribe to, must inherit from EventBase</typeparam>
    /// <param name="handler">Asynchronous function to execute when event is received</param>
    /// <returns>Unique string identifier for the subscription</returns>
    public async Task<string> SubscribeOnce<T>(Func<T, Task> handler) where T : EventBase
    {
        await ValidateAndCheckDisposedAsync();
        await ValidateEventContractAsync<T>(default(T));

        var subscriptionId = Guid.NewGuid().ToString();

        var subjectKey = typeof(T);
        var topicSubject = (TopicSubject<T>)_subjects.GetOrAdd(subjectKey, _ => new TopicSubject<T>());

        // Create the one-time subscription - use Take(1) to get only the first event
        IDisposable r3Subscription = topicSubject.GetObservable().Take(1).Subscribe(async evt =>
        {
            try
            {
                await handler(evt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in one-time event handler for: {EventType}", typeof(T).Name);
            }
            finally
            {
                // Automatically dispose of the subscription after the first event
                await Unsubscribe(subscriptionId);
            }
        });

        var eventSubscription = new EventSubscription(subscriptionId, r3Subscription, true, typeof(T));

        // Add to subscription tracking
        _subscriptionsLock.EnterWriteLock();
        try
        {
            _subscriptions[subscriptionId] = eventSubscription;

            // Increment metrics counter for thread safety
            Interlocked.Increment(ref _subscribeCount);
        }
        finally
        {
            _subscriptionsLock.ExitWriteLock();
        }

        _logger.LogInformation("Subscribed to event type {EventType} once with ID {SubscriptionId}", typeof(T).Name, subscriptionId);
        return subscriptionId;
    }

    /// <summary>
    /// Unsubscribe using the subscription identifier
    /// </summary>
    /// <param name="subscriptionId">Unique identifier returned when subscription was created</param>
    public async Task Unsubscribe(string subscriptionId)
    {
        await ValidateAndCheckDisposedAsync();

        if (string.IsNullOrEmpty(subscriptionId))
        {
            return; // Safe to ignore null/empty IDs
        }

        _subscriptionsLock.EnterWriteLock();
        try
        {
            if (_subscriptions.TryGetValue(subscriptionId, out var subscription))
            {
                // We dispose the subscription which cleans up underlying resources
                subscription.Dispose(); // This will dispose of the R3 subscription
                _subscriptions.TryRemove(subscriptionId, out _);
                _logger.LogInformation("Unsubscribed subscription ID {SubscriptionId}", subscriptionId);

                // Increment metrics counter for thread safety
                Interlocked.Increment(ref _unsubscribeCount);
            }
        }
        finally
        {
            _subscriptionsLock.ExitWriteLock();
        }
    }

    // Internal async publish method for contract tests and new API (renamed to avoid conflicts)
    internal async Task PublishAsyncForContract<T>(T @event) where T : EventBase
    {
        await ValidateAndCheckDisposedAsync();
        await ValidateEventContractAsync(@event);

        if (@event == null)
        {
            throw new ArgumentNullException(nameof(@event));
        }

        // Increment metrics counter for thread safety
        Interlocked.Increment(ref _publishCount);

        var subjectKey = typeof(T);

        // Look up the subject for this event type
        if (_subjects.TryGetValue(subjectKey, out var subjectObj))
        {
            var topicSubject = subjectObj as TopicSubject<T>;

            if (topicSubject != null && !topicSubject.Disposed)
            {
                try
                {
                    topicSubject.OnNext(@event);
                }
                catch (ObjectDisposedException)
                {
                    // If the subject is disposed, remove it from our dictionary and log
                    _subjects.TryRemove(subjectKey, out _);
                    _logger.LogWarning("Attempted to publish to disposed subject for type {EventType}", typeof(T).Name);
                }
            }
        }
        // If no subject exists, it means there are no subscribers - this is normal behavior
    }

    #endregion

    #region Thread-Safe Legacy Methods (for backward compatibility)

    /// <summary>
    /// Asynchronously Publish event (original interface)
    /// </summary>
    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : EventBase
    {
        await ValidateAndCheckDisposedAsync();
        await ValidateEventContractAsync(@event);

        await PublishAsyncForContract(@event);
    }

    /// <summary>
    /// Synchronously publish event (original interface)
    /// </summary>
    public void Publish<TEvent>(TEvent @event) where TEvent : EventBase
    {
        ValidateAndCheckDisposed();
        ValidateEventContract(@event);

        // Run the async version synchronously to maintain consistency
        PublishAsyncForContract(@event).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Subscribe to event (original interface)
    /// </summary>
    public IDisposable Subscribe<TEvent>(Action<TEvent> handler) where TEvent : EventBase
    {
        ValidateAndCheckDisposed();
        // Map to the task-based version
        var taskHandler = new Func<TEvent, Task>(t =>
        {
            handler(t);
            return Task.CompletedTask;
        });

        // Since this returns the old-style IDisposable that can be used directly
        // We'll create the subscription and return a wrapper IDisposable
        string subscriptionId = Subscribe(taskHandler).GetAwaiter().GetResult();
        return new SubscriptionWrapper(async () => await Unsubscribe(subscriptionId));
    }

    /// <summary>
    /// Async subscribe to event (original interface)
    /// </summary>
    public IDisposable Subscribe<TEvent>(Func<TEvent, CancellationToken, Task> handler, CancellationToken cancellationToken = default) where TEvent : EventBase
    {
        ValidateAndCheckDisposed();
        // Map to the task-based version without needing to pass the cancellation token through
        Func<TEvent, Task> taskHandler = async t =>
        {
            await handler(t, cancellationToken);
        };

        string subscriptionId = Subscribe(taskHandler).GetAwaiter().GetResult();
        return new SubscriptionWrapper(async () => await Unsubscribe(subscriptionId));
    }

    /// <summary>
    /// Conditional subscribe (original interface)
    /// </summary>
    public IDisposable Subscribe<TEvent>(Func<TEvent, bool> filter, Action<TEvent> handler) where TEvent : EventBase
    {
        ValidateAndCheckDisposed();
        Func<TEvent, Task> taskHandler = async t =>
        {
            if (filter(t))
            {
                handler(t);
            }
        };

        string subscriptionId = Subscribe(taskHandler).GetAwaiter().GetResult();
        return new SubscriptionWrapper(async () => await Unsubscribe(subscriptionId));
    }

    /// <summary>
    /// One-time subscribe (original interface)
    /// </summary>
    public IDisposable SubscribeOnce<TEvent>(Action<TEvent> handler) where TEvent : EventBase
    {
        ValidateAndCheckDisposed();
        Func<TEvent, Task> taskHandler = async t =>
        {
            handler(t);
        };

        string subscriptionId = SubscribeOnce(taskHandler).GetAwaiter().GetResult();
        return new SubscriptionWrapper(async () => await Unsubscribe(subscriptionId));
    }

    #endregion

    #region Validation Methods

    /// <summary>
    /// Validates an event before publishing according to the contract
    /// </summary>
    private async Task ValidateEventContractAsync<T>(T eventObj) where T : EventBase
    {
        // Would validate according to our contract but since this is for internal use
        // just letting async task return for consistency with signature
        if (eventObj is EventBase e)
        {
            // Basic validation for contract adherence
            if (string.IsNullOrEmpty(e.EventId))
            {
                _logger.LogWarning("Publishing event with empty EventId: {EventType}", typeof(T).Name);
            }

            var timeThreshold = DateTime.UtcNow.AddHours(-1); // Reasonable time check
            if (e.Timestamp < timeThreshold || e.Timestamp > DateTime.UtcNow.AddHours(1))
            {
                _logger.LogWarning("Event timestamp is outside reasonable range: {EventType}", typeof(T).Name);
            }

            if (string.IsNullOrWhiteSpace(e.Source))
            {
                _logger.LogWarning("Event source is null or empty: {EventType}", typeof(T).Name);
            }
        }
        await Task.Yield(); // Just for async consistency
    }

    private void ValidateEventContract<T>(T eventObj) where T : EventBase
    {
        // Blocking version of the same validation
        if (eventObj is EventBase e)
        {
            if (string.IsNullOrEmpty(e.EventId))
            {
                _logger.LogWarning("Publishing event with empty EventId: {EventType}", typeof(T).Name);
            }

            var timeThreshold = DateTime.UtcNow.AddHours(-1); // Reasonable time check
            if (e.Timestamp < timeThreshold || e.Timestamp > DateTime.UtcNow.AddHours(1))
            {
                _logger.LogWarning("Event timestamp is outside reasonable range: {EventType}", typeof(T).Name);
            }

            if (string.IsNullOrWhiteSpace(e.Source))
            {
                _logger.LogWarning("Event source is null or empty: {EventType}", typeof(T).Name);
            }
        }
    }

    private async Task ValidateAndCheckDisposedAsync()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(R3EventBus));
        await Task.Yield(); // async consistency
    }

    private void ValidateAndCheckDisposed()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(R3EventBus));
    }

    #endregion

    /// <summary>
    /// Releases resources and cleans up subscription resources
    /// </summary>
    /// <param name="disposing"></param>
    protected override void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            _logger.LogInformation("Disposing R3EventBus and cleaning up resources");

            _subscriptionsLock.EnterWriteLock();
            try
            {
                // Dispose all tracked subscriptions
                foreach (var kvp in _subscriptions)
                {
                    try
                    {
                        kvp.Value.Dispose();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning("Error disposing subscription {SubscriptionId}: {ErrorMessage}", kvp.Key, ex.Message);
                    }
                }

                _subscriptions.Clear();
            }
            finally
            {
                _subscriptionsLock.ExitWriteLock();
            }

            // Dispose all subjects
            foreach (var subject in _subjects.Values)
            {
                try
                {
                    (subject as IDisposable)?.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Error disposing subject: {ErrorMessage}", ex.Message);
                }
            }

            _subjects.Clear();
            _subscriptionsLock?.Dispose();
        }

        _disposed = true;
        base.Dispose(disposing);
    }

    #region Private Helper Classes
    // Helper for legacy interface compatibility
    private class SubscriptionWrapper : IDisposable
    {
        private readonly Func<Task> _unsubscribeAction;
        private bool _disposed = false;

        public SubscriptionWrapper(Func<Task> unsubscribeAction)
        {
            _unsubscribeAction = unsubscribeAction;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                try {
                    // Run unsubscribe synchronously to completion using ConfigureAwait(false) to prevent deadlocks
                    _unsubscribeAction().ConfigureAwait(false).GetAwaiter().GetResult();
                }
                catch
                {
                    // If anything goes wrong during unsubscribe, we still mark as disposed
                }
                _disposed = true;
            }
        }
    }
    #endregion
}
