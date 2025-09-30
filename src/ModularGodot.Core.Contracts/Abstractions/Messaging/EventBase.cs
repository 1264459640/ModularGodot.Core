namespace ModularGodot.Contracts.Abstractions.Messaging;

/// <summary>
/// 事件基类
/// </summary>
public abstract class EventBase
{
    /// <summary>
    /// 事件ID
    /// </summary>
    public string EventId { get; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// 事件时间�?    /// </summary>
    public DateTime Timestamp { get; } = DateTime.UtcNow;
    
    /// <summary>
    /// 事件�?    /// </summary>
    public virtual string Source { get; protected set; } = "Unknown";
    
    /// <summary>
    /// 关联ID
    /// </summary>
    public string CorrelationId { get; set; } = string.Empty;

    /// <summary>
    /// 事件基类
    /// </summary>
    /// <param name="source">事件�?/param>
    protected EventBase(string? source = null)
    {
        Source = source ?? GetType().Name;
    }
}
