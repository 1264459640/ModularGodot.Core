using ModularGodot.Contracts.Abstractions.Messaging;

namespace ModularGodot.Contracts.Events.ResourceManagement;

/// <summary>
/// 内存压力事件
/// </summary>
public class MemoryPressureEvent : EventBase
{
    /// <summary>
    /// 当前内存使用量（字节�?
    /// </summary>
    public long CurrentMemoryUsage { get; }
    
    /// <summary>
    /// 内存使用百分�?
    /// </summary>
    public double UsagePercentage { get; }
    
    /// <summary>
    /// 压力级别
    /// </summary>
    public MemoryPressureLevel PressureLevel { get; }
    
    /// <summary>
    /// 构造函�?
    /// </summary>
    /// <param name="currentMemoryUsage">当前内存使用�?/param>
    /// <param name="usagePercentage">使用百分�?/param>
    /// <param name="pressureLevel">压力级别</param>
    public MemoryPressureEvent(long currentMemoryUsage, double usagePercentage, MemoryPressureLevel pressureLevel)
        : base("ResourceManager")
    {
        CurrentMemoryUsage = currentMemoryUsage;
        UsagePercentage = usagePercentage;
        PressureLevel = pressureLevel;
    }
}

/// <summary>
/// 内存压力级别
/// </summary>
public enum MemoryPressureLevel
{
    /// <summary>
    /// 正常
    /// </summary>
    Normal,
    
    /// <summary>
    /// 警告
    /// </summary>
    Warning,
    
    /// <summary>
    /// 严重
    /// </summary>
    Critical
}
