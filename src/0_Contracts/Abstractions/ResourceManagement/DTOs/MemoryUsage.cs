namespace ModularGodot.Contracts.Abstractions.ResourceManagement.DTOs;

/// <summary>
/// 内存使用情况
/// </summary>
public class MemoryUsage
{
    /// <summary>
    /// 当前内存使用量（字节�?
    /// </summary>
    public long CurrentUsage { get; set; }
    
    /// <summary>
    /// 最大内存使用量（字节）
    /// </summary>
    public long MaxUsage { get; set; }
    
    /// <summary>
    /// 内存使用百分�?
    /// </summary>
    public double UsagePercentage { get; set; }
    
    /// <summary>
    /// 可用内存（字节）
    /// </summary>
    public long AvailableMemory { get; set; }
    
    /// <summary>
    /// 垃圾回收次数
    /// </summary>
    public int GCCollectionCount { get; set; }
    
    /// <summary>
    /// 最后检查时�?
    /// </summary>
    public DateTime LastChecked { get; set; } = DateTime.UtcNow;
}
