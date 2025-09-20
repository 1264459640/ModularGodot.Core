namespace MF.Contracts.Abstractions.ResourceManagement.DTOs;

/// <summary>
/// 资源系统配置
/// </summary>
public class ResourceSystemConfig
{
    /// <summary>
    /// 最大内存大小（字节�?
    /// </summary>
    public long MaxMemorySize { get; set; } = 100 * 1024 * 1024; // 100MB
    
    /// <summary>
    /// 默认过期时间
    /// </summary>
    public TimeSpan DefaultExpiration { get; set; } = TimeSpan.FromHours(1);
    
    /// <summary>
    /// 内存压力阈值（0.0-1.0�?
    /// </summary>
    public double MemoryPressureThreshold { get; set; } = 0.8; // 80%
    
    /// <summary>
    /// 清理间隔
    /// </summary>
    public TimeSpan CleanupInterval { get; set; } = TimeSpan.FromMinutes(5);
    
    /// <summary>
    /// 是否启用自动清理
    /// </summary>
    public bool EnableAutoCleanup { get; set; } = true;
    
    /// <summary>
    /// 是否启用性能监控
    /// </summary>
    public bool EnablePerformanceMonitoring { get; set; } = true;
    
    /// <summary>
    /// 最大缓存项数量
    /// </summary>
    public int MaxCacheItems { get; set; } = 1000;
}
