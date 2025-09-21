namespace ModularGodot.Contracts.Abstractions.Messaging;

/// <summary>
/// 仅订阅事件的接口，提供订阅功能但不包含发布功能
/// </summary>
public interface IEventSubscriber
{
    /// <summary>
    /// 订阅事件
    /// </summary>
    /// <typeparam name="TEvent">事件类型</typeparam>
    /// <param name="handler">事件处理器</param>
    /// <returns>订阅句柄，用于取消订阅</returns>
    IDisposable Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class;

    /// <summary>
    /// 异步订阅事件
    /// </summary>
    /// <typeparam name="TEvent">事件类型</typeparam>
    /// <param name="handler">异步事件处理器</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>订阅句柄，用于取消订阅</returns>
    IDisposable SubscribeAsync<TEvent>(Func<TEvent, CancellationToken, Task> handler, CancellationToken cancellationToken = default) where TEvent : class;

    /// <summary>
    /// 条件订阅事件
    /// </summary>
    /// <typeparam name="TEvent">事件类型</typeparam>
    /// <param name="filter">过滤条件</param>
    /// <param name="handler">事件处理器</param>
    /// <returns>订阅句柄，用于取消订阅</returns>
    IDisposable SubscribeWhere<TEvent>(Func<TEvent, bool> filter, Action<TEvent> handler) where TEvent : class;

    /// <summary>
    /// 一次性订阅事件
    /// </summary>
    /// <typeparam name="TEvent">事件类型</typeparam>
    /// <param name="handler">事件处理器</param>
    /// <returns>订阅句柄，用于取消订阅</returns>
    IDisposable SubscribeOnce<TEvent>(Action<TEvent> handler) where TEvent : class;
}