using System.Collections.Concurrent;
using ModularGodot.Core.Contracts.Abstractions.Bases;
using ModularGodot.Core.Contracts.Abstractions.Logging;
using ModularGodot.Core.Contracts.Abstractions.Messaging;
using ModularGodot.Core.Contracts.Attributes;
using R3;
using ObservableExtensions = System.ObservableExtensions;

namespace ModularGodot.Core.Infrastructure.Messaging;

/// <summary>
/// 基于R3的事件总线实现类
/// 提供事件发布和订阅功能，支持同步和异步操作
/// </summary>
[Injectable(Lifetime.Singleton)]
public class R3EventBus : BaseInfrastructure, IEventBus
{
    private readonly ConcurrentDictionary<Type, System.Reactive.Subjects.Subject<object>> _subjects = new();
    private readonly CompositeDisposable _disposables = new();
    private readonly IGameLogger _logger;
    private readonly object _lock = new();
    
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="logger">日志记录器</param>
    public R3EventBus(IGameLogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _logger.LogInformation("R3EventBus initialized");
    }
    
    /// <summary>
    /// 异步发布事件
    /// </summary>
    /// <typeparam name="TEvent">事件类型</typeparam>
    /// <param name="event">要发布的事件实例</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>异步任务</returns>
    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : EventBase
    {
        if (IsDisposed)
        {
            _logger.LogWarning("Attempted to publish event on disposed EventBus: {EventType}", typeof(TEvent).Name);
            return;
        }

        try
        {
            _logger.LogDebug("Publishing event asynchronously: {EventType}, EventId: {EventId}", typeof(TEvent).Name, @event.EventId);

            var subject = GetOrCreateSubject<TEvent>();
            await Task.Run(() => subject.OnNext(@event), cancellationToken);

            _logger.LogDebug("Event published successfully: {EventType}", typeof(TEvent).Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish event: {EventType}, EventId: {EventId}", typeof(TEvent).Name, @event.EventId);
            throw;
        }
    }
    
    /// <summary>
    /// 同步发布事件
    /// </summary>
    /// <typeparam name="TEvent">事件类型</typeparam>
    /// <param name="event">要发布的事件实例</param>
    public void Publish<TEvent>(TEvent @event) where TEvent : EventBase
    {
        if (IsDisposed)
        {
            _logger.LogWarning("Attempted to publish event on disposed EventBus: {EventType}", typeof(TEvent).Name);
            return;
        }

        try
        {
            _logger.LogDebug("Publishing event synchronously: {EventType}, EventId: {EventId}", typeof(TEvent).Name, @event.EventId);

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
    
    /// <summary>
    /// 订阅事件
    /// </summary>
    /// <typeparam name="TEvent">事件类型</typeparam>
    /// <param name="handler">事件处理函数</param>
    /// <returns>订阅句柄，用于取消订阅</returns>
    public IDisposable Subscribe<TEvent>(Action<TEvent> handler) where TEvent : EventBase
    {
        CheckDisposed();

        try
        {
            _logger.LogDebug("Subscribing to event: {EventType}", typeof(TEvent).Name);

            var subject = GetOrCreateSubject<TEvent>();
            var subscription = ObservableExtensions.Subscribe<object>(subject, evt =>
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
    
    /// <summary>
    /// 异步订阅事件
    /// </summary>
    /// <typeparam name="TEvent">事件类型</typeparam>
    /// <param name="handler">异步事件处理函数</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>订阅句柄，用于取消订阅</returns>
    public IDisposable Subscribe<TEvent>(Func<TEvent, CancellationToken, Task> handler, CancellationToken cancellationToken = default) where TEvent : EventBase
    {
        return Subscribe<TEvent>(evt =>
        {
            // 异步处理，但不阻塞事件发布
            _ = Task.Run(async () =>
            {
                try
                {
                    await handler(evt, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in async event handler for: {EventType}", typeof(TEvent).Name);
                }
            }, cancellationToken);
        });
    }
    
    /// <summary>
    /// 条件订阅事件
    /// </summary>
    /// <typeparam name="TEvent">事件类型</typeparam>
    /// <param name="filter">事件过滤条件</param>
    /// <param name="handler">事件处理函数</param>
    /// <returns>订阅句柄，用于取消订阅</returns>
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
    
    /// <summary>
    /// 一次性订阅事件
    /// </summary>
    /// <typeparam name="TEvent">事件类型</typeparam>
    /// <param name="handler">事件处理函数</param>
    /// <returns>订阅句柄，用于取消订阅</returns>
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


    
    /// <summary>
    /// 获取或创建事件主题
    /// </summary>
    /// <typeparam name="TEvent">事件类型</typeparam>
    /// <returns>事件主题</returns>
    private System.Reactive.Subjects.Subject<object> GetOrCreateSubject<TEvent>() where TEvent : EventBase
    {
        var eventType = typeof(TEvent);
        return _subjects.GetOrAdd(eventType, _ =>
        {
            var subject = new System.Reactive.Subjects.Subject<object>();
            _disposables.Add(subject);
            _logger.LogDebug("Created new subject for event type: {EventType}", eventType.Name);
            return subject;
        });
    }
    
    /// <summary>
    /// 释放事件总线资源
    /// </summary>
    /// <param name="disposing">是否释放托管资源</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _logger.LogInformation("Disposing R3EventBus");

            _disposables.Dispose();
            foreach (var subject in _subjects.Values)
            {
                subject.Dispose();
            }
            _subjects.Clear();

            _logger.LogInformation("R3EventBus disposed");
        }

        base.Dispose(disposing);
    }
}
