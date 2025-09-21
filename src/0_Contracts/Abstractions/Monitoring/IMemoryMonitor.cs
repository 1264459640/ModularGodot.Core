namespace ModularGodot.Contracts.Abstractions.Monitoring;

/// <summary>
/// 内存监控接口
/// </summary>
public interface IMemoryMonitor
{
    /// <summary>
    /// 内存压力检测事�?    /// </summary>
    event Action<long>? MemoryPressureDetected;
    
    /// <summary>
    /// 自动释放触发事件
    /// </summary>
    event Action? AutoReleaseTriggered;
    
    /// <summary>
    /// 自动释放阈值（字节�?    /// </summary>
    long AutoReleaseThreshold { get; set; }
    
    /// <summary>
    /// 检查间�?    /// </summary>
    TimeSpan CheckInterval { get; set; }
    
    /// <summary>
    /// 内存压力阈�?    /// </summary>
    double MemoryPressureThreshold { get; set; }
    
    /// <summary>
    /// 开始监�?    /// </summary>
    void StartMonitoring();
    
    /// <summary>
    /// 停止监控
    /// </summary>
    void StopMonitoring();
    
    /// <summary>
    /// 检查内存压�?    /// </summary>
    /// <param name="currentUsage">当前内存使用�?/param>
    void CheckMemoryPressure(long currentUsage);
    
    /// <summary>
    /// 获取当前内存使用�?    /// </summary>
    /// <returns>当前内存使用量（字节�?/returns>
    long GetCurrentMemoryUsage();
    
    /// <summary>
    /// 强制垃圾回收
    /// </summary>
    void ForceGarbageCollection();
}
